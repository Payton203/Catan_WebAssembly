"""Deterministic, audited base-Catan example for the app's seed 543456789.

Run: python3 tools/generate_example_game.py
The fixture was exported using the app's .NET Random and board assignment code.
Base game only: expansion, cities, all development cards and both special awards.
Rules: https://www.catan.com/sites/default/files/2021-06/catan_base_rules_2020_200707.pdf
Strategy: https://blog.colonist.io/guide-to-catan-starting-strategies/
"""
from collections import Counter, deque
from copy import deepcopy
from datetime import datetime, timedelta, timezone
from pathlib import Path
import json
import random

ROOT = Path(__file__).resolve().parents[1]
SEED = 543456789
NAMES = ['Affre', 'Gimenez', 'Monteagudo', 'Macheta']
RES = ['Wood', 'Brick', 'Sheep', 'Wheat', 'Rock']
CELLS = json.loads((ROOT / 'tools/fixtures/board-543456789.json').read_text())
COSTS = {'road': Counter(Wood=1, Brick=1),
         'settlement': Counter(Wood=1, Brick=1, Sheep=1, Wheat=1),
         'city': Counter(Wheat=2, Rock=3),
         'development': Counter(Wheat=1, Rock=1, Sheep=1)}
PIPS = {2: 1, 3: 2, 4: 3, 5: 4, 6: 5, 8: 5, 9: 4, 10: 3, 11: 2, 12: 1}
# Same traversal and insertion order as CatanOverlay; integral lattice avoids
# screen-size-dependent rounding. IDs are zero-based, hex IDs are one-based.
VERTICES, EDGES, HEXES = [], [], []
for row, cols in enumerate([3, 4, 5, 4, 3]):
    for col in range(cols):
        cx, cy = 5 - cols + 2 * col + 1, 2 + 3 * row
        ids = []
        for dx, dy in [(0, -2), (1, -1), (1, 1), (0, 2), (-1, 1), (-1, -1)]:
            point = (cx + dx, cy + dy)
            if point not in VERTICES:
                VERTICES.append(point)
            ids.append(VERTICES.index(point))
        HEXES.append(ids)
        for i in range(6):
            edge = tuple(sorted((ids[i], ids[(i + 1) % 6])))
            if edge not in EDGES:
                EDGES.append(edge)
assert len(VERTICES) == 54 and len(EDGES) == 72
ADJ = {v: [] for v in range(54)}
for e, (a, b) in enumerate(EDGES):
    ADJ[a].append((b, e)); ADJ[b].append((a, e))
TILES = {v: [i for i, vs in enumerate(HEXES) if v in vs] for v in range(54)}
PORTS = {}
for h, e, kind in [(0, 5, 'Sheep'), (1, 0, 'Any'), (3, 4, 'Any'),
                   (6, 0, 'Rock'), (11, 1, 'Any'), (12, 4, 'Wheat'),
                   (15, 2, 'Brick'), (16, 3, 'Any'), (17, 2, 'Wood')]:
    for v in [HEXES[h][e], HEXES[h][(e+1) % 6]]:
        PORTS[v] = kind


