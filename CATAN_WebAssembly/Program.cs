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
builder.Services.AddScoped<Historial_Class>();
builder.Services.AddScoped<Jugador_Class>();
builder.Services.AddScoped<Parametros_Iniciales>();

builder.Services.AddScoped<ArduinoSerialService>();

builder.Services.AddMudServices();

await builder.Build().RunAsync();

