using CATAN_WebAssembly.Pages.Partida;
using CATAN_WebAssembly.Pages.Partida.Panel_Tablero;

namespace CATAN_WebAssembly.Services;

/// <summary>
/// Aplica al estado de la partida los eventos (HistorialEvento) que llegan del Arduino.
/// Si los clicks de la pantalla también arman un HistorialEvento y pasan por Aplicar,
/// el tablero físico y el de pantalla comparten la misma lógica y no se desincronizan.
/// </summary>
public class PartidaService(CatanBoardState tablero, Jugador_Class jugadores)
{
    /// <summary>Se dispara cuando un evento modificó el estado, para que los componentes se re-rendericen.</summary>
    public event Action? OnChange;

    /// <returns>
    /// true si el evento era válido y se aplicó; false si se rechazó.
    /// Un evento rechazado no debería registrarse en el historial.
    /// </returns>
    public bool Aplicar(HistorialEvento e)
    {
        var jugador = jugadores.ListaJugadores.FirstOrDefault(j =>
            string.Equals(j.Nombre, e.Jugador, StringComparison.OrdinalIgnoreCase));
        if (jugador is null) return false;

        bool aplicado = e.Tipo switch
        {
            TipoEvento.Construccion_Estructura => ConstruirEstructura(e, jugador),
            TipoEvento.Construccion_Carretera => ConstruirCarretera(e, jugador),
            TipoEvento.Mover_Ladron => MoverLadron(e),
            TipoEvento.Fin_De_Turno => FinDeTurno(jugador),
            _ => true // eventos que todavía no tocan el estado (dados, comercio, etc.): pasan tal cual
        };

        if (aplicado) OnChange?.Invoke();
        return aplicado;
    }

    private bool ConstruirEstructura(HistorialEvento e, Jugador_Class jugador)
    {
        if (e.Id_Posicion_construccion is not int idVertice) return false;
        if (e.Estructura_Vertice is not StructureType nueva || nueva == StructureType.None) return false;

        var vertice = tablero.Vertices.FirstOrDefault(v => v.Id == idVertice);
        if (vertice is null) return false;

        var actual = vertice.Structure;

        bool valido =
            (nueva == StructureType.Settlement && actual == StructureType.None) ||
            (nueva == StructureType.City && actual == StructureType.Settlement && vertice.PlayerColor == jugador.Color);
        if (!valido) return false;

        vertice.Structure = nueva;
        vertice.PlayerColor = jugador.Color;

        // Un poblado suma 1; una ciudad reemplaza a un poblado, así que también suma 1 neto (1 -> 2).
        // Sacá esta línea si los puntos los calculás en otro lado.
        jugador.PuntosVictoria++;
        return true;
    }

    private bool ConstruirCarretera(HistorialEvento e, Jugador_Class jugador)
    {
        if (e.Id_Posicion_construccion is not int idArista) return false;

        var arista = tablero.Edges.FirstOrDefault(x => x.Id == idArista);
        if (arista is null || arista.HasRoad) return false;

        arista.HasRoad = true;
        arista.PlayerColor = jugador.Color;
        return true;
    }

    private bool MoverLadron(HistorialEvento e)
    {
        if (e.Id_Posicion_construccion is not int idCelda) return false;

        var destino = tablero.Celdas.FirstOrDefault(c => c.Id == idCelda);
        if (destino is null || destino.Has_Thief) return false;

        foreach (var celda in tablero.Celdas) celda.Has_Thief = false;
        destino.Has_Thief = true;
        return true;
    }

    private bool FinDeTurno(Jugador_Class jugador)
    {
        // Solo el jugador con el turno puede terminarlo: evita pasar dos veces por un rebote del botón.
        if (!jugador.EsTurno) return false;

        // Se llama sobre la instancia que tiene la lista de jugadores, no sobre "jugador":
        // cada Jugador_Class tiene su propia ListaJugadores, y la de un jugador suelto está vacía.
        jugadores.Pasar_Turno();
        return true;
    }
}
