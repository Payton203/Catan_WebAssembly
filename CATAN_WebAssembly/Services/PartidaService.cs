using CATAN_WebAssembly.Pages.Partida;
using CATAN_WebAssembly.Pages.Partida.Panel_Tablero;

namespace CATAN_WebAssembly.Services;

/// <summary>
/// Aplica al estado de la partida los eventos (HistorialEvento) que llegan del Arduino o UI.
/// </summary>
public class PartidaService(CatanBoardState tablero, Jugador_Class jugadores)
{
    /// <summary>Se dispara cuando un evento modificó el estado, para que los componentes se re-rendericen.</summary>
    public event Action? OnChange;
    public int ContadorTurnos { get; private set; } = 1;
    // Contador temporal para controlar las 2 carreteras gratuitas tras jugar la carta
    private int _carreterasGratisRestantes = 0;
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
            // Construcciones
            TipoEvento.Construccion_Estructura => ConstruirEstructura(e, jugador),
            TipoEvento.Construccion_Carretera => ConstruirCarretera(e, jugador),

            // Dados y Recursos
            TipoEvento.Tirada_Dados => ProcesarTiradaDados(e),
            TipoEvento.Produccion_Recursos => ProcesarProduccionRecursos(e, jugador),

            // Comercio
            TipoEvento.Comercio_Banca => ProcesarComercioBanca(e, jugador),
            TipoEvento.Comercio_Jugadores => ProcesarComercioJugadores(e, jugador),

            // Ladrón y Descarte
            TipoEvento.Mover_Ladron => MoverLadron(e),
            TipoEvento.Robar_Recurso => ProcesarRobarRecurso(e, jugador),
            TipoEvento.Descarte_Por_Siete => ProcesarDescarte(e, jugador),

            // Cartas de Desarrollo
            TipoEvento.Comprar_Carta_Desarrollo => ComprarCartaDesarrollo(e, jugador),
            TipoEvento.Jugar_Carta_Desarrollo => JugarCartaDesarrollo(e, jugador),

            // Títulos, Turno y Final
            TipoEvento.Obtener_Gran_Ruta => ProcesarTituloRuta(jugador),
            TipoEvento.Obtener_Ejercito => ProcesarTituloEjercito(jugador),
            TipoEvento.Fin_De_Turno => FinDeTurno(jugador),
            TipoEvento.Victoria => ProcesarVictoria(e, jugador),

