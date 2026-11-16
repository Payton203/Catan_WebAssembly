namespace CATAN_WebAssembly.Models;

public class JugadorEstadistica
{
    public string Nombre { get; set; } = "";
    public int Indice { get; set; }

    // Puntos
    public int PuntosVisibles { get; set; }       // construcciones + títulos
    public int CartasPuntoVictoria { get; set; }  // puntos ocultos
    public int PuntosTotales => PuntosVisibles + CartasPuntoVictoria;

    // Construcciones
    public int PobladosConstruidos { get; set; }  // incluye los que luego se mejoraron a ciudad
    public int CiudadesConstruidas { get; set; }
    public int PobladosActuales => PobladosConstruidos - CiudadesConstruidas;
    public int Caminos { get; set; }
    public int LongitudRuta { get; set; }
    public bool GranRuta { get; set; }
    public bool Ejercito { get; set; }

    // Recursos producidos por los dados (incluye los recursos iniciales)
    public Dictionary<string, int> Producido { get; set; } = new();
    public int TotalProducido => Producido.Values.Sum();

    // Acciones
    public int ComerciosJugadores { get; set; }   // participó proponiendo o aceptando
    public int ComerciosBanca { get; set; }
    public int CartasCompradas { get; set; }
    public int CartasJugadas { get; set; }
    public int LadronesMovidos { get; set; }
    public int RobosHechos { get; set; }
    public int RobosSufridos { get; set; }
    public int Descartes { get; set; }
}

public class EstadisticasPartida
{
    public int Seed { get; set; }
    public string Modo { get; set; } = "";
    public string Reglas { get; set; } = "";
    public string Alcance { get; set; } = "";
    public string Auditoria { get; set; } = "";

    public string Ganador { get; set; } = "";
    public int PuntosGanador { get; set; }
    public int Turnos { get; set; }
    public int Eventos { get; set; }
    public TimeSpan Duracion { get; set; }

    public List<JugadorEstadistica> Jugadores { get; set; } = new();

    // Linea[jugador][turno] = puntos visibles al terminar ese turno
    public int[][] Linea { get; set; } = Array.Empty<int[]>();

    // DadosConteo[suma] para suma = 2..12 (los índices 0 y 1 quedan en 0)
    public int[] DadosConteo { get; set; } = new int[13];
    public int TotalTiradas => DadosConteo.Sum();
    public double DadosEsperados(int suma) => TotalTiradas * Math.Max(0, 6 - Math.Abs(7 - suma)) / 36.0;

    public Dictionary<string, int> CartasCompradas { get; set; } = new();
    public Dictionary<string, int> CartasJugadas { get; set; } = new();

