# RESPUESTAS - Practica S01 Minimal API

**Nombre: Junior Andres Hernandez Villanueva**
**Fecha:11/08/26**

> Copia este archivo como `RESPUESTAS.md` y llena cada seccion.

---

## 1. Smells que encontre en el codigo inicial

Lista los problemas del `Program.cs` original y el principio que viola cada uno.

| #   | Smell (que estaba mal)                                       | Principio violado            | Como lo arregle                                                 |
| --- | ------------------------------------------------------------ | ---------------------------- | --------------------------------------------------------------- |
| 1   | Un endpoint que valida, guarda, loguea y serializa el mismo  | SRP                          |
| 2   | La validacion del nombre copiada en dos endpoints            | DRY                          | Crear un value object para la validacion del Name               |
| 3   | Los numeros 3 y 21 escritos a mano en varios lugares         | DRY (numeros magicos)        | Hacer la validacion en un solo lugar, en el value object        |
| 4   | El guardado usa una lista suelta, no hay entidad ni interfaz | DIP                          | Crear entidad, handler e interfaz para orquestar                |
| 5   | El connection string escrito directo en el codigo            | Twelve-Factor III (Config)   | Crear entidad que almacene en un solo lugar los datos sensibles |
| 6   | El log usa string interpolation y filtra el password         | Regla de Serilog (AGENTS.md) | Hacer uso correcto del patron para Serilog limpio               |

---

## 2. Donde quedo cada principio de SOLID en mi solucion

Senala el archivo/clase donde se ve cada principio.

- **S (Single Responsibility):** GetCommunityHandler.cs tiene una unica responsabilidad consultar una comunidad por su ID
- **O (Open/Closed):** CommunityName.cs se mantiene cerrado a modificaciones de las condiciones principales, pero abierto a extensiones del mismo CommunityName como en sus casos de uso
- **L (Liskov Substitution):** GetCommunityHandler.cs al poder usar substitciones de los nombres sin depender de implementaciones o ligaduras de bajo nivel, siendo mas idiomatico
- **I (Interface Segregation):** CreateCommunityHandler.cs en el uso de interfaces que solo tienen lo necesario
- **D (Dependency Inversion):** CreateCommunityHandler.cs en el hecho de hacer llamadas a otras clases y objetos que vienen con dependencias

---

## 3. Pregunta trampa - DI Lifetimes

> Si registrara `InMemoryCommunityRepository` como **Singleton** y adentro le
> pidiera un servicio **Scoped**, que pasaria al arrancar la app? Por que?

Tu respuesta: Marcaria un error al compilar

---

## 4. Chequeo YAGNI

Que abstraccion consideraste agregar y decidiste **NO** agregar (o quitaste)?
Por que no la necesitas hoy?

Tu respuesta: RenameCommunityHandler y su metodo, y las abstracciones de sus dependencias, y de momento no veo necesaria ninguna abstraccion

---

## 5. (Opcional) Reto extra

Agregaste `FileCommunityRepository`? Cuantas lineas tuviste que cambiar en
`Program.cs`? Tuviste que tocar `Application` o `Domain`? (Deberia ser una sola
linea en `Program.cs` y nada mas.)

Tu respuesta:
