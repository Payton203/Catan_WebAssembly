using CATAN_WebAssembly.Pages.Partida.Panel_Tablero;

namespace CATAN_WebAssembly.Services
{
    public class CatanBoardState
    {
        public List<HexCell> Celdas { get; set; } = new();
        public List<HexVertex> Vertices { get; set; } = new();
        public List<HexEdge> Edges { get; set; } = new();
        public List<HexPort> Ports { get; set; } = new();
        public bool Inicializado { get; set; } = false;
        public bool Replay { get; set; } = false;
    }
}