class Game:
    def __init__(self):
        self.hands = [Counter({r: 0 for r in RES}) for _ in NAMES]
        self.bank = Counter({r: 19 for r in RES})
        self.buildings = {}  # vertex -> (player, production/points)
        self.roads = {}
        self.robber = next(i for i, c in enumerate(CELLS) if c['Recurso'] == 'Desert')
        self.step = 0
        self.turn = 0
        self.active = 0
        self.longest = None
        self.rolled = False
        self.last_roll = None
        self.last_setup = None
        self.pending_production = {}
        self.pending_discards = set()
        self.pending_robber = False
        self.pending_theft = set()
        self.events = []
        self.winner = None
        self.deck = ['Caballero'] * 14 + ['Punto_Victoria'] * 5 + ['Carreteras', 'Invento', 'Monopolio'] * 2
        random.Random(SEED + 1).shuffle(self.deck)
        self.cards = [[] for _ in NAMES]
        self.played = [Counter() for _ in NAMES]
        self.dev_played = False
        self.free_roads = 0
        self.army = None

    def points(self, p):
        return self.public_points(p) + sum(kind == 'Punto_Victoria' for kind, _ in self.cards[p])

    def public_points(self, p):
        return (sum(n for owner, n in self.buildings.values() if owner == p) +
                2 * (self.longest == p) + 2 * (self.army == p) + self.played[p]['Punto_Victoria'])

    def valid_site(self, v):
        return v not in self.buildings and all(n not in self.buildings for n, _ in ADJ[v])

    def counts(self, p, level):
        return sum(owner == p and n == level for owner, n in self.buildings.values())

    def production(self, p):
        out = Counter()
        for v, (owner, level) in self.buildings.items():
            if owner == p:
                for h in TILES[v]:
                    c = CELLS[h]
                    out[c['Recurso']] += PIPS.get(c['Label'], 0) * level
        return out

    def site_score(self, p, v):
        prod = self.production(p)
        weights = [dict(Wood=1.1, Brick=1.1, Sheep=.85, Wheat=1.35, Rock=1.5),
                   dict(Wood=1.4, Brick=1.4, Sheep=.9, Wheat=1.2, Rock=1.1),
                   dict(Wood=1.1, Brick=1.1, Sheep=.9, Wheat=1.4, Rock=1.5),
                   dict(Wood=1.3, Brick=1.3, Sheep=1, Wheat=1.2, Rock=1.25)][p]
        score = 0
        for h in TILES[v]:
            c = CELLS[h]; r = c['Recurso']
            score += PIPS.get(c['Label'], 0) * weights.get(r, 0) * (1.6 if prod[r] == 0 else 1)
        if v in PORTS:
            score += 2 if PORTS[v] == 'Any' else prod[PORTS[v]] * .4
        return score

    def road_length(self, p):
        def walk(v, used):
            if used and v in self.buildings and self.buildings[v][0] != p:
                return len(used)
            return max([len(used)] + [walk(n, used | {e}) for n, e in ADJ[v]
                                     if self.roads.get(e) == p and e not in used])
        return max(walk(v, set()) for v in ADJ)

    def rate(self, p, resource):
        ports = {PORTS[v] for v, (owner, _) in self.buildings.items() if owner == p and v in PORTS}
        return 2 if resource in ports else 3 if 'Any' in ports else 4

    def transfer_bank(self, p, delta):
        for r, n in delta.items():
            assert r in RES
            self.hands[p][r] += n
            self.bank[r] -= n

    def apply(self, e):
        """Replay-time rule assertions independent of move selection heuristics."""
        assert self.winner is None
        p = NAMES.index(e['Jugador']); kind = e['Tipo']
        offered = Counter(e.get('Recursos_Ofrecidos', {}))
        received = Counter(e.get('Recursos_Recibidos', {}))
        assert all(n > 0 for n in [*offered.values(), *received.values()])
        if kind not in ['Produccion_Recursos', 'Descarte_Por_Siete', 'Obtener_Gran_Ruta', 'Obtener_Ejercito']:
            assert p == self.active, (kind, p, self.active)
        if kind in ['Construccion_Estructura', 'Construccion_Carretera', 'Comercio_Banca', 'Comercio_Jugadores', 'Comprar_Carta_Desarrollo', 'Jugar_Carta_Desarrollo'] and self.step >= 8:
            assert self.rolled and not (self.pending_production or self.pending_discards or self.pending_robber or self.pending_theft)
        if kind == 'Construccion_Estructura':
            v = e['Id_Posicion_pieza']; level = e['Estructura_Vertice']
            if level == 'Settlement':
                assert self.valid_site(v) and self.counts(p, 1) < 5
                if self.step >= 8:
                    assert any(self.roads.get(edge) == p for _, edge in ADJ[v])
                    self.transfer_bank(p, Counter({r: -n for r, n in COSTS['settlement'].items()}))
                else:
                    assert self.last_setup is None
                    self.last_setup = v
                self.buildings[v] = p, 1
            else:
                assert self.step >= 8 and self.buildings.get(v) == (p, 1) and self.counts(p, 2) < 4
                self.transfer_bank(p, Counter({r: -n for r, n in COSTS['city'].items()}))
                self.buildings[v] = p, 2
        elif kind == 'Construccion_Carretera':
            edge = e['Id_Posicion_pieza']; a, b = EDGES[edge]
            assert edge not in self.roads and list(self.roads.values()).count(p) < 15
            if self.step < 8:
                assert self.last_setup in [a, b]
            else:
                assert any((self.buildings.get(v, (-1, 0))[0] == p or
                            (v not in self.buildings and any(self.roads.get(k) == p for _, k in ADJ[v]))) for v in [a, b])
                if self.free_roads:
                    self.free_roads -= 1
                else:
                    self.transfer_bank(p, Counter({r: -n for r, n in COSTS['road'].items()}))
            self.roads[edge] = p
        elif kind == 'Produccion_Recursos':
            if self.step < 8:
                assert self.step >= 4 and p == self.active
                expected = Counter(CELLS[h]['Recurso'] for h in TILES[self.last_setup] if CELLS[h]['Recurso'] != 'Desert')
                assert received == expected
            else:
                assert received == self.pending_production.pop(p)
            self.transfer_bank(p, received)
        elif kind == 'Tirada_Dados':
            assert self.step >= 8 and not self.rolled
            self.rolled = True
            d1, d2 = e['Dado1'], e['Dado2']
            assert 1 <= d1 <= 6 and 1 <= d2 <= 6 and d1 + d2 == e['Resultado_Dados']
            self.last_roll = d1 + d2
            if self.last_roll == 7:
                self.pending_discards = {i for i, hand in enumerate(self.hands) if sum(hand.values()) > 7}
                self.pending_robber = True
            else:
                demand = [Counter() for _ in NAMES]
                for v, (owner, level) in self.buildings.items():
                    for h in TILES[v]:
                        if h != self.robber and CELLS[h]['Label'] == self.last_roll:
                            demand[owner][CELLS[h]['Recurso']] += level
                for r in RES:
                    recipients = [i for i in range(4) if demand[i][r]]
                    if sum(x[r] for x in demand) > self.bank[r]:
                        for i in recipients:
                            demand[i][r] = self.bank[r] if len(recipients) == 1 else 0
                self.pending_production = {i: +d for i, d in enumerate(demand) if +d}
        elif kind == 'Descarte_Por_Siete':
            assert p in self.pending_discards and sum(offered.values()) == sum(self.hands[p].values()) // 2
            self.pending_discards.remove(p)
            self.transfer_bank(p, Counter({r: -n for r, n in offered.items()}))
        elif kind == 'Mover_Ladron':
            h = e['Id_Posicion_pieza'] - 1
            assert self.pending_robber and not self.pending_discards and h != self.robber and 0 <= h < 19
            self.robber = h; self.pending_robber = False
            self.pending_theft = {owner for v, (owner, _) in self.buildings.items()
                                  if v in HEXES[h] and owner != p and sum(self.hands[owner].values()) > 0}
        elif kind == 'Robar_Recurso':
            target = NAMES.index(e['Jugador_Objetivo'])
            assert target in self.pending_theft and sum(received.values()) == 1
            self.hands[target].subtract(received); self.hands[p].update(received)
            self.pending_theft.clear()
        elif kind == 'Comercio_Banca':
            assert len(offered) == len(received) == 1
            r, n = next(iter(offered.items())); wanted, qty = next(iter(received.items()))
            assert r != wanted and n == self.rate(p, r) * qty
            self.transfer_bank(p, Counter({r: -n, wanted: qty}))
        elif kind == 'Comercio_Jugadores':
            target = NAMES.index(e['Jugador_Objetivo'])
            assert target != p and offered and received and not (offered.keys() & received.keys())
            self.hands[p].subtract(offered); self.hands[target].update(offered)
            self.hands[target].subtract(received); self.hands[p].update(received)
        elif kind == 'Obtener_Gran_Ruta':
            lengths = [self.road_length(i) for i in range(4)]
            assert lengths[p] >= 5 and lengths[p] == max(lengths) and self.longest != p
            assert self.longest is None or lengths[p] > lengths[self.longest]
            self.longest = p
        elif kind == 'Comprar_Carta_Desarrollo':
            assert self.deck and e['Tipo_Carta_Desarrollo'] == self.deck[0]
            self.transfer_bank(p, Counter({r: -n for r, n in COSTS['development'].items()}))
            self.cards[p].append((self.deck.pop(0), self.turn))
        elif kind == 'Jugar_Carta_Desarrollo':
            card = e['Tipo_Carta_Desarrollo']
            available = [(k,t) for k,t in self.cards[p] if k == card and (t < self.turn or k == 'Punto_Victoria')]
            assert available and (not self.dev_played or card == 'Punto_Victoria')
            if card == 'Punto_Victoria':
                assert self.points(p) >= 10
            else:
                self.dev_played = True
            self.cards[p].remove(available[0]); self.played[p][card] += 1
            if card == 'Caballero':
                self.pending_robber = True
            elif card == 'Carreteras':
                assert list(self.roads.values()).count(p) < 15
                self.free_roads = 2
            elif card == 'Invento':
                assert sum(received.values()) == 2 and all(self.bank[r] >= n for r,n in received.items())
                self.pending_production = {p:received}
            elif card == 'Monopolio':
                assert len(received) == 1
                r = next(iter(received))
                for q in range(4):
                    if q != p:
                        self.hands[p][r] += self.hands[q][r]; self.hands[q][r] = 0
        elif kind == 'Obtener_Ejercito':
            count = self.played[p]['Caballero']
            assert count >= 3 and self.army != p and (self.army is None or count > self.played[self.army]['Caballero'])
            self.army = p
        elif kind == 'Fin_De_Turno':
            if self.step < 8:
                assert self.last_setup is not None and any(self.roads.get(k) == p for _, k in ADJ[self.last_setup])
                self.step += 1; self.last_setup = None
                self.active = [0, 1, 2, 3, 3, 2, 1, 0, 0][self.step]
                if self.step == 8:
                    self.turn = 1
            else:
                assert self.rolled and not (self.pending_production or self.pending_discards or self.pending_robber or self.pending_theft)
                assert self.points(p) < 10
                self.active = (p + 1) % 4; self.turn += 1; self.rolled = False
                self.dev_played = False; self.free_roads = 0
        elif kind == 'Victoria':
            assert self.step >= 8 and self.points(p) >= 10 and e['Puntos_Victoria'] == self.points(p)
            self.winner = p
        else:
            raise AssertionError(kind)
        for r in RES:
            assert self.bank[r] >= 0 and all(h[r] >= 0 for h in self.hands), (kind, r, self.hands)
            assert self.bank[r] + sum(h[r] for h in self.hands) == 19

    def emit(self, kind, p=None, comment='', **fields):
        e = dict(Tipo=kind, Jugador=NAMES[self.active if p is None else p], Turno=self.turn,
                 Momento=(datetime(2026, 10, 28, 18, tzinfo=timezone(timedelta(hours=-3))) + timedelta(seconds=len(self.events)*12)).isoformat(),
                 Comentario=comment, **fields)
        self.apply(e)
        self.events.append(e)

    def paths(self, p):
        starts = {v for v, (owner, _) in self.buildings.items() if owner == p}
        for e, owner in self.roads.items():
            if owner == p:
                starts.update(v for v in EDGES[e] if v not in self.buildings or self.buildings[v][0] == p)
        found = {v: [] for v in starts}; todo = deque(sorted(starts))
        while todo:
            v = todo.popleft()
            if v in self.buildings and self.buildings[v][0] != p:
                continue
            for n, e in ADJ[v]:
                if e not in self.roads and n not in found:
                    found[n] = found[v] + [e]; todo.append(n)
        return [(v, path) for v, path in found.items() if self.valid_site(v)
                # Avoid cutting roads: this example exercises awards/transfers,
                # but deliberately never revokes a title without a new owner.
                and not any(self.roads.get(e, p) != p for _, e in ADJ[v])]

    def bank_plan(self, p, cost):
        hand, bank, trades = self.hands[p].copy(), self.bank.copy(), []
        for wanted in RES:
            while hand[wanted] < cost[wanted]:
                options = [r for r in RES if r != wanted and hand[r] - cost[r] >= self.rate(p, r)]
                if not options or bank[wanted] == 0:
                    return None
                r = max(options, key=lambda r: (hand[r]-cost[r])/self.rate(p, r))
                n = self.rate(p, r); hand[r] -= n; hand[wanted] += 1; bank[r] += n; bank[wanted] -= 1
                trades.append((r, n, wanted))
        return trades

    def pay_trades(self, trades):
        for r, n, wanted in trades:
            self.emit('Comercio_Banca', Recursos_Ofrecidos={r:n}, Recursos_Recibidos={wanted:1},
                      comment=f'Canje {n}:1 para completar el coste de construcción.')

    def award_road(self):
        lengths = [self.road_length(i) for i in range(4)]
        best = max(lengths)
        if best < 5 or (self.longest is not None and lengths[self.longest] == best):
            return
        leaders = [p for p in range(4) if lengths[p] == best]
        if len(leaders) == 1:
            self.emit('Obtener_Gran_Ruta', leaders[0], comment=f'Camino continuo de {best} segmentos; el título vale 2 puntos.')

    def victory(self):
        if self.points(self.active) >= 10:
            for kind, _ in list(self.cards[self.active]):
                if kind == 'Punto_Victoria':
                    self.emit('Jugar_Carta_Desarrollo', Tipo_Carta_Desarrollo=kind, comment='Revela el punto oculto únicamente al declarar la victoria.')
            self.emit('Victoria', Puntos_Victoria=self.points(self.active), comment='Diez puntos en su propio turno: termina la partida.')
            return True
        return False

    def resolve_robber(self, rng):
        p = self.active
        def block_score(h):
            return sum((self.public_points(owner)+2) * level * PIPS.get(CELLS[h]['Label'],0) * (1 if owner != p else -3)
                       for v,(owner,level) in self.buildings.items() if v in HEXES[h])
        h = max((h for h in range(19) if h != self.robber), key=block_score)
        self.emit('Mover_Ladron', Id_Posicion_pieza=h+1, comment='Bloquea producción rival, priorizando al jugador más adelantado.')
        if self.pending_theft:
            target = max(self.pending_theft, key=lambda q: (self.public_points(q),sum(self.hands[q].values())))
            r = rng.choice(list(self.hands[target].elements()))
            self.emit('Robar_Recurso', Jugador_Objetivo=NAMES[target], Recursos_Recibidos={r:1}, comment='Robo aleatorio de una carta a un rival adyacente.')

    def play_development(self, rng):
        p = self.active
        options = {kind for kind, turn in self.cards[p] if turn < self.turn and kind != 'Punto_Victoria'}
        if self.dev_played or not options:
            return
        # Don't play a card merely to consume it: each selected effect has utility.
        blocked = any(owner == p and self.robber in TILES[v] for v,(owner,_) in self.buildings.items())
        if 'Caballero' in options and (blocked or self.army != p):
            self.emit('Jugar_Carta_Desarrollo', Tipo_Carta_Desarrollo='Caballero', comment='Libera producción o disputa el ejército más grande; carta comprada en un turno anterior.')
            self.resolve_robber(rng)
            count = self.played[p]['Caballero']
            if count >= 3 and (self.army is None or count > self.played[self.army]['Caballero']):
                self.emit('Obtener_Ejercito', comment=f'{count} caballeros jugados: obtiene los 2 puntos de ejército más grande.')
        elif 'Monopolio' in options:
            # Estimate by production; do not inspect opponents' hidden resource types.
            r = max(RES, key=lambda r: sum(self.production(q)[r] for q in range(4) if q != p))
            self.emit('Jugar_Carta_Desarrollo', Tipo_Carta_Desarrollo='Monopolio', Recursos_Recibidos={r:1},
                      comment='Elige el recurso con mayor producción rival estimada; recoge todas esas cartas.')
        elif 'Invento' in options and sum(self.bank.values()) >= 2:
            goal = COSTS['city'] if self.counts(p,1) and self.counts(p,2)<4 else COSTS['settlement']
            received = Counter()
            for _ in range(2):
                r = max((r for r in RES if self.bank[r] > received[r]),
                        key=lambda r: (goal[r] - self.hands[p][r] - received[r], r in ['Rock','Wheat']))
                received[r] += 1
            self.emit('Jugar_Carta_Desarrollo', Tipo_Carta_Desarrollo='Invento', Recursos_Recibidos=dict(received),
                      comment='Elige dos recursos disponibles para completar una construcción.')
            self.emit('Produccion_Recursos', Recursos_Recibidos=dict(received), comment='Dos cartas de recurso obtenidas por Invento.')
        elif 'Carreteras' in options and any(path for _,path in self.paths(p)) and list(self.roads.values()).count(p) < 15:
            self.emit('Jugar_Carta_Desarrollo', Tipo_Carta_Desarrollo='Carreteras', comment='Construye hasta dos carreteras conectadas sin pagar madera ni ladrillo.')
            for _ in range(2):
                paths = [(v,path) for v,path in self.paths(p) if path]
                if not paths or list(self.roads.values()).count(p) >= 15:
                    break
                v,path = max(paths,key=lambda vp:self.site_score(p,vp[0])/(1+len(vp[1])))
                self.emit('Construccion_Carretera', Id_Posicion_pieza=path[0], comment='Carretera gratuita de la carta de desarrollo.')
                self.award_road()


