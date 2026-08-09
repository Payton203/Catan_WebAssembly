namespace CATAN_WebAssembly.Pages.Partida;

public enum Resource_Cell { Wood, Brick, Sheep, Wheat, Rock, Desert}
public class HexCell
{
    public int Id { get; set; }
    public int? Label { get; set; }
    public string? BackgroundColor { get; set; }
    public Resource_Cell Recurso { get; set; }
    public object? Data { get; set; } // para colgar lo que necesites (payload custom)
    public bool Has_Thief { get; set; } = false;
    public bool Externa {  get; set; } = false;
    /// <summary>
    /// serian como van las fichas, A, B, C, ... , S
    /// </summary>
    public int Alineacion { get; set; }
}
