namespace CATAN_WebAssembly.Pages.Partida;

public class HexCell
{
    public string Id { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public string? BackgroundColor { get; set; }
    public object? Data { get; set; } // para colgar lo que necesites (payload custom)
}
