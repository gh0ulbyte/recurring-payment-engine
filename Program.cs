using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using DebitosAutomaticos.Services;
using System;
using System.IO;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddScoped<DebitoAutomaticoService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseRouting();
app.UseStaticFiles();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Test}/{action=Index}/{id?}");

// Rutas alias que imitan paths de producción (sin tocar lógica)
app.MapControllerRoute(
    name: "alias_adhesion_get",
    pattern: "DebitoAutomatico/Adhesion",
    defaults: new { controller = "Test", action = "Adhesion" }
);

app.MapControllerRoute(
    name: "alias_adhesion_post",
    pattern: "DebitoAutomatico/Procesar",
    defaults: new { controller = "Test", action = "ProcesarAdhesion" }
);

Console.WriteLine(" Iniciando servidor de Débitos Automáticos...");
Console.WriteLine(" Directorio de trabajo: " + Directory.GetCurrentDirectory());
Console.WriteLine(" Servidor iniciado correctamente");
Console.WriteLine(" Navega a: http://localhost:5000/Test");
Console.WriteLine("  Presiona Ctrl+C para detener el servidor");

app.Run();

