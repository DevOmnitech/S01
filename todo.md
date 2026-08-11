# Practica S01 - Minimal API "Ordena el desorden"

**Capacitacion OMNI TECH - Sabado 1 (Fundamentos & Setup)**
**Modalidad:** individual - **Tiempo estimado:** 90 a 120 minutos
**Entregable:** el proyecto ordenado + `RESPUESTAS.md`

---

## 1. De que se trata (lee esto primero)

Te entregamos una Minimal API que **ya funciona**, pero esta escrita de la peor
forma posible: todo esta amontonado en un solo archivo (`Program.cs`). Cada
endpoint valida, guarda, escribe el log y arma la respuesta, todo revuelto en el
mismo lugar.

**Tu trabajo NO es reescribir la app desde cero.**
Tu trabajo es **ordenar** ese desorden en las capas que vimos en la S01, sin
romper lo que ya funciona.

Piensa en una cocina donde cuchillos, especias, platos y la factura de la luz
estan todos en el mismo cajon. Funciona, pero es un caos. Tu tarea es poner cada
cosa en su estante. Los estantes ya tienen nombre:

```
Domain          -> las reglas del negocio (que es una comunidad, que nombre es valido)
Application     -> la logica que coordina un caso de uso (crear, consultar)
Infrastructure  -> el guardado de datos (aqui, en memoria)
Host            -> el Program.cs, que solo conecta las piezas y expone los endpoints
```

Al terminar, la app hace exactamente lo mismo que antes, pero cada pieza vive en
su lugar y se entiende por que.

---

## 2. El contexto: una rebanada del Reddit-clone

Vas a trabajar con una sola entidad: **`Community`** (una comunidad, como un
subreddit).

**Reglas de negocio del nombre de la comunidad:**

- Es obligatorio (no vacio).
- Debe tener entre **3 y 21 caracteres**.
- Solo letras, numeros y guion bajo (`_`).
- Se guarda siempre en minusculas.

Estas 4 reglas son **una sola pieza de conocimiento** y deben vivir en **un solo
lugar** (pista fuerte de DRY).

---

## 3. Que hay en esta carpeta

```
practica-S01-minimal-api/
+-- todo.md                     <- este archivo (las instrucciones)
+-- RESPUESTAS.plantilla.md     <- copiala como RESPUESTAS.md y llenala
+-- codigo-inicial/             <- el codigo DESORDENADO que vas a arreglar
    +-- RedditClone.Api.csproj
    +-- Program.cs              <- aqui esta todo amontonado
    +-- appsettings.json
```

---

## 4. Paso 0 - Corre el codigo feo y compruebalo (10 min)

Antes de tocar nada, hazlo correr para ver que funciona.

```bash
cd codigo-inicial
dotnet run
```

En la consola vas a ver el puerto (algo como `http://localhost:5xxx`).
Prueba los 3 endpoints (cambia el puerto por el tuyo):

```bash
# Crear una comunidad
curl -X POST http://localhost:5xxx/communities \
  -H "Content-Type: application/json" \
  -d '{ "name": "dotnet_mx" }'

# Consultarla (usa el id que te devolvio)
curl http://localhost:5xxx/communities/1

# Renombrarla
curl -X PUT http://localhost:5xxx/communities/1/rename \
  -H "Content-Type: application/json" \
  -d '{ "name": "dotnet_latam" }'

# Intenta un nombre invalido (debe responder 400)
curl -X POST http://localhost:5xxx/communities \
  -H "Content-Type: application/json" \
  -d '{ "name": "ab" }'
```

Deja esto funcionando en tu cabeza: **al final de la practica estos mismos
comandos deben responder igual.** Eso es como sabras que no rompiste nada.

---

## 5. Paso 1 - Encuentra los olores (code smells) (15 min)

Abre `codigo-inicial/Program.cs` y **antes de mover nada**, identifica que esta
mal. Anota cada hallazgo en tu `RESPUESTAS.md`. Hay al menos 6 problemas
plantados a proposito. Guia:

