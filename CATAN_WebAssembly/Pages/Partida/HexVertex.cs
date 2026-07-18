namespace CATAN_WebAssembly.Pages.Partida;

public enum StructureType { None, Settlement, City }

public class HexVertex
{
    public string        Id          { get; set; } = string.Empty;
    public double        X           { get; set; }
    public double        Y           { get; set; }
    public StructureType Structure   { get; set; } = StructureType.None;
    public string?       PlayerColor { get; set; }
}
