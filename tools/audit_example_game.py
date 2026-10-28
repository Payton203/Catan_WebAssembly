"""Second audit: infer edges from actual rendered SVG coordinates, then replay.

Run from any directory: python3 tools/audit_example_game.py [live-geometry.json]
The default geometry fixture is a read-only DOM snapshot of the running app.
Move legality here does not use the generator's paths or site-selection code.
"""
from collections import Counter
from copy import deepcopy
import json
import math
from pathlib import Path
import re
import sys
from generate_example_game import Game, ROOT

data = json.loads((ROOT/'CATAN_WebAssembly/wwwroot/data/partida-543456789.json').read_text())
snapshot = Path(sys.argv[1]) if len(sys.argv)>1 else ROOT/'tools/fixtures/rendered-board-543456789.json'
live = json.loads(snapshot.read_text())
def xy(item):
    return tuple(map(float,re.search(r'translate\(([^,]+),([^\)]+)',item['transform']).groups()))
vertices = {v['id']:xy(v) for v in live['vertices']}
edges = {}
for edge in live['edges']:
    center = xy(edge)
    a,b = sorted(vertices,key=lambda v:math.dist(center,vertices[v]))[:2]
    midpoint = tuple((x+y)/2 for x,y in zip(vertices[a],vertices[b]))
    assert math.dist(midpoint,center)<.02, ('Misaligned road',edge['id'])
    reference = data['Aristas'][edge['id']]
    assert {a,b}=={reference['A'],reference['B']}, ('Wrong road ID',edge['id'])
    edges[edge['id']] = (a,b)
assert len(vertices)==54 and len(edges)==72
for rendered,cell in zip(live['cells'],data['Casillas']):
    assert rendered['id']==cell['Id'] and rendered['label'].strip()==str(cell['Label'] or '')
    assert cell['BackgroundColor'] in rendered['style']
neighbors = {v:set() for v in vertices}
incident = {v:set() for v in vertices}
for e,(a,b) in edges.items():
    neighbors[a].add(b);neighbors[b].add(a)
    incident[a].add(e);incident[b].add(e)

def spatial_audit(events):
    roads, buildings, details = {}, {}, []
    placement = None
    setup_turn = 0
    initial_settlements = initial_roads = 0
    for index,event in enumerate(events,1):
        p=event['Jugador'];kind=event['Tipo'];position=event.get('Id_Posicion_pieza')
        if kind=='Construccion_Estructura':
            if event['Estructura_Vertice']=='Settlement':
                assert position in vertices and position not in buildings, (index,'Occupied vertex')
                assert all(v not in buildings for v in neighbors[position]), (index,'Distance rule')
                if setup_turn<8:
                    assert initial_settlements==0
                    initial_settlements+=1;placement=position
                else:
                    assert any(roads.get(e)==p for e in incident[position]), (index,'Unconnected settlement')
                buildings[position]=(p,1)
            else:
                assert setup_turn>=8 and buildings.get(position)==(p,1), (index,'City on another player')
                buildings[position]=(p,2)
        elif kind=='Construccion_Carretera':
            assert position in edges and position not in roads, (index,'Duplicate road')
            a,b=edges[position]
            if setup_turn<8:
                assert placement in (a,b) and initial_roads==0, (index,'Initial road disconnected')
                anchors=[placement];initial_roads+=1
            else:
                anchors=[v for v in (a,b) if buildings.get(v,(None,0))[0]==p or
                         (v not in buildings and any(roads.get(e)==p for e in incident[v]))]
                assert anchors, (index,'Disconnected road or blocked by opponent')
            roads[position]=p
            assert sum(owner==p for owner in roads.values())<=15
            details.append(dict(Evento=index,Jugador=p,Arista=position,Vertices=[a,b],ConectaEn=anchors))
        elif kind=='Fin_De_Turno' and setup_turn<8:
            assert initial_settlements==initial_roads==1
            setup_turn+=1;initial_settlements=initial_roads=0;placement=None
        for player in data['Jugadores']:
            assert sum(owner==player and level==1 for owner,level in buildings.values())<=5
            assert sum(owner==player and level==2 for owner,level in buildings.values())<=4
    return details

