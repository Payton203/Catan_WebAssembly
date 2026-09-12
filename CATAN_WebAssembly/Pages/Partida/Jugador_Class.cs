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
    public class Jugador_Class
    {
        public List<Jugador_Class> ListaJugadores { get; private set; } =  new()
        {
        };
        public string Nombre { get; set; } = string.Empty;

        // Indica si es el turno actual de este jugador (dibuja el borde y el badge naranja)
        public bool EsTurno { get; set; }

        public string Color { get; set; } = "white";

        public int PuntosVictoria { get; set; }

        // Cantidad de cada recurso que tiene el jugador en mano.
        // Clave = clave del recurso (definida en RecursosCatan.Todos), Valor = cantidad.
        // No hace falta cargar todas las claves: si falta una, se muestra como 0.
        public Dictionary<ResourceType, int> Recursos { get; set; } = Enum.GetValues<ResourceType>().ToDictionary(tipo => tipo, _ => 0);
                                                                        //inicializa todos los valores en 0

        // Cantidad de caballeros jugados (para el marcador de ejército más grande)
        public Dictionary<TiposCartasDesarrollo, int> CartasDesarrollo { get; set; } = Enum.GetValues<TiposCartasDesarrollo>().ToDictionary(tipo => tipo, _ => 0);

        public bool Oculto { get; set; } = true;

        public void Agregar_jugador(Jugador_Class jugador)
        {
            ListaJugadores.Add(jugador);
        }

        /// <summary>
        /// devuelve el objeto jugador, del cual es su turno actualmente
        /// </summary>
        /// <returns></returns>
        public Jugador_Class TurnoJugador()
        {
            Jugador_Class? jugador = ListaJugadores.FirstOrDefault(j => j.EsTurno);

            if (jugador == null)
            {
                Console.WriteLine("Error, no es el turno de nadie");
                throw new InvalidOperationException("No hay ningún jugador con el turno.");
            }

            return jugador;
        }

        public void Pasar_Turno()
        {
            int id = 0;
            for (int i = 0; i < ListaJugadores.Count; i++)
            {
                if (ListaJugadores[i].EsTurno == true) { id = i; break; }
            }
            ListaJugadores[id].EsTurno = false;
            if ((id + 1) < ListaJugadores.Count) ListaJugadores[id + 1].EsTurno = true;
            else ListaJugadores[0].EsTurno = true;
        }
    }
}
