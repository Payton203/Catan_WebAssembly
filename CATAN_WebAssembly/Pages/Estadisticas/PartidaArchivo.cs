namespace CATAN_WebAssembly.Models;

// Modelos que reflejan el formato de los archivos JSON de la partida.
// Los nombres de las propiedades coinciden con las claves del JSON
// (GetFromJsonAsync ignora mayúsculas/minúsculas).

public class PartidaArchivo
{
    public int Seed { get; set; }
    public string Modo { get; set; } = "";
    public string Reglas { get; set; } = "";
    public string Alcance { get; set; } = "";
    public List<string> Jugadores { get; set; } = new();
    public ResumenArchivo Resumen { get; set; } = new();
    public List<EventoArchivo> Eventos { get; set; } = new();
}

public class ResumenArchivo
{
    public string Ganador { get; set; } = "";
    public int Turnos { get; set; }
    public Dictionary<string, int> Puntos { get; set; } = new();
    public Dictionary<string, int> PuntosVisibles { get; set; } = new();
    public Dictionary<string, int> Longitudes { get; set; } = new();
    public string? GranRuta { get; set; }
    public string? Ejercito { get; set; }
    public string Auditoria { get; set; } = "";
}

public class EventoArchivo
{
    public string Tipo { get; set; } = "";
    public string Jugador { get; set; } = "";
    public int Turno { get; set; }
    public DateTimeOffset Momento { get; set; }
    public string Comentario { get; set; } = "";

    public int? Id_Posicion_pieza { get; set; }
    public string? Estructura_Vertice { get; set; }
    public int? Resultado_Dados { get; set; }
    public string? Jugador_Objetivo { get; set; }
    public Dictionary<string, int>? Recursos_Recibidos { get; set; }
    public Dictionary<string, int>? Recursos_Ofrecidos { get; set; }
    public string? Tipo_Carta_Desarrollo { get; set; }
    public int? Puntos_Victoria { get; set; }
}

// ───────── Auditoría (partida-XXXX-audit.json) ─────────

public class AuditoriaArchivo
{
    public string Estado { get; set; } = "";
    public int Eventos { get; set; }
    public int VerticesVerificados { get; set; }
    public int AristasVerificadas { get; set; }
    public int CarreterasConstruidas { get; set; }
    public int PobladosConstruidos { get; set; }
    public int CiudadesConstruidas { get; set; }
    public List<string> CasosIlegalesRechazados { get; set; } = new();
    public List<ConexionArchivo> Conexiones { get; set; } = new();
    public string Alcance { get; set; } = "";
}

public class ConexionArchivo
{
    public int Evento { get; set; }
    public string Jugador { get; set; } = "";
    public int Arista { get; set; }
}