details=spatial_audit(data['Eventos'])
game=Game()
for e in data['Eventos']:
    game.apply(e)
assert game.winner is not None

# Prove the auditors reject corrupted logs, rather than merely accepting this one.
negative_tests=[]
def rejects(label, mutation, validator):
    events=deepcopy(data['Eventos']);mutation(events)
    try:validator(events)
    except (AssertionError,KeyError,ValueError):negative_tests.append(label)
    else:raise AssertionError('Auditor accepted invalid case: '+label)
def rules(events):
    game=Game()
    for e in events:game.apply(e)
road_indexes=[i for i,e in enumerate(data['Eventos']) if e['Tipo']=='Construccion_Carretera']
first_road=data['Eventos'][road_indexes[0]]['Id_Posicion_pieza']
rejects('carretera duplicada',lambda es:es[road_indexes[1]].update(Id_Posicion_pieza=first_road),spatial_audit)
initial_v=data['Eventos'][0]['Id_Posicion_pieza']
disconnected=next(e for e,(a,b) in edges.items() if initial_v not in (a,b))
rejects('carretera inicial desconectada',lambda es:es[road_indexes[0]].update(Id_Posicion_pieza=disconnected),spatial_audit)
second_settlement=next(i for i,e in enumerate(data['Eventos']) if i>0 and e['Tipo']=='Construccion_Estructura')
rejects('poblados adyacentes',lambda es:es[second_settlement].update(Id_Posicion_pieza=min(neighbors[initial_v])),spatial_audit)
roll=next(i for i,e in enumerate(data['Eventos']) if e['Tipo']=='Tirada_Dados')
rejects('dados inconsistentes',lambda es:es[roll].update(Resultado_Dados=99),rules)
income=next(i for i,e in enumerate(data['Eventos']) if e['Tipo']=='Produccion_Recursos' and e['Turno']>0)
rejects('producción inventada',lambda es:es[income].update(Recursos_Recibidos={'Rock':19}),rules)
trade=next(i for i,e in enumerate(data['Eventos']) if e['Tipo']=='Comercio_Banca')
rejects('canje bancario ilegal',lambda es:es[trade].update(Recursos_Recibidos={'Rock':19}),rules)
purchase=next(i for i,e in enumerate(data['Eventos']) if e['Tipo']=='Comprar_Carta_Desarrollo')
def same_turn(es):
    e=deepcopy(es[purchase]);e['Tipo']='Jugar_Carta_Desarrollo';es.insert(purchase+1,e)
rejects('carta comprada y jugada en el mismo turno',same_turn,rules)
rejects('victoria falsa',lambda es:es[-1].update(Puntos_Victoria=20),rules)

result=dict(Estado='PASS',Seed=data['Seed'],Eventos=len(data['Eventos']),
            VerticesVerificados=len(vertices),AristasVerificadas=len(edges),
            CarreterasConstruidas=len(details),
            PobladosConstruidos=sum(e['Tipo']=='Construccion_Estructura' and e['Estructura_Vertice']=='Settlement' for e in data['Eventos']),
            CiudadesConstruidas=sum(e['Tipo']=='Construccion_Estructura' and e['Estructura_Vertice']=='City' for e in data['Eventos']),
            CasosIlegalesRechazados=negative_tests,Conexiones=details,
            Alcance='Verifica este historial sintético; no certifica todas las entradas posibles del motor interactivo.')
(ROOT/'CATAN_WebAssembly/wwwroot/data/partida-543456789-audit.json').write_text(json.dumps(result,ensure_ascii=False,indent=2)+'\n')
print(json.dumps({k:v for k,v in result.items() if k!='Conexiones'},ensure_ascii=False,indent=2))
