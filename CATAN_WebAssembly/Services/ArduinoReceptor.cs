using System.Text.Json;
using System.Text.Json.Serialization;
using CATAN_WebAssembly.Pages.Partida;   // CAMBIO (tablero): Jugador_Class

namespace CATAN_WebAssembly.Services
{
    public class ArduinoReceptor
    {
        private static readonly JsonSerializerOptions _opciones = new()
        {
            PropertyNameCaseInsensitive = true,
            Converters = { new JsonStringEnumConverter() }
        };

        public ArduinoReceptor(ArduinoSerialService serial, PartidaService partida, HistorialService historial,
                               CatanBoardState tablero, Jugador_Class jugadores)   // CAMBIO (tablero)
        {
            serial.OnMessageReceived += json =>
            {
                HistorialEvento? evento;
                try { evento = JsonSerializer.Deserialize<HistorialEvento>(json, _opciones); }
                catch (JsonException) { return; } // no es un evento (ej. la seed del lobby): se ignora

                if (evento is not null && partida.Aplicar(evento))
                    historial.AgregarEvento(evento);
            };
            historial.OnEventoMostrado += async e =>
            {
                if (!serial.IsConnected) return;
                try
                {
                    // CAMBIO (tablero): al mostrar el primer evento del Replay (o si el Arduino
                    // todavia no lo tiene), primero se le manda el tablero
                    if (!serial.TableroEnviado || historial.Historial_Mostrar.Count == 1)
                        await serial.EnviarTableroAsync(tablero, jugadores);
                    await serial.SendJsonAsync(e);
                }
                catch (Exception) { /* puerto caído: la pantalla sigue sin el Arduino */ }
            };
            // CAMBIO (tablero): al ir para atras, el tablero fisico tambien vuelve
            historial.OnHistorialReconstruido += async eventos =>
            {
                if (!serial.IsConnected) return;
                try
                {
                    if (!serial.TableroEnviado) await serial.EnviarTableroAsync(tablero, jugadores);
                    await serial.ReconstruirTableroAsync(eventos);
                }
                catch (Exception) { /* puerto caído */ }
            };
        }

    }
}