| # | Que buscar | Principio que viola |
|---|------------|---------------------|
| 1 | Un endpoint que valida, guarda, loguea y serializa el mismo | SRP |
| 2 | La validacion del nombre copiada en dos endpoints | DRY |
| 3 | Los numeros 3 y 21 escritos a mano en varios lugares | DRY (numeros magicos) |
| 4 | El guardado usa una lista suelta, no hay entidad ni interfaz | DIP |
| 5 | El connection string escrito directo en el codigo | Twelve-Factor III (Config) |
| 6 | El log usa string interpolation y filtra el password | Regla de Serilog (AGENTS.md) |

Si encuentras mas, mejor. Escribelos todos.

---

## 6. Paso 2 - Ordena en capas (60 min)

Crea una **copia** de `codigo-inicial/` (por ejemplo una carpeta `src/`) y ahi
haz el refactor. Ve un paso a la vez y corre `dotnet build` seguido. La
estructura objetivo es esta (un solo proyecto, carpetas por capa):

```
src/
+-- Domain/
|   +-- Community.cs                <- la entidad
|   +-- CommunityName.cs            <- Value Object con la regla del nombre
|   +-- ICommunityRepository.cs     <- la interfaz (contrato)
|   +-- Result.cs                   <- Result pattern (exito o error, sin excepciones)
+-- Application/
|   +-- CreateCommunityHandler.cs   <- caso de uso: crear
|   +-- GetCommunityHandler.cs      <- caso de uso: consultar
+-- Infrastructure/
|   +-- InMemoryCommunityRepository.cs   <- implementa ICommunityRepository en memoria
+-- Endpoints/
|   +-- CommunityEndpoints.cs       <- los MapPost/MapGet agrupados
+-- Program.cs                      <- solo conecta piezas (DI) y llama a los endpoints
+-- appsettings.json
```

> Nota: en el back-template real cada capa es un **proyecto** separado. Aqui
> usamos carpetas dentro de un solo proyecto para enfocarnos en la separacion
> logica y no perder tiempo con referencias. (Convertirlo en proyectos
> separados es el reto extra del final.)

### 6.1 Domain - la regla del nombre en UN solo lugar (DRY)

Crea `CommunityName` como un Value Object. La idea: que sea **imposible** crear
un nombre invalido. Constructor privado + un metodo `Create()` que es el unico
camino.

```csharp
// Domain/CommunityName.cs
public sealed record CommunityName
{
    public const int MinLength = 3;    // los numeros magicos, ahora con nombre y en UN lugar
    public const int MaxLength = 21;

    public string Value { get; }

    private CommunityName(string value) => Value = value;   // nadie puede hacer new CommunityName(...)

    public static Result<CommunityName> Create(string? raw)
    {
        // TODO: aplica las 4 reglas del punto 2.
        //   - vacio          -> Result.Fail<CommunityName>("El nombre es requerido.")
        //   - longitud        -> usa MinLength / MaxLength
        //   - caracteres      -> letras, numeros y '_'
        //   - si todo OK      -> Result.Ok(new CommunityName(raw.ToLowerInvariant()))
    }
}
```

Cuando la regla cambie (por ejemplo, permitir guion medio), **solo tocas este
archivo**. Los dos endpoints reciben el cambio gratis. Eso es DRY.

### 6.2 Domain - Result pattern (sin excepciones para el flujo)

Crea un `Result<T>` sencillo que represente "salio bien con un valor" o "salio
mal con un mensaje". Skeleton sugerido:

```csharp
// Domain/Result.cs
public sealed class Result<T>
{
    public bool IsSuccess { get; }
    public T? Value { get; }
    public string? Error { get; }

    private Result(bool ok, T? value, string? error)
        => (IsSuccess, Value, Error) = (ok, value, error);

    public static Result<T> Ok(T value) => new(true, value, null);
    public static Result<T> Fail(string error) => new(false, default, error);
}

// Helper opcional para escribir Result.Fail<T>(...) mas comodo
public static class Result
{
    public static Result<T> Ok<T>(T value) => Result<T>.Ok(value);
    public static Result<T> Fail<T>(string error) => Result<T>.Fail(error);
}
```

