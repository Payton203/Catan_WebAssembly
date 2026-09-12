using System.Text.Json;
using Microsoft.JSInterop;

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
    public async Task<bool> ConnectAsync(int baudRate = 9600)
    {
        var module = await GetModuleAsync();
        _selfRef ??= DotNetObjectReference.Create(this);
        IsConnected = await module.InvokeAsync<bool>("requestAndOpen", baudRate, _selfRef);
        return IsConnected;
    }

    /// <summary>Serializa el objeto a JSON y lo manda como una línea terminada en '\n'.</summary>
    public async Task SendJsonAsync<T>(T mensaje)
    {
        var module = await GetModuleAsync();
        var json = JsonSerializer.Serialize(mensaje);
        await module.InvokeVoidAsync("sendJson", json);
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
}
