using Microsoft.JSInterop;
using CATAN_WebAssembly.Pages.Partida;   // CAMBIO (tablero): Jugador_Class
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CATAN_WebAssembly.Services;

/// <summary>
/// Wrapper de la Web Serial API para hablar con el Arduino por USB desde Blazor WASM,
/// usando un protocolo de líneas JSON (un mensaje = un string JSON terminado en '\n').
/// Registrar como Scoped en Program.cs: builder.Services.AddScoped&lt;ArduinoSerialService&gt;();
/// </summary>
public class ArduinoSerialService(IJSRuntime js) : IAsyncDisposable
{
    private IJSObjectReference? _module;
    private DotNetObjectReference<ArduinoSerialService>? _selfRef;

    /// <summary>Se dispara con cada línea JSON completa recibida (ya sin el '\n', lista para deserializar).</summary>
    public event Action<string>? OnMessageReceived;

    /// <summary>Se dispara si el puerto se cae o hay un error de lectura.</summary>
    public event Action<string>? OnError;

    public bool IsConnected { get; private set; }

    /// <summary>CAMBIO (tablero): ya se le mando el tablero (casillas y numeros) al Arduino.
    /// Vuelve a false al conectar, porque el Mega se reinicia y lo pierde.</summary>
    public bool TableroEnviado { get; private set; }

    // CAMBIO (tablero): el Arduino contesta "OK" o "ERROR ..." a cada mensaje y recien
    // ahi se manda el siguiente (mientras actualiza los LEDs no escucha el USB y perderia datos).
    // Al conectar, el Mega se reinicia y avisa "LISTO" cuando termina de arrancar.
    private readonly SemaphoreSlim _envio = new(1, 1);
    private TaskCompletionSource<string>? _respuesta;
    private TaskCompletionSource<bool>? _listo;
    private const int EsperaRespuestaMs = 2000;
    private const int EsperaListoMs = 5000;

