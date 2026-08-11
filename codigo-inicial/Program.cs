using System.Text.Json;

// =====================================================================
//  CODIGO INICIAL - PRACTICA S01 (Minimal API)
//
//  ADVERTENCIA: este archivo funciona, pero esta HECHO A PROPOSITO MAL.
//  Todo vive amontonado en Program.cs: validacion + persistencia +
//  serializacion + logging, todo mezclado en cada endpoint.
//
//  TU TRABAJO NO ES REESCRIBIRLO DESDE CERO.
//  Tu trabajo es ORDENARLO en capas (Domain / Application /
//  Infrastructure / Host) aplicando lo de la S01. Ver todo.md.
// =====================================================================

var builder = WebApplication.CreateBuilder(args);

// el connection string ahora se LEE de configuracion, no se hardcodea (Factor III)
var connectionString = builder.Configuration.GetConnectionString("Default")
    ?? throw new InvalidOperationException("Falta la connection string 'Default'.");

builder.Services.AddScoped<ICommunityRepository, InMemoryCommunityRepository>();
builder.Services.AddScoped<CreateCommunityHandler>();
builder.Services.AddScoped<GetCommunityHandler>();
builder.Services.AddScoped<RenameCommunityHandler>();

var app = builder.Build();
app.MapCommunityEndpoints();
app.Run();
