using CATAN_WebAssembly.Pages.Partida;
using CATAN_WebAssembly.Pages.Partida.Panel_Tablero;

public enum TipoEvento
{
    // Construcciones
    Construccion_Estructura,
    Construccion_Carretera,

    // Dados y Fases
    Tirada_Dados,
    Produccion_Recursos, // cuando reciben los jugadores recursos por sus pueblos/ciudades adyacentes a las casillas
    Fin_De_Turno,

    // Comercio
    Comercio_Banca,
    Comercio_Jugadores,

    // Ladrón y Descarte
    Mover_Ladron,
    Robar_Recurso,
    Descarte_Por_Siete,

    // Cartas de Desarrollo
    Comprar_Carta_Desarrollo,
    Jugar_Carta_Desarrollo,

    // Títulos y Fin de Juego
    Obtener_Gran_Ruta,
    Obtener_Ejercito,
    Victoria
}

public record HistorialEvento
{
    public required TipoEvento Tipo { get; init; }
    public required string Jugador { get; init; }

    // Evalúa DateTime.Now automáticamente al instanciar
    public DateTime Momento { get; init; } = DateTime.Now;
    // Campos opcionales
    public int? Resultado_Dados { get; init; }
    public int? Dado1 { get; init; }
    public int? Dado2 { get; init; }
    public int? Turno { get; init; }
    public string? Comentario { get; init; }
    /// <summary>
    /// Con pieza, me refiero a lo que en el catan tradicional es una pieza:
    /// el ladron, las carreteras, los pueblos y las ciudades
    /// </summary>
    public int? Id_Posicion_pieza { get; init; }
    public StructureType? Estructura_Vertice { get; init; }
    public string? Jugador_Objetivo { get; init; }
    public TiposCartasDesarrollo? Tipo_Carta_Desarrollo { get; init; }
    public int? Puntos_Victoria { get; init; }

    // Listas/Diccionarios para soportar múltiples recursos en una sola acción
    public Dictionary<ResourceType, int>? Recursos_Ofrecidos { get; init; }
    public Dictionary<ResourceType, int>? Recursos_Recibidos { get; init; }

    //solo para el WebAssembly, para ver si el evento ya fue aplicado
    public bool aplicado { get; set; } = false;
}
namespace CATAN_WebAssembly.Services
{
    public class HistorialService()
    {
        //jajasndjkasnfjk

        public List<HistorialEvento> Historial { get; private set; } = new List<HistorialEvento>();
        public List<HistorialEvento> Historial_Mostrar { get; private set; } = new List<HistorialEvento>();

        public event Action? OnAccion;
        public event Action<HistorialEvento>? OnEventoMostrado;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="evento">new HistorialEvento {...}</param>
        public void AgregarEvento(HistorialEvento evento)
        {
            Historial.Add(evento);
            OnAccion?.Invoke();
        }
        public void EliminarHistorial()
        {
            Historial.RemoveRange(0, Historial.Count);
            Historial_Mostrar.RemoveRange(0, Historial_Mostrar.Count);
            OnAccion?.Invoke();
        }
        public void CargarEjemplo(IEnumerable<HistorialEvento> eventos)
        {
            Historial = eventos.ToList();
            Historial_Mostrar.Clear();
            OnAccion?.Invoke();
        }

        public void IrAlEvento(int cantidad, PartidaService partidaService)
        {
            var destino = Historial.Take(Math.Clamp(cantidad, 0, Historial.Count)).ToList();
            partidaService.ReconstruirEstadoDesdeHistorial(destino);
            Historial_Mostrar = destino;
            OnAccion?.Invoke();
        }
        //
        public void Siguiente_Evento(PartidaService partidaService, CatanBoardState boardState)
        {
            if (Historial_Mostrar.Count < Historial.Count)
            {
                var siguiente = Historial[Historial_Mostrar.Count];
                // Aplica solo el nuevo evento que entra en pantalla
                if (!partidaService.Aplicar(siguiente))
                    throw new InvalidOperationException($"Evento {Historial_Mostrar.Count + 1} rechazado: {siguiente.Tipo} / {siguiente.Jugador}");
                Historial_Mostrar.Add(siguiente);
                if (boardState.Replay) OnEventoMostrado?.Invoke(siguiente);
                OnAccion?.Invoke();
            }
        }

        public void Anterior_Evento(PartidaService partidaService, CatanBoardState boardState)
        {
            if (Historial_Mostrar.Count > 0)
            {
                // 1. Quitamos el último evento de la vista
                Historial_Mostrar.RemoveAt(Historial_Mostrar.Count - 1);

                // 2. Reconstruimos la partida completa al estado exacto del Historial_Mostrar
                partidaService.ReconstruirEstadoDesdeHistorial(Historial_Mostrar);

                OnAccion?.Invoke();
            }
        }
    }
}
