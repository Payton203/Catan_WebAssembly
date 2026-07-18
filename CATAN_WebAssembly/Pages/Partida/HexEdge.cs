namespace CATAN_WebAssembly.Pages.Partida;

public class HexEdge
{
    public string  Id          { get; set; } = string.Empty;
    public double  X           { get; set; }  // punto medio de la arista
    public double  Y           { get; set; }
    public double  Angle       { get; set; }  // rotación en grados desde vertical
    public bool    HasRoad     { get; set; }
    public string? PlayerColor { get; set; }
}
