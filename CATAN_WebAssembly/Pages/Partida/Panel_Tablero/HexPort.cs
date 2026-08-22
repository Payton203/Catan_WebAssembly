namespace CATAN_WebAssembly.Pages.Partida.Panel_Tablero;

public enum PortType { Wood, Brick, Sheep, Wheat, Rock, ThreeToOne }

public class HexPort
{
    public string   Id      { get; set; } = string.Empty;
    public PortType Type    { get; set; } = PortType.ThreeToOne;

    // Posición del barco (calculada por CatanOverlay)
    public double X     { get; set; }
    public double Y     { get; set; }
    public double Angle { get; set; }  // rotación del barco en grados

    // Los 2 vértices del muelle (a los que apuntan los caminos del puerto)
    public double V1X { get; set; }
    public double V1Y { get; set; }
    public double V2X { get; set; }
    public double V2Y { get; set; }
}