    public static EstadisticasPartida Calcular(PartidaArchivo p)
    {
        var r = new EstadisticasPartida
        {
            Seed = p.Seed,
            Modo = p.Modo,
            Reglas = p.Reglas,
            Alcance = p.Alcance,
            Auditoria = p.Resumen.Auditoria,
            Eventos = p.Eventos.Count,
        };

        var eventos = p.Eventos;
        int maxTurno = eventos.Count == 0 ? 0 : eventos.Max(e => e.Turno);
        r.Turnos = maxTurno;

        if (eventos.Count > 1)
            r.Duracion = eventos.Max(e => e.Momento) - eventos.Min(e => e.Momento);

        r.Jugadores = p.Jugadores
            .Select((nombre, i) => new JugadorEstadistica
            {
                Nombre = nombre,
                Indice = i,
                LongitudRuta = p.Resumen.Longitudes.GetValueOrDefault(nombre),
            })
            .ToList();

        var porNombre = r.Jugadores.ToDictionary(j => j.Nombre);

        JugadorEstadistica? Buscar(string? nombre) =>
            nombre != null && porNombre.TryGetValue(nombre, out var j) ? j : null;

        string? titularGranRuta = null;
        string? titularEjercito = null;

        // ───────── Conteos generales ─────────
        foreach (var e in eventos)
        {
            var j = Buscar(e.Jugador);
            if (j == null) continue;

            switch (e.Tipo)
            {
                case "Construccion_Estructura":
                    if (e.Estructura_Vertice == "City") j.CiudadesConstruidas++;
                    else j.PobladosConstruidos++;
                    break;

                case "Construccion_Carretera":
                    j.Caminos++;
                    break;

                case "Produccion_Recursos":
                    if (e.Recursos_Recibidos != null)
                        foreach (var kv in e.Recursos_Recibidos)
                            j.Producido[kv.Key] = j.Producido.GetValueOrDefault(kv.Key) + kv.Value;
                    break;

                case "Comercio_Jugadores":
                {
                    j.ComerciosJugadores++;
                    var objetivo = Buscar(e.Jugador_Objetivo);
                    if (objetivo != null) objetivo.ComerciosJugadores++;
                    break;
                }

                case "Comercio_Banca":
                    j.ComerciosBanca++;
                    break;

                case "Comprar_Carta_Desarrollo":
                {
                    j.CartasCompradas++;
                    var tipo = e.Tipo_Carta_Desarrollo ?? "";
                    r.CartasCompradas[tipo] = r.CartasCompradas.GetValueOrDefault(tipo) + 1;
                    if (tipo == "Punto_Victoria") j.CartasPuntoVictoria++;
                    break;
                }

                case "Jugar_Carta_Desarrollo":
                {
                    j.CartasJugadas++;
                    var tipo = e.Tipo_Carta_Desarrollo ?? "";
                    r.CartasJugadas[tipo] = r.CartasJugadas.GetValueOrDefault(tipo) + 1;
                    break;
                }

                case "Mover_Ladron":
                    j.LadronesMovidos++;
                    break;

                case "Robar_Recurso":
                {
                    j.RobosHechos++;
                    var victima = Buscar(e.Jugador_Objetivo);
                    if (victima != null) victima.RobosSufridos++;
                    break;
                }

                case "Descarte_Por_Siete":
                    j.Descartes++;
                    break;

                case "Tirada_Dados":
                    if (e.Resultado_Dados is >= 2 and <= 12)
                        r.DadosConteo[e.Resultado_Dados.Value]++;
                    break;

                case "Obtener_Gran_Ruta":
                    titularGranRuta = e.Jugador;
                    break;

                case "Obtener_Ejercito":
                    titularEjercito = e.Jugador;
                    break;

                case "Victoria":
                    r.Ganador = e.Jugador;
                    r.PuntosGanador = e.Puntos_Victoria ?? 0;
                    break;
            }
        }

        if (string.IsNullOrEmpty(r.Ganador)) r.Ganador = p.Resumen.Ganador;

        foreach (var j in r.Jugadores)
        {
            j.GranRuta = j.Nombre == titularGranRuta;
            j.Ejercito = j.Nombre == titularEjercito;
        }

        // ───────── Evolución de puntos visibles por turno ─────────
        // Poblado +1, mejora a ciudad +1, y cada título vale 2 puntos
        // (el título pasa de un jugador a otro: el anterior pierde 2).
        int n = r.Jugadores.Count;
        var puntos = new int[n];
        r.Linea = r.Jugadores.Select(_ => new int[maxTurno + 1]).ToArray();

        JugadorEstadistica? holderRuta = null;
        JugadorEstadistica? holderEjercito = null;

        for (int t = 0; t <= maxTurno; t++)
        {
            foreach (var e in eventos.Where(x => x.Turno == t))
            {
                var j = Buscar(e.Jugador);
                if (j == null) continue;

                switch (e.Tipo)
                {
                    case "Construccion_Estructura":
                        puntos[j.Indice] += 1;
                        break;

                    case "Obtener_Gran_Ruta":
                        if (holderRuta != null) puntos[holderRuta.Indice] -= 2;
                        holderRuta = j;
                        puntos[j.Indice] += 2;
                        break;

                    case "Obtener_Ejercito":
                        if (holderEjercito != null) puntos[holderEjercito.Indice] -= 2;
                        holderEjercito = j;
                        puntos[j.Indice] += 2;
                        break;
                }
            }

            for (int i = 0; i < n; i++) r.Linea[i][t] = puntos[i];
        }

        for (int i = 0; i < n; i++) r.Jugadores[i].PuntosVisibles = puntos[i];

        return r;
    }
}
