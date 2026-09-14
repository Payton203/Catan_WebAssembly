using CATAN_WebAssembly.Pages.Partida;
using CATAN_WebAssembly.Pages.Partida.Panel_Tablero;

public enum TipoEvento
{
    Construccion_Estructura,
    Construccion_Carretera,
    Robo,
    Tirada_Dados,
    Intercambio,
    Carta_Desarrollo,
}

public record HistorialEvento
{
    //Datos generales
    public TipoEvento Tipo { get; init; }
    public string Jugador { get; init; } = "";
    public int Puntos { get; init; }
    public DateTime Momento { get; init; } = DateTime.Now;

    public bool Mostrar_en_partida { get; init; } = true;
    //Se usan cuando Tipo == TipoEvento.Intercambio
    public string? JugadorReceptor { get; init; }
    public Dictionary<ResourceType, int>? RecursosEntregados { get; init; }
    public Dictionary<ResourceType, int>? RecursosRecibidos { get; init; }

    //Se usa cuando Tipo == TipoEvento.Carta_Desarrollo, se usa solo para datos post-match
    public TiposCartasDesarrollo? Carta_canjeada;

    //Se usa cuando Tipo == TipoEvento.Construccion_Estructura || TipoEvento.Construccion_Camino
    public int? Id_Posicion_construccion { get; init; }
    public StructureType? Estructura_Vertice { get; init; }
    public bool Estructura_Camino { get; init; }
    //Se usa cuando Tipo == TipoEvento.Tirada_Dados
    public int Resultado_Dados { get; init; }
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
    }
}
