using CATAN_WebAssembly;
using CATAN_WebAssembly.Layout;
using CATAN_WebAssembly.Pages.Partida;
using CATAN_WebAssembly.Services;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

builder.Services.AddScoped<Personalizacion_Pagina>(); //registra la clase Personalizacion_pagina

// Los usa ArduinoReceptor (singleton), así que también tienen que ser singleton
builder.Services.AddSingleton<ArduinoSerialService>();
builder.Services.AddSingleton<HistorialService>();
builder.Services.AddSingleton<Jugador_Class>();
builder.Services.AddSingleton<CatanBoardState>();
builder.Services.AddSingleton<PartidaService>();
builder.Services.AddSingleton<Parametros_Iniciales>();
builder.Services.AddSingleton<ArduinoReceptor>();

builder.Services.AddMudServices();

var host = builder.Build();
host.Services.GetRequiredService<ArduinoReceptor>();
await host.RunAsync();

