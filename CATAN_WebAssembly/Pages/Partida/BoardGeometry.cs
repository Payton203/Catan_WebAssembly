namespace CATAN_WebAssembly.Pages.Partida;

/// <summary>
/// Toda la información geométrica que HexGrid le pasa a CatanBoard
/// cada vez que el tamaño del contenedor cambia.
/// </summary>
public record BoardGeometry(
    double                    HexSize,
    double                    Gap,
    double                    ContainerWidth,
    List<(double X, double Y)> HexCenters
);
