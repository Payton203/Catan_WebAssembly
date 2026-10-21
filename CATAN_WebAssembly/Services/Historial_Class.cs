using CATAN_WebAssembly.Pages.Partida;
using CATAN_WebAssembly.Pages.Partida.Panel_Tablero;

public enum TipoEvento
{
    // Construcciones
    Construccion_Estructura,
    Construccion_Carretera,

    // Dados y Fases
    Tirada_Dados,
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
    public bool Mostrar_en_partida { get; init; } = true;

    // Campos opcionales
    public int? Resultado_Dados { get; init; }
    public int? Id_Posicion_construccion { get; init; }
    public StructureType? Estructura_Vertice { get; init; }
    public string? Jugador_Objetivo { get; init; }
    public ResourceType? Recurso_Ofrecido { get; init; }
    public ResourceType? Recurso_Recibido { get; init; }
    public int? Cantidad_Ofrecida { get; init; }
    public int? Cantidad_Recibida { get; init; }
    public TiposCartasDesarrollo? Tipo_Carta_Desarrollo { get; init; }
    public int? Puntos_Victoria { get; init; }
}
namespace CATAN_WebAssembly.Services
{
    public class Historial_Class
    {

        public List<HistorialEvento> Historial { get; private set; } = new List<HistorialEvento>();

        public event Action? OnAccion;
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
        }
    }
}
