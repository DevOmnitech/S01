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
var app = builder.Build();

// "base de datos" global en memoria (una lista suelta, sin entidad de dominio)
var communities = new List<Dictionary<string, object>>();
var nextId = 1;

// connection string escrito directo en el codigo (con password incluido)
var connectionString = "Host=localhost;Port=5432;Database=reddit;Username=dev;Password=changeme";

// ---------------------------------------------------------------------
// POST /communities  ->  crear una comunidad
// Este endpoint valida, guarda, loguea y serializa: TODO aqui adentro.
// ---------------------------------------------------------------------
app.MapPost("/communities", (CreateCommunityDto dto) =>
{
    // validacion inline
    if (string.IsNullOrWhiteSpace(dto.Name))
        return Results.BadRequest("El nombre es requerido.");
    if (dto.Name.Length < 3 || dto.Name.Length > 21)
        return Results.BadRequest("El nombre debe tener entre 3 y 21 caracteres.");
    foreach (var ch in dto.Name)
    {
        if (!char.IsLetterOrDigit(ch) && ch != '_')
            return Results.BadRequest("Solo se permiten letras, numeros y guion bajo.");
    }

    // persistencia inline (guardando en un diccionario suelto)
    var entity = new Dictionary<string, object>
    {
        ["id"] = nextId,
        ["name"] = dto.Name.ToLower(),
        ["createdAt"] = DateTime.UtcNow
    };
    communities.Add(entity);
    nextId++;

    // log con string interpolation y filtrando el connection string con el password
    app.Logger.LogInformation($"Comunidad creada: {dto.Name} (id {entity["id"]}) usando {connectionString}");

    // serializacion inline
    var json = JsonSerializer.Serialize(new { id = entity["id"], name = entity["name"] });
    return Results.Text(json, "application/json");
});

// ---------------------------------------------------------------------
// PUT /communities/{id}/rename  ->  renombrar una comunidad
// Fijate que la MISMA validacion de nombre esta copiada aqui otra vez.
// ---------------------------------------------------------------------
app.MapPut("/communities/{id}/rename", (int id, RenameCommunityDto dto) =>
{
    // validacion inline DUPLICADA (identica a la del POST)
    if (string.IsNullOrWhiteSpace(dto.Name))
        return Results.BadRequest("El nombre es requerido.");
    if (dto.Name.Length < 3 || dto.Name.Length > 21)
        return Results.BadRequest("El nombre debe tener entre 3 y 21 caracteres.");
    foreach (var ch in dto.Name)
    {
        if (!char.IsLetterOrDigit(ch) && ch != '_')
            return Results.BadRequest("Solo se permiten letras, numeros y guion bajo.");
    }

    var found = communities.FirstOrDefault(x => (int)x["id"] == id);
    if (found is null)
        return Results.NotFound($"No existe la comunidad {id}.");

    found["name"] = dto.Name.ToLower();
    app.Logger.LogInformation($"Comunidad {id} renombrada a {dto.Name}");
    return Results.Ok(new { id, name = found["name"] });
});

// ---------------------------------------------------------------------
// GET /communities/{id}  ->  consultar una comunidad por id
// ---------------------------------------------------------------------
app.MapGet("/communities/{id}", (int id) =>
{
    var found = communities.FirstOrDefault(x => (int)x["id"] == id);
    if (found is null)
        return Results.NotFound($"No existe la comunidad {id}.");

    var json = JsonSerializer.Serialize(new { id = found["id"], name = found["name"] });
    return Results.Text(json, "application/json");
});

app.Run();

// DTOs de entrada declarados al final del archivo (junto a todo lo demas)
record CreateCommunityDto(string Name);
record RenameCommunityDto(string Name);
