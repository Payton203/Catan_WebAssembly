using CATAN_WebAssembly.Pages.Partida.Panel_Tablero;

namespace CATAN_WebAssembly.Services;

public class PartidaEjemplo
{
    public int Seed { get; set; }
    public string Modo { get; set; } = "";
    public List<string> Jugadores { get; set; } = new();
    public List<HexCell> Casillas { get; set; } = new();
    public int EventosIniciales { get; set; }
    public List<HistorialEvento> Eventos { get; set; } = new();

    public void ValidarTablero(CatanBoardState tablero, int seed, string modo, IEnumerable<string> jugadores)
    {
        if (Seed != seed || Modo != modo || !Jugadores.SequenceEqual(jugadores) ||
            tablero.Vertices.Count != 54 || tablero.Edges.Count != 72 ||
            !Casillas.OrderBy(c => c.Id).Select(c => (c.Id, c.Recurso, c.Label))
                .SequenceEqual(tablero.Celdas.OrderBy(c => c.Id).Select(c => (c.Id, c.Recurso, c.Label))))
            throw new InvalidOperationException("El ejemplo no corresponde a esta seed, tablero o jugadores.");
    }
}