### 6.3 Domain - la entidad y la interfaz

```csharp
// Domain/Community.cs
public sealed class Community
{
    public int Id { get; set; }
    public CommunityName Name { get; set; } = default!;
    public DateTime CreatedAt { get; set; }
}
```

```csharp
// Domain/ICommunityRepository.cs
// Solo los metodos que los casos de uso realmente usan (ISP).
public interface ICommunityRepository
{
    Community Add(Community community);
    Community? GetById(int id);
    // agrega lo que necesites para el rename, pero nada de mas (YAGNI)
}
```

Fijate: la interfaz vive en **Domain**, pero NO dice nada de "lista", "memoria"
ni "SQL". Ese es el corazon del DIP.

### 6.4 Infrastructure - la implementacion concreta (DIP)

```csharp
// Infrastructure/InMemoryCommunityRepository.cs
public sealed class InMemoryCommunityRepository : ICommunityRepository
{
    private readonly List<Community> _items = new();
    private int _nextId = 1;

    public Community Add(Community community) { /* TODO: asigna id y guarda */ }
    public Community? GetById(int id) { /* TODO */ }
}
```

La lista suelta del codigo feo ahora vive **aqui adentro y en ningun otro
lado**. Application no sabe que es una lista; solo conoce la interfaz.

### 6.5 Application - los casos de uso (SRP)

Cada handler hace **una sola cosa**: orquestar un caso de uso. Nada de
serializar ni de escribir HTTP.

```csharp
// Application/CreateCommunityHandler.cs
public sealed class CreateCommunityHandler
{
    private readonly ICommunityRepository _repository;
    private readonly ILogger<CreateCommunityHandler> _logger;

    public CreateCommunityHandler(ICommunityRepository repository,
                                  ILogger<CreateCommunityHandler> logger)
        => (_repository, _logger) = (repository, logger);

    public Result<Community> Handle(string? rawName)
    {
        var nameResult = CommunityName.Create(rawName);
        if (!nameResult.IsSuccess)
            return Result.Fail<Community>(nameResult.Error!);

        var saved = _repository.Add(new Community
        {
            Name = nameResult.Value!,
            CreatedAt = DateTime.UtcNow
        });

        // log CORRECTO: structured logging, sin interpolation, sin filtrar secretos
        _logger.LogInformation("Comunidad creada {CommunityId} {CommunityName}",
            saved.Id, saved.Name.Value);

        return Result.Ok(saved);
    }
}
```

Haz lo mismo para `GetCommunityHandler`.

### 6.6 Host - endpoints limpios y DI (KISS + Factor III)

```csharp
// Endpoints/CommunityEndpoints.cs
public static class CommunityEndpoints
{
    public static void MapCommunityEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/communities");

        group.MapPost("/", (CreateCommunityDto dto, CreateCommunityHandler handler) =>
        {
            var result = handler.Handle(dto.Name);
            return result.IsSuccess
                ? Results.Ok(new { id = result.Value!.Id, name = result.Value!.Name.Value })
                : Results.BadRequest(result.Error);
        });

        // TODO: GET /communities/{id} y PUT /communities/{id}/rename
    }
}

public record CreateCommunityDto(string? Name);
```

```csharp
// Program.cs  (queda cortito)
var builder = WebApplication.CreateBuilder(args);

// el connection string ahora se LEE de configuracion, no se hardcodea (Factor III)
var connectionString = builder.Configuration.GetConnectionString("Default")
    ?? throw new InvalidOperationException("Falta la connection string 'Default'.");

builder.Services.AddScoped<ICommunityRepository, InMemoryCommunityRepository>();
builder.Services.AddScoped<CreateCommunityHandler>();
builder.Services.AddScoped<GetCommunityHandler>();

var app = builder.Build();
app.MapCommunityEndpoints();
app.Run();
```

Cuando termines, `Program.cs` no debe tener **ni una linea** de validacion,
persistencia ni serializacion. Solo conecta piezas y expone los endpoints.

---