    // CAMBIO (tablero): enums como texto y sin los campos null (lineas mas cortas y legibles).
    // El Arduino tambien entiende los enums como numero, pero asi se lee mejor en el monitor serie.
    private static readonly JsonSerializerOptions _opcionesArduino = new()
    {
        Converters = { new JsonStringEnumConverter() },
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    private async Task<IJSObjectReference> GetModuleAsync()
    {
        _module ??= await js.InvokeAsync<IJSObjectReference>(
            "import", "./js/serialInterop.js");
        return _module;
    }

    public async Task<bool> IsSupportedAsync()
    {
        var module = await GetModuleAsync();
        return await module.InvokeAsync<bool>("isSupported");
    }

    /// <summary>
    /// Abre el selector de puertos del navegador y conecta.
    /// IMPORTANTE: llamar esto desde el manejador de un click (ej. un botón "Conectar"),
    /// nunca desde OnInitializedAsync ni código automático — el navegador lo bloquea sin gesto de usuario.
    /// </summary>
    public async Task<bool> ConnectAsync(int baudRate = 115200)   // CAMBIO (tablero): misma velocidad que el Arduino
    {
        var module = await GetModuleAsync();
        _selfRef ??= DotNetObjectReference.Create(this);
        _listo = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        TableroEnviado = false;
        IsConnected = await module.InvokeAsync<bool>("requestAndOpen", baudRate, _selfRef);
        if (!IsConnected) return false;

        // CAMBIO (tablero): esperar a que el Mega termine de arrancar
        var primero = await Task.WhenAny(_listo.Task, Task.Delay(EsperaListoMs));
        if (primero != _listo.Task)
            Console.WriteLine("Tablero: no llego LISTO (se sigue igual)");
        return IsConnected;
    }

    /// <summary>Serializa el objeto a JSON y lo manda como una línea terminada en '\n'.</summary>
    /// CAMBIO (tablero): espera el "OK" del Arduino antes de volver. Devuelve la respuesta.
    public async Task<string> SendJsonAsync<T>(T mensaje)
    {
        var module = await GetModuleAsync();
        var json = JsonSerializer.Serialize(mensaje, _opcionesArduino);

        await _envio.WaitAsync();                  // de a un mensaje por vez, en orden
        try
        {
            _respuesta = new TaskCompletionSource<string>(TaskCreationOptions.RunContinuationsAsynchronously);
            await module.InvokeVoidAsync("sendJson", json);
            var primero = await Task.WhenAny(_respuesta.Task, Task.Delay(EsperaRespuestaMs));
            string r = primero == _respuesta.Task ? _respuesta.Task.Result : "ERROR sin respuesta del tablero";
            if (r.StartsWith("ERROR")) Console.WriteLine($"Tablero: {r}  <-  {json}");
            return r;
        }
        finally
        {
            _respuesta = null;
            _envio.Release();
        }
    }

    // ------------------------------------------------------------------
    // CAMBIO (tablero): mensajes propios del tablero (no son HistorialEvento)
    // ------------------------------------------------------------------

    /// <summary>
    /// Le manda al Arduino el tablero de la pagina: recurso y numero de las 19 celdas
    /// (en orden de Id, 1..19), y nombre y color de cada jugador. Arranca sin ladron,
    /// igual que la pagina al reconstruir la partida.
    /// </summary>
    public async Task<string> EnviarTableroAsync(CatanBoardState tablero, Jugador_Class jugadores)
    {
        var celdas = tablero.Celdas.OrderBy(c => c.Id).ToList();
        if (celdas.Count != 19) return "ERROR el tablero de la pagina no esta armado";

        string r = await SendJsonAsync(new
        {
            Tipo = "Tablero",
            Jugadores = jugadores.ListaJugadores.Select(j => j.Nombre).ToList(),
            Colores = jugadores.ListaJugadores.Select(j => j.Color).ToList(),   // "#00FF00"...
            Casillas = celdas.Select(c => c.Recurso.ToString()).ToList(),       // Wood..Desert
            Numeros = celdas.Select(c => c.Label ?? 0).ToList(),                // desierto = 0
            Ladron = 0                                                          // 0 = sin ladron
        });
        TableroEnviado = r.StartsWith("OK");
        return r;
    }

    /// <summary>Apaga los caminos y devuelve el ladron a su casilla inicial (el tablero queda).</summary>
    public Task<string> ReiniciarTableroAsync() => SendJsonAsync(new { Tipo = "Reiniciar" });

    /// <summary>Deja el tablero fisico igual a la lista de eventos (para ir para atras).</summary>
    public async Task ReconstruirTableroAsync(IEnumerable<HistorialEvento> eventos)
    {
        await ReiniciarTableroAsync();
        foreach (var e in eventos.ToList())
            if (e.aplicado) await SendJsonAsync(e);
    }

    public async Task DisconnectAsync()
    {
        if (_module is not null)
        {
            await _module.InvokeVoidAsync("close");
        }
        IsConnected = false;
    }

    /// <summary>
    /// Genera mensajes JSON falsos periódicamente, sin Arduino ni navigator.serial de por medio.
    /// Dispara el mismo evento OnMessageReceived que la conexión real, así podés
    /// desarrollar y probar el parser y la UI antes de tener el hardware a mano.
    /// </summary>
    //public async Task StartSimulationAsync(int intervalMs = 1000)
    //{
    //    var module = await GetModuleAsync();
    //    _selfRef ??= DotNetObjectReference.Create(this);
    //    await module.InvokeVoidAsync("startSimulation", _selfRef, intervalMs);
    //    IsConnected = true;
    //}

    //public async Task StopSimulationAsync()
    //{
    //    if (_module is not null)
    //    {
    //        await _module.InvokeVoidAsync("stopSimulation");
    //    }
    //    IsConnected = false;
    //}

    ///// <summary>Manda un único mensaje simulado (el objeto que le pases, ya serializado a JSON) sin esperar al timer.</summary>
    //public async Task SimulateMessageAsync<T>(T mensaje)
    //{
    //    var module = await GetModuleAsync();
    //    _selfRef ??= DotNetObjectReference.Create(this);
    //    var json = JsonSerializer.Serialize(mensaje);
    //    await module.InvokeVoidAsync("sendSimulatedMessage", _selfRef, json);
    //}

    [JSInvokable]
    public Task OnSerialLineReceived(string json)
    {
        // CAMBIO (tablero): las respuestas del Arduino no son eventos, no se pasan al receptor
        string linea = json.Trim();
        if (linea == "LISTO") { _listo?.TrySetResult(true); return Task.CompletedTask; }
        if (linea.StartsWith("OK") || linea.StartsWith("ERROR"))
        {
            _respuesta?.TrySetResult(linea);
            return Task.CompletedTask;
        }

        OnMessageReceived?.Invoke(json);
        return Task.CompletedTask;
    }

    [JSInvokable]
    public Task OnSerialError(string message)
    {
        IsConnected = false;
        OnError?.Invoke(message);
        return Task.CompletedTask;
    }

    public async ValueTask DisposeAsync()
    {
        if (_module is not null)
        {
            await _module.InvokeVoidAsync("close");
            await _module.DisposeAsync();
        }
        _selfRef?.Dispose();
        GC.SuppressFinalize(this);
    }
    private static readonly JsonSerializerOptions _opcionesEnvio = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };
}
