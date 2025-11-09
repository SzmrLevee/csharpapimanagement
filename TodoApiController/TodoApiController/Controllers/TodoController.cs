// TodoController - RESTful CRUD műveletek Todo elem kezeléshez
// Példa: Controller-based API pattern ASP.NET Core-ban

using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using TodoApiController.Model;

namespace TodoApiController.Controllers;

/// <summary>
/// TodoController - Todo lista kezelés API endpoint-ok
/// Base Route: /api/todo
/// </summary>
[Route("api/[controller]")]  // [controller] = "Todo" (Controller prefix nélkül)
[ApiController]               // API specifikus viselkedés: auto model validation, routing, stb.
public class TodoController : ControllerBase  // ControllerBase = API controller (nincs View támogatás)
{
    // Readonly fieldek - csak constructor-ban inicializálhatók
    readonly IDataStore dataStore;          // Adattár interface
    readonly IValidator<TodoItem> validator; // FluentValidation validator

#pragma warning disable IDE0290  // Primary constructor helyett explicit constructor
    /// <summary>
    /// Constructor - Dependency Injection
    /// Az ASP.NET Core DI container automatikusan injektálja a paramétereket
    /// </summary>
    /// <param name="dataStore">IDataStore implementáció (Singleton)</param>
    /// <param name="validator">TodoItem validator (FluentValidation)</param>
    public TodoController(IDataStore dataStore, IValidator<TodoItem> validator)
    {
        // Null check - ha nincs regisztrálva, ArgumentNullException-t dob
        this.dataStore = dataStore ?? throw new ArgumentNullException(nameof(dataStore));
        this.validator = validator ?? throw new ArgumentNullException(nameof(validator));
    }
#pragma warning restore IDE0290

    // === HTTP GET MŰVELETEK ===

    /// <summary>
    /// GET: api/todo
    /// Összes Todo lekérése
    /// </summary>
    /// <returns>TodoItem lista (JSON array)</returns>
    /// <response code="200">Sikeres lekérés - lista (lehet üres)</response>
    [HttpGet]  // HTTP GET metódus
    public IEnumerable<TodoItem> Get()
    {
        // IDataStore -> IItemStore<TodoItem> explicit cast
        // GetAll() visszaadja az összes TodoItem-et Dictionary.Values-ból
        return ((IItemStore<TodoItem>)dataStore).GetAll();
    }

    /// <summary>
    /// GET: api/todo/{id}
    /// Egy konkrét Todo lekérése ID alapján
    /// </summary>
    /// <param name="id">Todo egyedi azonosítója</param>
    /// <returns>TodoItem vagy 404 ha nem található</returns>
    /// <response code="200">Sikeres lekérés - egy TodoItem objektum</response>
    /// <response code="404">Nem található a megadott ID-val</response>
    [HttpGet("{id}")]  // Route paraméter: /api/todo/5
    public IActionResult Get(int id)
    {
        // LINQ FirstOrDefault - első elem ami megfelel a feltételnek, vagy null
        var item = Get().FirstOrDefault(x => x.Id == id);
        
        // Ternary operator: feltétel ? igaz_ág : hamis_ág
        // null = NotFound (404), egyébként Ok (200) + item JSON
        return item == null ? NotFound() : Ok(item);
    }

    // === HTTP POST MŰVELET ===

    /// <summary>
    /// POST: api/todo
    /// Új Todo létrehozása
    /// </summary>
    /// <param name="value">Új TodoItem objektum (JSON body-ból)</param>
    /// <returns>Létrehozott TodoItem generált ID-val vagy validációs hiba</returns>
    /// <response code="200">Sikeres létrehozás - TodoItem az új ID-val</response>
    /// <response code="400">Validációs hiba - FluentValidation hibák listája</response>
    [HttpPost]  // HTTP POST metódus
    public IActionResult Post([FromBody] TodoItem value)
    {
        // 1. VALIDÁLÁS FluentValidation-nel
        var result = validator.Validate(value);
        
        // Ha nem valid, visszaadjuk a validációs hibákat (400 Bad Request)
        if (!result.IsValid)
        {
            return BadRequest(result);  // result automatikusan JSON-ná alakul
        }
        
        // 2. ÚJ ID GENERÁLÁSA
        // Alapértelmezett: 1 (első elem esetén)
        var newId = 1;
        try
        {
            // LINQ Max() - legnagyobb ID kiválasztása, majd +1
            // ⚠️ Ha üres a lista, Max() InvalidOperationException-t dob!
            newId = Get().Max(x => x.Id) + 1;
        }
        catch 
        { 
            // Catch ág üres - newId marad 1
        }
        
        // 3. ID BEÁLLÍTÁSA ÉS MENTÉS
        value.Id = newId;
        dataStore.Add(value);
        
        // 4. VÁLASZ
        // Ok(200) + létrehozott TodoItem a generált ID-val
        return Ok(value);
    }

    // === HTTP PUT MŰVELET ===

    /// <summary>
    /// PUT: api/todo/{id}
    /// Meglévő Todo módosítása
    /// </summary>
    /// <param name="id">Módosítandó Todo ID-ja (URL-ből)</param>
    /// <param name="value">Frissített TodoItem adatok (JSON body-ból)</param>
    /// <returns>Frissített TodoItem vagy hiba</returns>
    /// <response code="200">Sikeres módosítás</response>
    /// <response code="400">Validációs hiba</response>
    /// <response code="404">Nem található a megadott ID</response>
    [HttpPut("{id}")]  // Route: /api/todo/{id}
    public IActionResult Put(int id, [FromBody] TodoItem value)
    {
        // 1. VALIDÁLÁS
        var result = validator.Validate(value);
        if (!result.IsValid)
        {
            return BadRequest(result);
        }
        
        // 2. ID BEÁLLÍTÁSA
        // Az URL-ből jövő ID felülírja a body-ban lévőt
        // REST konvenció: PUT /api/todo/5 -> ID = 5
        value.Id = id;
        
        // 3. FRISSÍTÉS
        // ⚠️ BUG! A DataStore.Update() TRUE-t ad vissza ha SIKERES!
        // De itt NotFound()-ot adunk TRUE esetén -> ROSSZ!
        if(dataStore.Update(value))
        {
            return NotFound();  // Ez soha nem fut le helyesen!
        }
        
        // Ha eljutunk ide, sikeres volt
        return Ok(value);
    }

    // === HTTP DELETE MŰVELET ===

    /// <summary>
    /// DELETE: api/todo/{id}
    /// Todo törlése ID alapján
    /// </summary>
    /// <param name="id">Törlendő Todo ID-ja</param>
    /// <returns>Üres válasz vagy 404</returns>
    /// <response code="200">Sikeres törlés</response>
    /// <response code="404">Nem található a megadott ID</response>
    [HttpDelete("{id}")]  // Route: /api/todo/{id}
    public IActionResult Delete(int id)
    {
        // 1. KERESÉS ID ALAPJÁN
        var item = Get().FirstOrDefault(x => x.Id == id);
        
        // 2. LÉTEZÉS ELLENŐRZÉSE
        if (item == null) 
        { 
            return NotFound();  // 404 ha nem létezik
        }
        
        // 3. TÖRLÉS
        dataStore.Delete(item);
        
        // 4. VÁLASZ
        return Ok();  // 200 + üres body
    }
}
