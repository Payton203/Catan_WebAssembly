using System.Text.Json;
using System.Text.Json.Serialization;

namespace CATAN_WebAssembly.Services
{
    public class ArduinoReceptor
    {
        private static readonly JsonSerializerOptions _opciones = new()
        {
            PropertyNameCaseInsensitive = true,
            Converters = { new JsonStringEnumConverter() }
        };

        public ArduinoReceptor(ArduinoSerialService serial, PartidaService partida, HistorialService historial, CatanBoardState tablero)
        {
            serial.OnMessageReceived += json =>
            {
                if (tablero.EjemploActivo) return;
                HistorialEvento? evento;
                try { evento = JsonSerializer.Deserialize<HistorialEvento>(json, _opciones); }
                catch (JsonException) { return; } // no es un evento (ej. la seed del lobby): se ignora

                if (evento is not null && partida.Aplicar(evento))
                    historial.AgregarEvento(evento);
            };
            historial.OnEventoMostrado += async e =>
            {
                if (!serial.IsConnected || tablero.EjemploActivo) return;
                try { await serial.SendJsonAsync(e); }
                catch (Exception) { /* puerto caído: la pantalla sigue sin el Arduino */ }
            };
        }

    }
}
