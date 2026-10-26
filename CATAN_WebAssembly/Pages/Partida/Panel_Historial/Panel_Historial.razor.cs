using CATAN_WebAssembly.Pages.Partida.Panel_Tablero;

namespace CATAN_WebAssembly.Pages.Partida.Panel_Historial
{
    public partial class Panel_Historial
    {
        void partida1()
        {
            // --- RONDA 1 INICIAL (Jugadores 0 -> 1 -> 2 -> 3) ---

            // Jugador 0 (Verde / Rojo): Busca buena combinación 6 Trigo y 3 Madera
            Historial_.AgregarEvento(new HistorialEvento
            {
                Tipo = TipoEvento.Construccion_Estructura,
                Jugador = Jugador_.ListaJugadores[0].Nombre,
                Id_Posicion_pieza = 9,
                Estructura_Vertice = StructureType.Settlement
            });
            Historial_.AgregarEvento(new HistorialEvento
            {
                Tipo = TipoEvento.Construccion_Carretera,
                Jugador = Jugador_.ListaJugadores[0].Nombre,
                Id_Posicion_pieza = 9
            });
            Historial_.AgregarEvento(new HistorialEvento { Tipo = TipoEvento.Fin_De_Turno, Jugador = Jugador_.ListaJugadores[0].Nombre });

            // Jugador 1 (Azul / Naranja): Se posiciona en 8 Madera y 3 Madera (Vértice 13)
            Historial_.AgregarEvento(new HistorialEvento
            {
                Tipo = TipoEvento.Construccion_Estructura,
                Jugador = Jugador_.ListaJugadores[1].Nombre,
                Id_Posicion_pieza = 13,
                Estructura_Vertice = StructureType.Settlement
            });
            Historial_.AgregarEvento(new HistorialEvento
            {
                Tipo = TipoEvento.Construccion_Carretera,
                Jugador = Jugador_.ListaJugadores[1].Nombre,
                Id_Posicion_pieza = 24
            });
            Historial_.AgregarEvento(new HistorialEvento { Tipo = TipoEvento.Fin_De_Turno, Jugador = Jugador_.ListaJugadores[1].Nombre });

            // Jugador 2 (Amarillo / Azul): Se ubica en 6 Madera (Vértice 44)
            Historial_.AgregarEvento(new HistorialEvento
            {
                Tipo = TipoEvento.Construccion_Estructura,
                Jugador = Jugador_.ListaJugadores[2].Nombre,
                Id_Posicion_pieza = 44,
                Estructura_Vertice = StructureType.Settlement
            });
            Historial_.AgregarEvento(new HistorialEvento
            {
                Tipo = TipoEvento.Construccion_Carretera,
                Jugador = Jugador_.ListaJugadores[2].Nombre,
                Id_Posicion_pieza = 66
            });
            Historial_.AgregarEvento(new HistorialEvento { Tipo = TipoEvento.Fin_De_Turno, Jugador = Jugador_.ListaJugadores[2].Nombre });

            // Jugador 3 (Magenta / Violeta): Ocupa 8 Oveja y 4 Ladrillo (Vértice 30)
            Historial_.AgregarEvento(new HistorialEvento
            {
                Tipo = TipoEvento.Construccion_Estructura,
                Jugador = Jugador_.ListaJugadores[3].Nombre,
                Id_Posicion_pieza = 30,
                Estructura_Vertice = StructureType.Settlement
            });
            Historial_.AgregarEvento(new HistorialEvento
            {
                Tipo = TipoEvento.Construccion_Carretera,
                Jugador = Jugador_.ListaJugadores[3].Nombre,
                Id_Posicion_pieza = 49
            });
            // El jugador 3 repite turno inmediatamente por el serpenteo (Turno 4 al 5)
            Historial_.AgregarEvento(new HistorialEvento { Tipo = TipoEvento.Fin_De_Turno, Jugador = Jugador_.ListaJugadores[3].Nombre });

            // --- RONDA 2 INICIAL (Jugadores 3 -> 2 -> 1 -> 0) ---

            // Jugador 3: Segunda casa en 10 Ladrillo y 5 Piedra (Vértice 33)
            Historial_.AgregarEvento(new HistorialEvento
            {
                Tipo = TipoEvento.Construccion_Estructura,
                Jugador = Jugador_.ListaJugadores[3].Nombre,
                Id_Posicion_pieza = 33,
                Estructura_Vertice = StructureType.Settlement
            });
            Historial_.AgregarEvento(new HistorialEvento
            {
                Tipo = TipoEvento.Construccion_Carretera,
                Jugador = Jugador_.ListaJugadores[3].Nombre,
                Id_Posicion_pieza = 48
            });
            Historial_.AgregarEvento(new HistorialEvento { Tipo = TipoEvento.Fin_De_Turno, Jugador = Jugador_.ListaJugadores[3].Nombre });

            // Jugador 2: Segunda casa en 10 Ladrillo y 9 Ladrillo (Vértice 14)
            Historial_.AgregarEvento(new HistorialEvento
            {
                Tipo = TipoEvento.Construccion_Estructura,
                Jugador = Jugador_.ListaJugadores[2].Nombre,
                Id_Posicion_pieza = 14,
                Estructura_Vertice = StructureType.Settlement
            });
            Historial_.AgregarEvento(new HistorialEvento
            {
                Tipo = TipoEvento.Construccion_Carretera,
                Jugador = Jugador_.ListaJugadores[2].Nombre,
                Id_Posicion_pieza = 18
            });
            Historial_.AgregarEvento(new HistorialEvento { Tipo = TipoEvento.Fin_De_Turno, Jugador = Jugador_.ListaJugadores[2].Nombre });

            // Jugador 1: Segunda casa en 11 Piedra y 10 Ladrillo (Vértice 21)
            Historial_.AgregarEvento(new HistorialEvento
            {
                Tipo = TipoEvento.Construccion_Estructura,
                Jugador = Jugador_.ListaJugadores[1].Nombre,
                Id_Posicion_pieza = 21,
                Estructura_Vertice = StructureType.Settlement
            });
            Historial_.AgregarEvento(new HistorialEvento
            {
                Tipo = TipoEvento.Construccion_Carretera,
                Jugador = Jugador_.ListaJugadores[1].Nombre,
                Id_Posicion_pieza = 39
            });
            Historial_.AgregarEvento(new HistorialEvento { Tipo = TipoEvento.Fin_De_Turno, Jugador = Jugador_.ListaJugadores[1].Nombre });

            // Jugador 0: Segunda casa en 5 Trigo y 10 Oveja (Vértice 15)
            Historial_.AgregarEvento(new HistorialEvento
            {
                Tipo = TipoEvento.Construccion_Estructura,
                Jugador = Jugador_.ListaJugadores[0].Nombre,
                Id_Posicion_pieza = 15,
                Estructura_Vertice = StructureType.Settlement
            });
            Historial_.AgregarEvento(new HistorialEvento
            {
                Tipo = TipoEvento.Construccion_Carretera,
                Jugador = Jugador_.ListaJugadores[0].Nombre,
                Id_Posicion_pieza = 31
            });
            // Historial_.AgregarEvento(new HistorialEvento { Tipo = TipoEvento.Fin_De_Turno, Jugador = Jugador_.ListaJugadores[0].Nombre });


            // ==========================================
            // FASE 2: FASE NORMAL DE LA PARTIDA (TURNO 9+)
            // ==========================================







            // =========================================================================
            // PREPARACIÓN: PRODUCCIÓN INICIAL DE RECURSOS PARA COMPRAS Y MONOPOLIO
            // =========================================================================

            // Todos reciben recursos iniciales suficientes para comprar cartas de desarrollo
            // y acumular Madera para hacer visible la acción del Monopolio.

            // Jugador 0 acumula Madera masiva
            Historial_.AgregarEvento(new HistorialEvento
            {
                Tipo = TipoEvento.Produccion_Recursos,
                Jugador = Jugador_.ListaJugadores[0].Nombre,
                Recursos_Recibidos = new Dictionary<ResourceType, int>
    {
        { ResourceType.Wood, 4 },
        { ResourceType.Sheep, 2 },
        { ResourceType.Wheat, 2 },
        { ResourceType.Rock, 2 }
    }
            });

            // Jugador 1 acumula Madera masiva y costo de compra
            Historial_.AgregarEvento(new HistorialEvento
            {
                Tipo = TipoEvento.Produccion_Recursos,
                Jugador = Jugador_.ListaJugadores[1].Nombre,
                Recursos_Recibidos = new Dictionary<ResourceType, int>
    {
        { ResourceType.Wood, 5 },
        { ResourceType.Sheep, 2 },
        { ResourceType.Wheat, 2 },
        { ResourceType.Rock, 2 }
    }
            });

            // Jugador 2 acumula Madera masiva y costo de compra
            Historial_.AgregarEvento(new HistorialEvento
            {
                Tipo = TipoEvento.Produccion_Recursos,
                Jugador = Jugador_.ListaJugadores[2].Nombre,
                Recursos_Recibidos = new Dictionary<ResourceType, int>
    {
        { ResourceType.Wood, 3 },
        { ResourceType.Sheep, 2 },
        { ResourceType.Wheat, 2 },
        { ResourceType.Rock, 2 }
    }
            });

            // Jugador 3 se prepara para ser el ejecutor del Monopolio
            Historial_.AgregarEvento(new HistorialEvento
            {
                Tipo = TipoEvento.Produccion_Recursos,
                Jugador = Jugador_.ListaJugadores[3].Nombre,
                Recursos_Recibidos = new Dictionary<ResourceType, int>
    {
        { ResourceType.Sheep, 3 },
        { ResourceType.Wheat, 3 },
        { ResourceType.Rock, 3 }
    }
            });

            // =========================================================================
            // PRUEBA 1: COMPRA Y USO DE CARTA "CABALLERO" (Jugador 0)
            // =========================================================================

            // 1. Compra la carta
            Historial_.AgregarEvento(new HistorialEvento
            {
                Tipo = TipoEvento.Comprar_Carta_Desarrollo,
                Jugador = Jugador_.ListaJugadores[0].Nombre,
                Tipo_Carta_Desarrollo = TiposCartasDesarrollo.Caballero
            });

            // 2. La juega en su turno
            Historial_.AgregarEvento(new HistorialEvento
            {
                Tipo = TipoEvento.Jugar_Carta_Desarrollo,
                Jugador = Jugador_.ListaJugadores[0].Nombre,
                Tipo_Carta_Desarrollo = TiposCartasDesarrollo.Caballero
            });

            // 3. Mueve al Ladrón
            Historial_.AgregarEvento(new HistorialEvento
            {
                Tipo = TipoEvento.Mover_Ladron,
                Jugador = Jugador_.ListaJugadores[0].Nombre,
                Id_Posicion_pieza = 6
            });

            // 4. Roba 1 recurso al objetivo
            Historial_.AgregarEvento(new HistorialEvento
            {
                Tipo = TipoEvento.Robar_Recurso,
                Jugador = Jugador_.ListaJugadores[0].Nombre,
                Jugador_Objetivo = Jugador_.ListaJugadores[1].Nombre,
                Recursos_Recibidos = new Dictionary<ResourceType, int> { { ResourceType.Wood, 1 } }
            });

            Historial_.AgregarEvento(new HistorialEvento { Tipo = TipoEvento.Fin_De_Turno, Jugador = Jugador_.ListaJugadores[0].Nombre });


            // =========================================================================
            // PRUEBA 2: COMPRA Y USO DE CARTA "CARRETERAS" (Jugador 1)
            // =========================================================================

            // 1. Compra la carta
            Historial_.AgregarEvento(new HistorialEvento
            {
                Tipo = TipoEvento.Comprar_Carta_Desarrollo,
                Jugador = Jugador_.ListaJugadores[1].Nombre,
                Tipo_Carta_Desarrollo = TiposCartasDesarrollo.Carreteras
            });

            // 2. Juega la carta de carreteras (activa _carreterasGratisRestantes = 2)
            Historial_.AgregarEvento(new HistorialEvento
            {
                Tipo = TipoEvento.Jugar_Carta_Desarrollo,
                Jugador = Jugador_.ListaJugadores[1].Nombre,
                Tipo_Carta_Desarrollo = TiposCartasDesarrollo.Carreteras
            });

            // 3. Construye la 1ra carretera GRATIS
            Historial_.AgregarEvento(new HistorialEvento
            {
                Tipo = TipoEvento.Construccion_Carretera,
                Jugador = Jugador_.ListaJugadores[1].Nombre,
                Id_Posicion_pieza = 24
            });

            // 4. Construye la 2da carretera GRATIS
            Historial_.AgregarEvento(new HistorialEvento
            {
                Tipo = TipoEvento.Construccion_Carretera,
                Jugador = Jugador_.ListaJugadores[1].Nombre,
                Id_Posicion_pieza = 28
            });

            Historial_.AgregarEvento(new HistorialEvento { Tipo = TipoEvento.Fin_De_Turno, Jugador = Jugador_.ListaJugadores[1].Nombre });


            // =========================================================================
            // PRUEBA 3: COMPRA Y USO DE CARTA "INVENTO" (Jugador 2)
            // =========================================================================

            // 1. Compra la carta
            Historial_.AgregarEvento(new HistorialEvento
            {
                Tipo = TipoEvento.Comprar_Carta_Desarrollo,
                Jugador = Jugador_.ListaJugadores[2].Nombre,
                Tipo_Carta_Desarrollo = TiposCartasDesarrollo.Invento
            });

            // 2. Juega la carta
            Historial_.AgregarEvento(new HistorialEvento
            {
                Tipo = TipoEvento.Jugar_Carta_Desarrollo,
                Jugador = Jugador_.ListaJugadores[2].Nombre,
                Tipo_Carta_Desarrollo = TiposCartasDesarrollo.Invento
            });

            // 3. Recibe los 2 recursos a elección mediante Produccion_Recursos
            Historial_.AgregarEvento(new HistorialEvento
            {
                Tipo = TipoEvento.Produccion_Recursos,
                Jugador = Jugador_.ListaJugadores[2].Nombre,
                Recursos_Recibidos = new Dictionary<ResourceType, int>
    {
        { ResourceType.Brick, 1 },
        { ResourceType.Rock, 1 }
    }
            });

            Historial_.AgregarEvento(new HistorialEvento { Tipo = TipoEvento.Fin_De_Turno, Jugador = Jugador_.ListaJugadores[2].Nombre });


            // =========================================================================
            // PRUEBA 4: COMPRA Y USO DE CARTA "MONOPOLIO" (Jugador 3)
            // =========================================================================

            // 1. Compra la carta
            Historial_.AgregarEvento(new HistorialEvento
            {
                Tipo = TipoEvento.Comprar_Carta_Desarrollo,
                Jugador = Jugador_.ListaJugadores[3].Nombre,
                Tipo_Carta_Desarrollo = TiposCartasDesarrollo.Monopolio
            });

            // 2. Juega Monopolio eligiendo 'Wood' (Madera).
            // En este punto:
            // Jugador 0 tiene ~4 Mader
            // Jugador 1 tiene ~5 Mader
            // Jugador 2 tiene ~3 Mader
            // El método ProcesarMonopolio dejará en 0 a los rivales y le sumará ~12 de Madera al Jugador 3.
            Historial_.AgregarEvento(new HistorialEvento
            {
                Tipo = TipoEvento.Jugar_Carta_Desarrollo,
                Jugador = Jugador_.ListaJugadores[3].Nombre,
                Tipo_Carta_Desarrollo = TiposCartasDesarrollo.Monopolio,
                Recursos_Recibidos = new Dictionary<ResourceType, int>
    {
        { ResourceType.Wood, 1 } // Indica qué recurso se monopoliza
    }
            });

            Historial_.AgregarEvento(new HistorialEvento { Tipo = TipoEvento.Fin_De_Turno, Jugador = Jugador_.ListaJugadores[3].Nombre });


            // =========================================================================
            // PRUEBA 5: COMPRA Y USO DE CARTA "PUNTO DE VICTORIA" (Jugador 0)
            // =========================================================================

            // 1. Jugador 0 vuelve a comprar una carta
            Historial_.AgregarEvento(new HistorialEvento
            {
                Tipo = TipoEvento.Comprar_Carta_Desarrollo,
                Jugador = Jugador_.ListaJugadores[0].Nombre,
                Tipo_Carta_Desarrollo = TiposCartasDesarrollo.Punto_Victoria
            });

            // 2. Revela el Punto de Victoria (Suma 1 PuntosVictoria al jugador en PartidaService)
            Historial_.AgregarEvento(new HistorialEvento
            {
                Tipo = TipoEvento.Jugar_Carta_Desarrollo,
                Jugador = Jugador_.ListaJugadores[0].Nombre,
                Tipo_Carta_Desarrollo = TiposCartasDesarrollo.Punto_Victoria
            });

            Historial_.AgregarEvento(new HistorialEvento { Tipo = TipoEvento.Fin_De_Turno, Jugador = Jugador_.ListaJugadores[0].Nombre });


        }
    }
}