def generate():
    game = Game(); rng = random.Random(SEED + 42)
    placements = []
    for _ in range(8):
        p = game.active
        v = max((v for v in ADJ if game.valid_site(v)), key=lambda v: game.site_score(p, v))
        placements.append(dict(Jugador=NAMES[p], Vertice=v, Casillas=[CELLS[h]['Id'] for h in TILES[v]],
                               Pips=sum(PIPS.get(CELLS[h]['Label'], 0) for h in TILES[v])))
        game.emit('Construccion_Estructura', Id_Posicion_pieza=v, Estructura_Vertice='Settlement',
                  comment='Colocación inicial: producción probable y recursos complementarios.')
        # Point the free road toward a productive legal third settlement.
        choices = []
        for n, edge in ADJ[v]:
            if edge in game.roads:
                continue
            future = [game.site_score(p, dest) for dest, _ in ADJ[n] if game.valid_site(dest)]
            choices.append((max(future, default=0), edge))
        edge = max(choices)[1]
        game.emit('Construccion_Carretera', Id_Posicion_pieza=edge, comment='Carretera gratuita unida al poblado recién colocado.')
        if game.step >= 4:
            resources = Counter(CELLS[h]['Recurso'] for h in TILES[v] if CELLS[h]['Recurso'] != 'Desert')
            if resources:
                game.emit('Produccion_Recursos', Recursos_Recibidos=resources, comment='Recursos iniciales: una carta por hexágono del segundo poblado.')
        game.emit('Fin_De_Turno')
    setup_count = len(game.events)
    while game.winner is None and game.turn <= 400:
        p = game.active
        d1, d2 = rng.randint(1,6), rng.randint(1,6)
        game.emit('Tirada_Dados', Dado1=d1, Dado2=d2, Resultado_Dados=d1+d2)
        for owner, resources in list(game.pending_production.items()):
            game.emit('Produccion_Recursos', owner, Recursos_Recibidos=dict(resources), comment='Producción exacta del tablero; el ladrón bloquea su hexágono.')
        for owner in sorted(game.pending_discards):
            hand = game.hands[owner].copy(); discard = Counter()
            for _ in range(sum(hand.values())//2):
                r = max(RES, key=lambda r: hand[r] / (2 if r in ['Wheat','Rock'] else 1))
                hand[r] -= 1; discard[r] += 1
            game.emit('Descarte_Por_Siete', owner, Recursos_Ofrecidos=dict(discard), comment='Más de siete cartas: descarta la mitad, redondeando hacia abajo.')
        if game.pending_robber:
            game.resolve_robber(rng)
        if game.victory():
            break
        game.play_development(rng)
        if game.victory():
            break
        for action in range(12):
            sites = game.paths(p)
            candidates = []
            if game.deck:
                trades = game.bank_plan(p, COSTS['development'])
                if trades is not None:
                    # Cities first when productive; cards are efficient when
                    # expansion is blocked or army/hidden points can finish.
                    score = 9 + (6 if game.points(p)>=7 else 0) + (3 if not sites else 0)
                    candidates.append((score-len(trades)*1.5,'development',-1,trades))
            for v, (owner, level) in game.buildings.items():
                if owner == p and level == 1 and game.counts(p,2) < 4:
                    trades = game.bank_plan(p, COSTS['city'])
                    if trades is not None:
                        score = sum(PIPS.get(CELLS[h]['Label'],0) for h in TILES[v]) + 5
                        candidates.append((score-len(trades)*1.5, 'city', v, trades))
            if game.counts(p,1) < 5:
                for v, path in sites:
                    if not path:
                        trades = game.bank_plan(p, COSTS['settlement'])
                        if trades is not None:
                            candidates.append((game.site_score(p,v)+4-len(trades)*1.5,'settlement',v,trades))
                    elif list(game.roads.values()).count(p) < 15:
                        trades = [] if game.free_roads else game.bank_plan(p, COSTS['road'])
                        if trades is not None:
                            candidates.append((game.site_score(p,v)/(1+len(path)) -len(trades), 'road',path[0],trades))
            if candidates:
                _, kind, position, trades = max(candidates)
                game.pay_trades(trades)
                if kind == 'development':
                    game.emit('Comprar_Carta_Desarrollo', Tipo_Carta_Desarrollo=game.deck[0],
                              comment='Paga mineral, trigo y lana; roba la siguiente carta del mazo barajado.')
                elif kind == 'road':
                    game.emit('Construccion_Carretera', Id_Posicion_pieza=position, comment='Expansión conectada hacia un lugar disponible para poblar.')
                else:
                    game.emit('Construccion_Estructura', Id_Posicion_pieza=position,
                              Estructura_Vertice='City' if kind == 'city' else 'Settlement',
                              comment='Duplica producción y suma un punto.' if kind == 'city' else 'Nuevo poblado respetando distancia y conexión.')
                game.award_road()
                if game.victory():
                    break
                continue
            # One mutually useful 1:1 offer; opponents refuse to help a player at 9 VP.
            did_trade = False
            if game.public_points(p) < 9:
                goal = COSTS['city'] if game.counts(p,1) and game.counts(p,2)<4 else COSTS['settlement']
                for q in range(4):
                    if q == p or game.public_points(q) >= 9:
                        continue
                    wanted = [r for r in RES if game.hands[p][r] < goal[r] and game.hands[q][r] >= 2]
                    offered = [r for r in RES if game.hands[p][r] > goal[r] and game.hands[q][r] == 0]
                    if wanted and offered:
                        game.emit('Comercio_Jugadores', Jugador_Objetivo=NAMES[q], Recursos_Ofrecidos={offered[0]:1},
                                  Recursos_Recibidos={wanted[0]:1}, comment='Intercambio 1:1 de excedentes por un recurso que falta a cada jugador.')
                        did_trade = True; break
            if not did_trade:
                break
        if game.winner is None:
            game.emit('Fin_De_Turno')
    assert game.winner is not None, 'No completed game within the deterministic turn limit'
    # Validate the saved stream from a fresh state, without any move-selection code.
    audit = Game()
    for index,e in enumerate(game.events):
        try:
            audit.apply(e)
        except AssertionError as error:
            raise AssertionError(f'Event {index}: {e}') from error
    assert audit.hands == game.hands and audit.buildings == game.buildings and audit.winner == game.winner
    result = dict(Seed=SEED, Modo='Modo_Clasico', SemillaSimulacion=SEED+42,
                  Reglas='CATAN base 3–4, reglamento 2020; sin variante ladrón amistoso',
                  Alcance='Catan base sin expansiones. Mazo de 25 cartas barajado con semilla independiente; compra y uso de cartas. Decisiones heurísticas, no óptimas garantizadas.',
                  Jugadores=NAMES, Casillas=CELLS, Vertices=[dict(Id=v,Casillas=[h+1 for h in TILES[v]]) for v in ADJ],
                  Aristas=[dict(Id=e,A=a,B=b) for e,(a,b) in enumerate(EDGES)],
                  ColocacionInicial=placements, EventosIniciales=setup_count,
                  Resumen=dict(Ganador=NAMES[game.winner], Turnos=game.turn, Eventos=len(game.events),
                               Puntos={n:game.points(p) for p,n in enumerate(NAMES)},
                               PuntosVisibles={n:game.public_points(p) for p,n in enumerate(NAMES)},
                               Ciudades={n:game.counts(p,2) for p,n in enumerate(NAMES)},
                               Poblados={n:game.counts(p,1) for p,n in enumerate(NAMES)},
                               Longitudes={n:game.road_length(p) for p,n in enumerate(NAMES)},
                               GranRuta=NAMES[game.longest] if game.longest is not None else None,
                               Ejercito=NAMES[game.army] if game.army is not None else None,
                               CartasCompradas=dict(Counter(e['Tipo_Carta_Desarrollo'] for e in game.events if e['Tipo']=='Comprar_Carta_Desarrollo')),
                               CartasJugadas=dict(Counter(e['Tipo_Carta_Desarrollo'] for e in game.events if e['Tipo']=='Jugar_Carta_Desarrollo')),
                               RecursosFinales={n:dict(game.hands[p]) for p,n in enumerate(NAMES)},
                               Dados=dict(sorted(Counter(e['Resultado_Dados'] for e in game.events if e['Tipo']=='Tirada_Dados').items())),
                               Auditoria='PASS: turnos, distancia, conexión, costes, banco de 19, producción, puertos, descarte, robo, piezas, mazo de 25, espera de cartas, una carta por turno, efectos, títulos y victoria.'),
                  Eventos=game.events)
    path=ROOT/'CATAN_WebAssembly/wwwroot/data/partida-543456789.json'
    path.write_text(json.dumps(result,ensure_ascii=False,indent=2)+'\n')
    print(json.dumps(result['Resumen'],ensure_ascii=False,indent=2))
    print('Colocaciones:', placements)


if __name__ == '__main__':
    generate()