            _ => false
        };

        if (aplicado == true)
        { 
            OnChange?.Invoke();
            e.aplicado = true;
        }
        return aplicado;
    }

    private bool ConstruirEstructura(HistorialEvento e, Jugador_Class jugador)
    {
        if (e.Id_Posicion_pieza is not int idVertice) return false;
        if (e.Estructura_Vertice is not StructureType nueva || nueva == StructureType.None) return false;

        var vertice = tablero.Vertices.FirstOrDefault(v => v.Id == idVertice);
        if (vertice is null) return false;

        var actual = vertice.Structure;

        bool valido =
            (nueva == StructureType.Settlement && actual == StructureType.None) ||
            (nueva == StructureType.City && actual == StructureType.Settlement && vertice.PlayerColor == jugador.Color);
        if (!valido) return false;

        // --- DISTINCIÓN POR FASE ---
        if (ContadorTurnos > 8)
        {
            if (nueva == StructureType.Settlement)
            {
                var costoPoblado = new Dictionary<ResourceType, int>
                {
                    { ResourceType.Wood, 1 },
                    { ResourceType.Brick, 1 },
                    { ResourceType.Sheep, 1 },
                    { ResourceType.Wheat, 1 }
                };

                if (!TieneRecursosSuficientes(jugador, costoPoblado)) return false;
                DescontarRecursos(jugador, costoPoblado);
            }
            else if (nueva == StructureType.City)
            {
                var costoCiudad = new Dictionary<ResourceType, int>
                {
                    { ResourceType.Wheat, 2 },
                    { ResourceType.Rock, 3 }
                };

                if (!TieneRecursosSuficientes(jugador, costoCiudad)) return false;
                DescontarRecursos(jugador, costoCiudad);
            }
        }

        vertice.Structure = nueva;
        vertice.PlayerColor = jugador.Color;
        jugador.PuntosVictoria++;

        return true;
    }

    private bool ConstruirCarretera(HistorialEvento e, Jugador_Class jugador)
    {
        if (e.Id_Posicion_pieza is not int idArista) return false;

        var arista = tablero.Edges.FirstOrDefault(x => x.Id == idArista);
        if (arista is null || arista.HasRoad) return false;
        // Se cobra solo si estamos en Fase Normal Y no tenemos carreteras gratuitas de la carta
        bool esGratis = ContadorTurnos <= 8 || _carreterasGratisRestantes > 0;
        if (!esGratis)
        {
            var costoCarretera = new Dictionary<ResourceType, int>
            {
                { ResourceType.Wood, 1 },
                { ResourceType.Brick, 1 }
            };

            if (!TieneRecursosSuficientes(jugador, costoCarretera)) return false;
            DescontarRecursos(jugador, costoCarretera);
        }

        arista.HasRoad = true;
        arista.PlayerColor = jugador.Color;
        return true;
    }

    private bool ComprarCartaDesarrollo(HistorialEvento e, Jugador_Class jugador)
    {
        if (e.Tipo_Carta_Desarrollo is not TiposCartasDesarrollo tipoCarta) return false;

        var costoCarta = new Dictionary<ResourceType, int>
        {
            { ResourceType.Sheep, 1 },
            { ResourceType.Wheat, 1 },
            { ResourceType.Rock, 1 }
        };

        if (!TieneRecursosSuficientes(jugador, costoCarta)) return false;
        DescontarRecursos(jugador, costoCarta);

        jugador.CartasEnMano[tipoCarta]++;
        return true;
    }

    private bool ProcesarTiradaDados(HistorialEvento e)
    {
        // Evento informativo del dado; valida que traiga un número de dado legal en Catan (2 a 12)
        return e.Resultado_Dados is >= 2 and <= 12;
    }

    private bool ProcesarProduccionRecursos(HistorialEvento e, Jugador_Class jugador)
    {
        if (e.Recursos_Recibidos is null || e.Recursos_Recibidos.Count == 0) return false;

        foreach (var (recurso, cantidad) in e.Recursos_Recibidos)
        {
            if (cantidad <= 0) continue;
            jugador.Recursos[recurso] += cantidad;
        }

        return true;
    }

    private bool ProcesarComercioBanca(HistorialEvento e, Jugador_Class jugador)
    {
        if (e.Recursos_Ofrecidos is null || e.Recursos_Recibidos is null) return false;

        // Verificar si tiene todos los recursos que ofrece
        if (!TieneRecursosSuficientes(jugador, e.Recursos_Ofrecidos)) return false;

        // Descontar entregados
        foreach (var (recurso, cantidad) in e.Recursos_Ofrecidos)
        {
            jugador.Recursos[recurso] -= cantidad;
        }

        // Acreditar recibidos
        foreach (var (recurso, cantidad) in e.Recursos_Recibidos)
        {
            jugador.Recursos[recurso] += cantidad;
        }

        return true;
    }

    private bool ProcesarComercioJugadores(HistorialEvento e, Jugador_Class jugadorEmisor)
    {
        if (string.IsNullOrEmpty(e.Jugador_Objetivo)) return false;
        if (e.Recursos_Ofrecidos is null || e.Recursos_Recibidos is null) return false;

        var jugadorReceptor = jugadores.ListaJugadores.FirstOrDefault(j =>
            string.Equals(j.Nombre, e.Jugador_Objetivo, StringComparison.OrdinalIgnoreCase));
        if (jugadorReceptor is null) return false;

        // Validar fondos de ambas partes
        if (!TieneRecursosSuficientes(jugadorEmisor, e.Recursos_Ofrecidos)) return false;
        if (!TieneRecursosSuficientes(jugadorReceptor, e.Recursos_Recibidos)) return false;

        // Transferir del Emisor al Receptor
        foreach (var (recurso, cantidad) in e.Recursos_Ofrecidos)
        {
            jugadorEmisor.Recursos[recurso] -= cantidad;
            jugadorReceptor.Recursos[recurso] += cantidad;
        }

        // Transferir del Receptor al Emisor
        foreach (var (recurso, cantidad) in e.Recursos_Recibidos)
        {
            jugadorReceptor.Recursos[recurso] -= cantidad;
            jugadorEmisor.Recursos[recurso] += cantidad;
        }

        return true;
    }

    private bool MoverLadron(HistorialEvento e)
    {
        if (e.Id_Posicion_pieza is not int idCelda) return false;

        var destino = tablero.Celdas.FirstOrDefault(c => c.Id == idCelda);
        if (destino is null || destino.Has_Thief) return false;

        foreach (var celda in tablero.Celdas) celda.Has_Thief = false;
        destino.Has_Thief = true;
        return true;
    }

    private bool ProcesarRobarRecurso(HistorialEvento e, Jugador_Class jugadorLadron)
    {
        if (string.IsNullOrEmpty(e.Jugador_Objetivo)) return false;
        if (e.Recursos_Recibidos is null || e.Recursos_Recibidos.Count == 0) return false;

        var jugadorVictima = jugadores.ListaJugadores.FirstOrDefault(j =>
            string.Equals(j.Nombre, e.Jugador_Objetivo, StringComparison.OrdinalIgnoreCase));
        if (jugadorVictima is null) return false;

        // Se roba según los recursos indicados en Recursos_Recibidos
        if (!TieneRecursosSuficientes(jugadorVictima, e.Recursos_Recibidos)) return false;

        foreach (var (recurso, cantidad) in e.Recursos_Recibidos)
        {
            jugadorVictima.Recursos[recurso] -= cantidad;
            jugadorLadron.Recursos[recurso] += cantidad;
        }

        return true;
    }

    private bool ProcesarDescarte(HistorialEvento e, Jugador_Class jugador)
    {
        if (e.Recursos_Ofrecidos is null || e.Recursos_Ofrecidos.Count == 0) return false;
        if (!TieneRecursosSuficientes(jugador, e.Recursos_Ofrecidos)) return false;

        foreach (var (recurso, cantidad) in e.Recursos_Ofrecidos)
        {
            jugador.Recursos[recurso] -= cantidad;
        }

        return true;
    }

    private bool JugarCartaDesarrollo(HistorialEvento e, Jugador_Class jugador)
    {
        if (e.Tipo_Carta_Desarrollo is not TiposCartasDesarrollo tipoCarta) return false;

        // JugarCarta quita de mano y suma a CartasJugadas
        bool exitoso = jugador.JugarCarta(tipoCarta);
        if (!exitoso) return false;

        switch (tipoCarta)
        {
            case TiposCartasDesarrollo.Punto_Victoria:
                jugador.PuntosVictoria++;
                break;

            case TiposCartasDesarrollo.Carreteras:
                // Habilita las próximas 2 carreteras a costo $0
                _carreterasGratisRestantes = 2;
                break;

            case TiposCartasDesarrollo.Monopolio:
                ProcesarMonopolio(e, jugador);
                break;

            case TiposCartasDesarrollo.Invento:
            case TiposCartasDesarrollo.Caballero:
                // Como mencionaste, Invento se resuelve con Produccion_Recursos 
                // y Caballero con Mover_Ladron / Robar_Recurso.
                break;
        }

        return true;
    }

    private bool ProcesarTituloRuta(Jugador_Class jugador)
    {
        // Otorga 2 Puntos de Victoria por la Ruta Más Larga
        jugador.PuntosVictoria += 2;
        return true;
    }

    private bool ProcesarTituloEjercito(Jugador_Class jugador)
    {
        // Otorga 2 Puntos de Victoria por el Gran Ejército
        jugador.PuntosVictoria += 2;
        return true;
    }

    private bool FinDeTurno(Jugador_Class jugador)
    {
        if (!jugador.EsTurno) return false;

        if (ContadorTurnos <= 8)
        {
            // FASE INICIAL (Placement 1 al 8): Maneja el serpenteo (snake draft)
            jugadores.Pasar_Turno_Inicial(ContadorTurnos);
        }
        else
        {
            // FASE NORMAL: Rotación circular estándar
            jugadores.Pasar_Turno();
        }

        ContadorTurnos++;
        return true;
    }

    private bool ProcesarVictoria(HistorialEvento e, Jugador_Class jugador)
    {
        if (e.Puntos_Victoria is int puntos && puntos > 0)
        {
            jugador.PuntosVictoria = puntos;
        }
        return true;
    }

    // --- MÉTODOS AUXILIARES ---

    private static bool TieneRecursosSuficientes(Jugador_Class jugador, Dictionary<ResourceType, int> requeridos)
    {
        foreach (var (recurso, cantidad) in requeridos)
        {
            if (!jugador.Recursos.TryGetValue(recurso, out int disponible) || disponible < cantidad)
            {
                return false;
            }
        }
        return true;
    }

    private static void DescontarRecursos(Jugador_Class jugador, Dictionary<ResourceType, int> costo)
    {
        foreach (var (recurso, cantidad) in costo)
        {
            jugador.Recursos[recurso] -= cantidad;
        }
    }
    private void ProcesarMonopolio(HistorialEvento e, Jugador_Class jugadorBeneficiado)
    {
        // Se espera que el evento traiga el recurso seleccionado en Recursos_Recibidos
        if (e.Recursos_Recibidos is null || e.Recursos_Recibidos.Count == 0) return;

        ResourceType recursoElegido = e.Recursos_Recibidos.Keys.First();
        int totalRecolectado = 0;

        // Le quitamos todas las cartas de ese recurso a cada rival
        foreach (var rival in jugadores.ListaJugadores)
        {
            if (string.Equals(rival.Nombre, jugadorBeneficiado.Nombre, StringComparison.OrdinalIgnoreCase))
                continue;

            if (rival.Recursos.TryGetValue(recursoElegido, out int cantidadRival) && cantidadRival > 0)
            {
                totalRecolectado += cantidadRival;
                rival.Recursos[recursoElegido] = 0;
            }
        }

        // Le acreditamos el total al jugador que jugó el monopolio
        jugadorBeneficiado.Recursos[recursoElegido] += totalRecolectado;
    }

    /// <summary>
    /// Resetea el tablero y los jugadores a su estado inicial y re-aplica 
    /// los eventos contenidos en la lista mostrada.
    /// </summary>
    public void ReconstruirEstadoDesdeHistorial(List<HistorialEvento> eventosAMostrar)
    {
        // 1. Resetear Tablero
        foreach (var vertice in tablero.Vertices)
        {
            vertice.Structure = StructureType.None;
            vertice.PlayerColor = null;
        }

        foreach (var arista in tablero.Edges)
        {
            arista.HasRoad = false;
            arista.PlayerColor = null;
        }

        foreach (var celda in tablero.Celdas)
        {
            celda.Has_Thief = false; // Se puede reasignar al Desierto por defecto si lo deseas
        }

        // 2. Resetear Jugadores
        ContadorTurnos = 1;
        _carreterasGratisRestantes = 0;

        foreach (var j in jugadores.ListaJugadores)
        {
            j.PuntosVictoria = 0;
            j.EsTurno = false;

            // Resetear recursos a 0
            foreach (var recurso in j.Recursos.Keys.ToList())
            {
                j.Recursos[recurso] = 0;
            }

            // Resetear cartas a 0
            foreach (var carta in j.CartasEnMano.Keys.ToList())
            {
                j.CartasEnMano[carta] = 0;
                j.CartasJugadas[carta] = 0;
            }
        }

        // Primer jugador arranca con el turno
        if (jugadores.ListaJugadores.Count > 0)
        {
            jugadores.ListaJugadores[0].EsTurno = true;
        }

        // 3. Re-aplicar secuencialmente todos los eventos hasta la posición deseada
        foreach (var e in eventosAMostrar)
        {
            e.aplicado = false; // Forzamos la re-ejecución
            Aplicar(e);
        }

        // 4. Notificar a Blazor
        OnChange?.Invoke();
    }
}