## 7. Paso 3 - Pregunta trampa de DI Lifetimes (5 min)

En `Program.cs` registraste el repositorio como `AddScoped`. Responde en tu
`RESPUESTAS.md`:

> Si registraras `InMemoryCommunityRepository` como **Singleton** y adentro le
> pidieras un servicio **Scoped**, que pasaria al arrancar la app? Explica en
> tus palabras por que.

(Repasa la seccion 5.3 del hands-on de S01.)

---

## 8. Paso 4 - Chequeo YAGNI (5 min)

Revisa TU codigo ordenado. Por cada interfaz o clase que agregaste, preguntate:
"que problema concreto de HOY resuelve?". Si agregaste algo "por si acaso"
(un `IUnitOfWork`, un generico `IRepository<T>`, una fabrica que no usas),
**quitalo** y anota en `RESPUESTAS.md` que quitaste y por que.

---

## 9. Endpoints que deben seguir funcionando

Al terminar, estos comandos deben responder igual que en el Paso 0:

- `POST /communities` con nombre valido -> 200 con `{ id, name }`
- `POST /communities` con nombre invalido -> 400 con el mensaje de error
- `GET /communities/{id}` existente -> 200 ; inexistente -> 404
- `PUT /communities/{id}/rename` -> 200 ; inexistente -> 404

---

## 10. Entregable

1. La carpeta `src/` con el proyecto **ordenado y compilando** (`dotnet build`
   sin errores ni warnings).
2. `RESPUESTAS.md` (copia de la plantilla) con:
   - la lista de smells que encontraste en el codigo inicial,
   - donde quedo cada principio de SOLID en tu solucion,
   - la respuesta a la pregunta trampa de DI Lifetimes,
   - que abstraccion quitaste por YAGNI (si aplica).
3. Un commit con mensaje en formato conventional commits, por ejemplo:
   `refactor(communities): ordenar minimal api en capas (solid + result pattern)`

---

## 11. Rubrica (10 puntos)

| Criterio | Pts |
|----------|-----|
| Compila sin warnings y los 4 endpoints responden como en el Paso 0 | 2 |
| `CommunityName` con constructor privado + `Create()` que valida las 4 reglas | 2 |
| DIP correcto: interfaz en Domain, implementacion en Infrastructure, handler no conoce la clase concreta | 2 |
| `Program.cs` limpio: sin validacion / persistencia / serializacion; endpoints en su propio archivo | 1 |
| Config leida con `IConfiguration`, sin hardcode (Factor III) | 1 |
| Result pattern usado (sin excepciones para el control de flujo) y log correcto (sin interpolation) | 1 |
| `RESPUESTAS.md` completo (smells + SOLID + DI trampa) | 1 |

---

## 12. Reto extra (opcional, +1)

Agrega un segundo repositorio `FileCommunityRepository` que guarde las
comunidades en un archivo JSON, y **cambia una sola linea en `Program.cs`** para
usarlo en vez del de memoria. Si tuviste que tocar algo de `Application` o
`Domain`, tu DIP no estaba bien: arreglalo hasta que el cambio sea de una linea.

---

## 13. Reglas que NUNCA violar (del AGENTS.md del proyecto)

- NO string interpolation en Serilog/ILogger. Correcto:
  `_logger.LogInformation("Comunidad {Id} creada", id)`
- NO `.Result` ni `async void`.
- NO `IRepository<T>` generico. Usa una interfaz por entidad (`ICommunityRepository`).
- NO hardcodear connection strings ni secretos: van en configuracion.
- NO registrar un servicio Scoped dentro de un Singleton.

---

## 14. Como saber que terminaste

- [ ] `dotnet build` sin errores ni warnings.
- [ ] Los 4 comandos curl del Paso 0 responden igual que al inicio.
- [ ] `Program.cs` cabe en una pantalla y no tiene logica de negocio.
- [ ] La regla del nombre existe en UN solo lugar (`CommunityName`).
- [ ] Puedo senalar SRP, OCP, LSP, ISP y DIP en mi codigo.
- [ ] `RESPUESTAS.md` entregado.
