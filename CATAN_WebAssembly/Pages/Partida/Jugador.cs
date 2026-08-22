namespace CATAN_WebAssembly.Pages.Partida
{
    public enum ResourceType { Wood, Brick, Sheep, Wheat, Rock }
    public enum TiposCartasDesarrollo
    {
        Caballero,

        Punto_Victoria,

        Carreteras,

        Invento,

        Monopolio
    }
    /// <summary>
    /// Representa el estado de un jugador para mostrar en el panel "JUGADORES".
    /// Un jugador nuevo se crea así:
    ///
    ///   new Jugador
    ///   {
    ///       Nombre = "Ana",
    ///       EsTurno = true,
    ///       PuntosVictoria = 5,
    ///       Caballeros = 0,
    ///       Recursos = new() { {"madera", 2}, {"ladrillo", 1}, {"oveja", 3}, {"trigo", 0} }
    /// </summary>

    //   }
    public class Jugador
    {
            public string Nombre { get; set; } = string.Empty;

            // Indica si es el turno actual de este jugador (dibuja el borde y el badge naranja)
            public bool EsTurno { get; set; }

            public int PuntosVictoria { get; set; }

            // Cantidad de cada recurso que tiene el jugador en mano.
            // Clave = clave del recurso (definida en RecursosCatan.Todos), Valor = cantidad.
            // No hace falta cargar todas las claves: si falta una, se muestra como 0.
            public Dictionary<ResourceType, int> Recursos { get; set; } = Enum.GetValues<ResourceType>().ToDictionary(tipo => tipo, _ => 0);
                                                                          //inicializa todos los valores en 0

        // Cantidad de caballeros jugados (para el marcador de ejército más grande)
            public Dictionary<TiposCartasDesarrollo, int> CartasDesarrollo { get; set; } = Enum.GetValues<TiposCartasDesarrollo>().ToDictionary(tipo => tipo, _ => 0);

            public bool Oculto { get; set; } = true;
    }
}
