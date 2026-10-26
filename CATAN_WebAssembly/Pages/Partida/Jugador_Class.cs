using System.ComponentModel;

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
        // Cartas de desarrollo que el jugador tiene en mano (todavía sin jugar).
        // Para los rivales, el tipo es información privada.
        public Dictionary<TiposCartasDesarrollo, int> CartasEnMano { get; set; } =
            Enum.GetValues<TiposCartasDesarrollo>().ToDictionary(tipo => tipo, _ => 0);

        // Cartas de desarrollo ya jugadas (públicas para todos).
        // Los caballeros jugados cuentan para el ejército más grande.
        public Dictionary<TiposCartasDesarrollo, int> CartasJugadas { get; set; } =
            Enum.GetValues<TiposCartasDesarrollo>().ToDictionary(tipo => tipo, _ => 0);

        // Helpers para el panel
        public int TotalCartasEnMano => CartasEnMano.Values.Sum();
        public int CaballerosJugados => CartasJugadas[TiposCartasDesarrollo.Caballero];

        // Jugar una carta: pasa de "en mano" a "jugada"
        public bool JugarCarta(TiposCartasDesarrollo tipo)
        {
            if (CartasEnMano[tipo] <= 0) return false;
            CartasEnMano[tipo]--;
            CartasJugadas[tipo]++;
            return true;
        }
        public bool Oculto { get; set; } = true;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="jugador"></param>
        /// <param name="numero_jugador"> 0,1,2,3</param>
        public void Insertar_jugador(Jugador_Class jugador, int numero_jugador)
        {
            ListaJugadores.Insert(numero_jugador, jugador);
        }

        public void Eliminar_todos_los_jugadores()
        {
            ListaJugadores.RemoveRange(0, ListaJugadores.Count());
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

        public void Pasar_Turno_Inicial(int contadorTurno)
        {
            int idActual = ListaJugadores.FindIndex(j => j.EsTurno);
            if (idActual == -1) return;

            ListaJugadores[idActual].EsTurno = false;

            int siguienteId;

            if (contadorTurno < 4)
            {
                // Ronda 1 (Turnos 1 a 4): Avanza normal (0 -> 1 -> 2 -> 3)
                siguienteId = idActual + 1;
            }
            else if (contadorTurno == 4)
            {
                // Turno 4 al 5: El último jugador repite turno (3 -> 3)
                siguienteId = idActual;
            }
            else if (contadorTurno < 8)
            {
                // Ronda 2 (Turnos 5 a 8): Retrocede (3 -> 2 -> 1 -> 0)
                siguienteId = idActual - 1;
            }
            else
            {
                // A partir del Turno 8 al 9: Vuelve a avanzar normal (0 -> 1 -> 2 -> 3)
                siguienteId = (idActual + 1) % ListaJugadores.Count;
            }
            ListaJugadores[siguienteId].EsTurno = true;
        }
    }
}
