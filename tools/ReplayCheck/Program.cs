using System.Text.Json;
using System.Text.Json.Serialization;
using CATAN_WebAssembly.Pages.Partida;
using CATAN_WebAssembly.Pages.Partida.Panel_Tablero;
using CATAN_WebAssembly.Services;

// Run from the repository root: dotnet run --project tools/ReplayCheck
var json = File.ReadAllText("CATAN_WebAssembly/wwwroot/data/partida-543456789.json");
var options = new JsonSerializerOptions { Converters = { new JsonStringEnumConverter() } };
var example = JsonSerializer.Deserialize<PartidaEjemplo>(json, options)!;
var summary = JsonDocument.Parse(json).RootElement.GetProperty("Resumen");
var players = new Jugador_Class();
foreach (var name in example.Jugadores)
    players.Insertar_jugador(new Jugador_Class { Nombre = name, Color = name, EsTurno = players.ListaJugadores.Count == 0 }, players.ListaJugadores.Count);
var board = new CatanBoardState
{
    Celdas = example.Casillas,
    Vertices = Enumerable.Range(0,54).Select(i => new HexVertex { Id = i }).ToList(),
    Edges = Enumerable.Range(0,72).Select(i => new HexEdge { Id = i }).ToList(),
    Replay = true
};
var game = new PartidaService(board, players);
var history = new HistorialService();
void Check(bool condition, string message) { if (!condition) throw new Exception(message); }
example.ValidarTablero(board, example.Seed, example.Modo, example.Jugadores);
bool rejected = false;
try { example.ValidarTablero(board, example.Seed + 1, example.Modo, example.Jugadores); }
catch (InvalidOperationException) { rejected = true; }
Check(rejected, "Example must reject a different seed");
history.CargarEjemplo(example.Eventos);
history.IrAlEvento(0, game);
for (int i = 0; i < example.Eventos.Count; i++)
{
    history.Siguiente_Evento(game, board);
    Check(players.ListaJugadores.All(p => p.Recursos.Values.All(n => n >= 0)), $"Negative resources at {i}");
    Check(players.ListaJugadores.Count(p => p.EsTurno) == 1, $"Wrong active player count at {i}");
    Check(board.Celdas.Count(c => c.Has_Thief) == 1, $"Wrong robber count at {i}");
}
void CheckFinal()
{
    foreach (var p in players.ListaJugadores)
    {
        Check(p.PuntosVictoria == summary.GetProperty("PuntosVisibles").GetProperty(p.Nombre).GetInt32(), $"Visible score: {p.Nombre}");
        Check(p.PuntosVictoria + p.CartasEnMano[TiposCartasDesarrollo.Punto_Victoria] == summary.GetProperty("Puntos").GetProperty(p.Nombre).GetInt32(), $"Total score: {p.Nombre}");
        foreach (var r in Enum.GetValues<ResourceType>())
            Check(p.Recursos[r] == summary.GetProperty("RecursosFinales").GetProperty(p.Nombre).GetProperty(r.ToString()).GetInt32(), $"Final resource: {p.Nombre}/{r}");
        Check(board.Vertices.Count(v => v.PlayerColor == p.Color && v.Structure == StructureType.City) == summary.GetProperty("Ciudades").GetProperty(p.Nombre).GetInt32(), "Final cities");
        Check(board.Vertices.Count(v => v.PlayerColor == p.Color && v.Structure == StructureType.Settlement) == summary.GetProperty("Poblados").GetProperty(p.Nombre).GetInt32(), "Final settlements");
    }
    Check(players.TurnoJugador().Nombre == summary.GetProperty("Ganador").GetString(), "Winner must be active");
}
CheckFinal();
foreach (int checkpoint in new[] { example.EventosIniciales, 200, 0, example.Eventos.Count })
    history.IrAlEvento(checkpoint, game);
CheckFinal();
history.Anterior_Evento(game, board);
history.Siguiente_Evento(game, board);
CheckFinal();
history.IrAlEvento(0, game);
Check(game.UltimaTirada is null && board.Celdas.Single(c => c.Has_Thief).Recurso == Resource_Cell.Desert, "Rewind dice and robber");
Check(players.ListaJugadores.All(p => p.PuntosVictoria == 0 && p.CartasEnMano.Values.All(n => n == 0)), "Rewind player state");

// Regression: buying now, playing later, one non-VP card per turn, exactly two free roads.
history.IrAlEvento(example.EventosIniciales, game);
var actor = players.TurnoJugador();
foreach (var r in Enum.GetValues<ResourceType>()) actor.Recursos[r] = 10;
HistorialEvento Card(TipoEvento kind, TiposCartasDesarrollo card) => new() { Tipo = kind, Jugador = actor.Nombre, Tipo_Carta_Desarrollo = card };
Check(game.Aplicar(Card(TipoEvento.Comprar_Carta_Desarrollo, TiposCartasDesarrollo.Carreteras)), "Buy roads card");
Check(!game.Aplicar(Card(TipoEvento.Jugar_Carta_Desarrollo, TiposCartasDesarrollo.Carreteras)), "Cannot play purchased card on same turn");
for (int i = 0; i < 4; i++) Check(game.Aplicar(new() { Tipo = TipoEvento.Fin_De_Turno, Jugador = players.TurnoJugador().Nombre }), "Advance turn");
actor.CartasEnMano[TiposCartasDesarrollo.Caballero] = 1;
Check(game.Aplicar(Card(TipoEvento.Jugar_Carta_Desarrollo, TiposCartasDesarrollo.Carreteras)), "Play old roads card");
Check(!game.Aplicar(Card(TipoEvento.Jugar_Carta_Desarrollo, TiposCartasDesarrollo.Caballero)), "One card per turn");
foreach (var r in Enum.GetValues<ResourceType>()) actor.Recursos[r] = 0;
var edges = board.Edges.Where(e => !e.HasRoad).Take(3).ToArray();
for (int i = 0; i < 3; i++)
    Check(game.Aplicar(new() { Tipo = TipoEvento.Construccion_Carretera, Jugador = actor.Nombre, Id_Posicion_pieza = edges[i].Id }) == (i < 2), "Exactly two free roads");
Check(!game.Aplicar(new() { Tipo = TipoEvento.Victoria, Jugador = actor.Nombre, Puntos_Victoria = 10 }), "Victory cannot fabricate points");
Console.WriteLine($"PASS: {example.Eventos.Count} events; final scores/resources/buildings; forward/rewind; card timing; free roads; victory validation.");
