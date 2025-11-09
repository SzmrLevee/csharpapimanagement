# TodoApiController - Controller-based RESTful API

## 📋 Projekt Leírás

Ez a projekt egy **hagyományos Controller-based ASP.NET Core Web API**, amely egy Todo (teendő) lista kezelést valósít meg.
A projekt bemutatja a **klasszikus MVC (Model-View-Controller) pattern** használatát API fejlesztésben.

**Típus:** ASP.NET Core Web API (.NET 8.0)
**Port:** `http://localhost:5000` (alapértelmezett)

---

## 🎯 Mit tanulhatsz meg ebből a projektből?

1. **Controller osztályok** - `[ApiController]` és `[Route]` attribútumok
2. **RESTful API tervezés** - GET, POST, PUT, DELETE HTTP metódusok
3. **Dependency Injection** - Constructor injection használata
4. **FluentValidation** - Modell validáció fluent API-val
5. **In-memory data store** - Egyszerű adattárolás Dictionary-vel
6. **Interface-based design** - `IDataStore`, `IItemStore<T>` interfészek
7. **Action Result-ok** - `Ok()`, `NotFound()`, `BadRequest()`
8. **Route paraméterek** - `{id}` használata URL-ben

---

## 🏗️ Projekt Struktúra

```
TodoApiController/
├── Program.cs                      # Alkalmazás belépési pont
├── Controllers/
│   └── TodoController.cs           # Todo CRUD műveletek
├── Model/
│   ├── TodoItem.cs                 # Todo modell osztály
│   ├── User.cs                     # User modell osztály
│   ├── IDataStore.cs               # Data store interface
│   └── DataStore.cs                # In-memory implementáció
└── Validators/
    └── TodoItemValidator.cs        # FluentValidation szabályok
```

---

## 🌐 API Endpoint-ok

### Base URL
```
http://localhost:5000/api/todo
```

### 1️⃣ GET /api/todo - Összes Todo Lekérése

**Kérés:**
```bash
curl http://localhost:5000/api/todo
```

**Válasz (200 OK):**
```json
[
  {
    "id": 1,
    "title": "Bevásárlás a boltban",
    "description": "Tej, kenyér, tojás",
    "dueDate": "2025-11-10T00:00:00"
  },
  {
    "id": 2,
    "title": "Projekt befejezése",
    "description": "C# API projekt dokumentálása",
    "dueDate": "2025-11-15T00:00:00"
  }
]
```

---

### 2️⃣ GET /api/todo/{id} - Egy Todo Lekérése ID Alapján

**Kérés:**
```bash
curl http://localhost:5000/api/todo/1
```

**Válasz (200 OK):**
```json
{
  "id": 1,
  "title": "Bevásárlás a boltban",
  "description": "Tej, kenyér, tojás",
  "dueDate": "2025-11-10T00:00:00"
}
```

**Hiba (404 Not Found):**
```bash
curl http://localhost:5000/api/todo/999
# Nincs válasz body, csak 404 status code
```

---

### 3️⃣ POST /api/todo - Új Todo Létrehozása

**Kérés:**
```bash
curl -X POST http://localhost:5000/api/todo \
  -H "Content-Type: application/json" \
  -d '{
    "title": "Új feladat példa",
    "description": "Ez egy leírás",
    "dueDate": "2025-12-01T00:00:00"
  }'
```

**Válasz (200 OK):**
```json
{
  "id": 3,
  "title": "Új feladat példa",
  "description": "Ez egy leírás",
  "dueDate": "2025-12-01T00:00:00"
}
```

**Validációs Hiba (400 Bad Request):**
```bash
curl -X POST http://localhost:5000/api/todo \
  -H "Content-Type: application/json" \
  -d '{
    "title": "Rövid",
    "description": "",
    "dueDate": "2025-12-01T00:00:00"
  }'
```

**Hiba válasz:**
```json
{
  "errors": [
    {
      "propertyName": "Title",
      "errorMessage": "'Title' must be between 10 and 200 characters.",
      "attemptedValue": "Rövid"
    },
    {
      "propertyName": "Description",
      "errorMessage": "'Description' must not be empty."
    }
  ]
}
```

---

### 4️⃣ PUT /api/todo/{id} - Todo Módosítása

**Kérés:**
```bash
curl -X PUT http://localhost:5000/api/todo/1 \
  -H "Content-Type: application/json" \
  -d '{
    "title": "Bevásárlás frissítve",
    "description": "Tej, kenyér, tojás, vaj",
    "dueDate": "2025-11-11T00:00:00"
  }'
```

**Válasz (200 OK):**
```json
{
  "id": 1,
  "title": "Bevásárlás frissítve",
  "description": "Tej, kenyér, tojás, vaj",
  "dueDate": "2025-11-11T00:00:00"
}
```

**Hiba (404 Not Found):**
- Ha az ID nem létezik

---

### 5️⃣ DELETE /api/todo/{id} - Todo Törlése

**Kérés:**
```bash
curl -X DELETE http://localhost:5000/api/todo/1
```

**Válasz (200 OK):**
- Üres body, csak 200 status code

**Hiba (404 Not Found):**
- Ha az ID nem létezik

---

## 🔑 Fő Komponensek Részletesen

### 1. Program.cs - Alkalmazás Konfigurálása

```csharp
var builder = WebApplication.CreateBuilder(args);

// Controller-ök regisztrálása
builder.Services.AddControllers();

// ⭐ IDataStore implementáció Singleton-ként
// Singleton = egy példány az egész alkalmazás életciklusa alatt
builder.Services.AddSingleton<IDataStore, DataStore>();

// ⭐ FluentValidation regisztrálása
// Automatikusan megtalálja az összes Validator osztályt
builder.Services.AddValidatorsFromAssemblyContaining<TodoItemValidator>();

// Swagger dokumentáció
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Development környezetben Swagger UI
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Autorizáció middleware
app.UseAuthorization();

// Controller-ök route-olása
app.MapControllers();

app.Run();
```

**Dependency Injection Lifecycle-ok:**
- **Singleton** - Egy példány mindenkinek (DataStore)
- **Scoped** - Egy példány HTTP request-enként
- **Transient** - Új példány minden injektálásnál

---

### 2. TodoController.cs - Controller Osztály

#### Controller Dekoráció

```csharp
[Route("api/[controller]")]  // URL: /api/todo ([controller] = "Todo")
[ApiController]               // API specifikus viselkedés (auto model validation)
public class TodoController : ControllerBase
```

**Fontos:**
- `ControllerBase` - API controller-ekhez (nincs View támogatás)
- `Controller` - MVC controller-ekhez (van View támogatás)
- `[controller]` - Automatikusan lecseréli a "Controller" prefix-et

---

#### Constructor Injection

```csharp
readonly IDataStore dataStore;
readonly IValidator<TodoItem> validator;

public TodoController(IDataStore dataStore, IValidator<TodoItem> validator)
{
    // Null check - ArgumentNullException ha nincs implementáció
    this.dataStore = dataStore ?? throw new ArgumentNullException(nameof(dataStore));
    this.validator = validator ?? throw new ArgumentNullException(nameof(validator));
}
```

**Dependency Injection előnyei:**
- **Testability** - Mock objektumokkal tesztelhető
- **Loose coupling** - Interface-re hivatkozás, nem konkrét osztályra
- **Lifecycle management** - DI container kezeli az objektumok élettartamát

---

#### GET Action - Összes Todo

```csharp
[HttpGet]  // HTTP GET metódus
public IEnumerable<TodoItem> Get()
{
    // IDataStore -> IItemStore<TodoItem> cast
    // GetAll() visszaadja az összes TodoItem-et
    return ((IItemStore<TodoItem>)dataStore).GetAll();
}
```

**Action signature:**
- Nincs `[Route]` - Base route-ot használja: `/api/todo`
- Return type: `IEnumerable<TodoItem>` - Automatikus JSON serialization

---

#### GET Action - Egy Todo ID Alapján

```csharp
[HttpGet("{id}")]  // Route: /api/todo/{id} (pl. /api/todo/5)
public IActionResult Get(int id)
{
    // LINQ FirstOrDefault - első elem ami megfelel, vagy null
    var item = Get().FirstOrDefault(x => x.Id == id);
    
    // Ternary operator: feltétel ? igaz_ág : hamis_ág
    return item == null ? NotFound() : Ok(item);
}
```

**IActionResult:**
- `Ok(object)` - 200 + JSON body
- `NotFound()` - 404 + üres body
- `BadRequest(object)` - 400 + JSON body
- `Created(uri, object)` - 201 + Location header

---

#### POST Action - Új Todo Létrehozása

```csharp
[HttpPost]  // HTTP POST metódus
public IActionResult Post([FromBody] TodoItem value)
{
    // 1. Validálás FluentValidation-nel
    var result = validator.Validate(value);
    if (!result.IsValid)
    {
        // Validációs hibák visszaadása
        return BadRequest(result);
    }
    
    // 2. Új ID generálása (max ID + 1)
    var newId = 1;
    try
    {
        newId = Get().Max(x => x.Id) + 1;
    }
    catch { }  // Első elem esetén Max() exception-t dob
    
    // 3. ID beállítása és mentés
    value.Id = newId;
    dataStore.Add(value);
    
    // 4. Visszaadás a generált ID-val
    return Ok(value);
}
```

**[FromBody]:**
- A request body-ból deserializálja a JSON-t
- Automatikus model binding
- Content-Type: application/json szükséges

---

#### PUT Action - Todo Módosítása

```csharp
[HttpPut("{id}")]  // Route: /api/todo/{id}
public IActionResult Put(int id, [FromBody] TodoItem value)
{
    // 1. Validálás
    var result = validator.Validate(value);
    if (!result.IsValid)
    {
        return BadRequest(result);
    }
    
    // 2. ID beállítása (URL-ből jön az ID)
    value.Id = id;
    
    // 3. Frissítés
    if(dataStore.Update(value))
    {
        return NotFound();  // Ha nem létezik
    }
    
    return Ok(value);
}
```

**REST konvenció:**
- PUT - Teljes erőforrás cseréje
- PATCH - Részleges módosítás (ez a projekt nem használja)

---

#### DELETE Action - Todo Törlése

```csharp
[HttpDelete("{id}")]  // Route: /api/todo/{id}
public IActionResult Delete(int id)
{
    // 1. Keresés ID alapján
    var item = Get().FirstOrDefault(x => x.Id == id);
    
    // 2. Létezés ellenőrzése
    if (item == null) 
    { 
        return NotFound(); 
    }
    
    // 3. Törlés
    dataStore.Delete(item);
    
    return Ok();
}
```

---

### 3. TodoItem.cs - Modell Osztály

```csharp
public class TodoItem
{
    public int Id { get; set; }                    // Egyedi azonosító
    public string Title { get; set; } = string.Empty;  // Cím (10-200 karakter)
    public string Description { get; set; } = string.Empty;  // Leírás (kötelező)
    public DateTime DueDate { get; set; }          // Határidő
}
```

**Property Initializer:**
- `= string.Empty` - Alapértelmezett érték (nem null)
- Elkerüljük a `NullReferenceException`-t

---

### 4. DataStore.cs - In-Memory Adattárolás

```csharp
public class DataStore : IDataStore
{
    // Dictionary<Key, Value> - Gyors keresés O(1) időben
    readonly Dictionary<int, TodoItem> todoItems = [];
    readonly Dictionary<string, User> users = [];
    
    public bool Add(TodoItem item)
    {
        // ContainsKey - ellenőrzi, hogy létezik-e már az ID
        if (todoItems.ContainsKey(item.Id))
        {
            return false;  // Már létezik
        }
        
        // Hozzáadás a Dictionary-hez
        todoItems.Add(item.Id, item);
        return true;
    }
    
    public IEnumerable<TodoItem> GetAll()
    {
        // Dictionary.Values - összes érték lekérése
        return todoItems.Values;
    }
    
    public bool Update(TodoItem item)
    {
        if (!todoItems.ContainsKey(item.Id))
        {
            return false;  // Nem létezik
        }
        
        // Indexer [] - érték felülírása
        todoItems[item.Id] = item;
        return true;
    }
    
    public bool Delete(TodoItem item)
    {
        if (!todoItems.ContainsKey(item.Id))
        {
            return false;
        }
        
        // Remove - törlés a Dictionary-ből
        todoItems.Remove(item.Id);
        return true;
    }
}
```

**⚠️ FONTOS:**
- In-memory = újraindításkor ELVESZNEK az adatok!
- Production-ben: SQL, MongoDB, Cosmos DB, stb.
- Dictionary thread-safe? **NEM!** ConcurrentDictionary kell multi-threading-hez

---

### 5. IDataStore.cs - Interface

```csharp
public interface IItemStore<T>
{
    bool Add(T item);
    bool Update(T item);
    bool Delete(T item);
    IEnumerable<T> GetAll();
}

public interface IDataStore : IItemStore<TodoItem>, IItemStore<User>
{
}
```

**Generic Interface:**
- `IItemStore<T>` - Típus-független CRUD műveletek
- Újrafelhasználható TodoItem-re, User-re, stb.

---

### 6. TodoItemValidator.cs - FluentValidation

```csharp
public class TodoItemValidator : AbstractValidator<TodoItem>
{
    public TodoItemValidator()
    {
        // Title hossza: 10-200 karakter között
        RuleFor(x => x.Title).Length(10, 200);
        
        // Description nem lehet üres
        RuleFor(x => x.Description).NotEmpty();
    }
}
```

**FluentValidation szabályok:**
- `NotEmpty()` - Nem lehet null, üres string, vagy whitespace
- `Length(min, max)` - Hossz validálás
- `GreaterThan(value)` - Nagyobb mint
- `EmailAddress()` - Email formátum
- `Custom(lambda)` - Egyedi validáció

**Validációs hiba üzenet testreszabása:**
```csharp
RuleFor(x => x.Title)
    .Length(10, 200)
    .WithMessage("A címnek 10 és 200 karakter között kell lennie!");
```

---

## 🚀 Hogyan Használd?

### 1. Alkalmazás Indítása

```bash
cd TodoApiController/TodoApiController
dotnet run
```

**Kimenet:**
```
Now listening on: http://localhost:5000
```

---

### 2. Swagger UI Megnyitása

```
http://localhost:5000/swagger
```

Itt látod az összes endpoint-ot és **tesztelheted** őket interaktívan!

---

### 3. Postman Collection Létrehozása

**1. Új Todo hozzáadása:**
- Method: `POST`
- URL: `http://localhost:5000/api/todo`
- Headers: `Content-Type: application/json`
- Body:
```json
{
  "title": "Tanulás C# API fejlesztés",
  "description": "Controller-based API pattern elsajátítása",
  "dueDate": "2025-11-20T00:00:00"
}
```

**2. Összes Todo lekérése:**
- Method: `GET`
- URL: `http://localhost:5000/api/todo`

**3. Egy Todo módosítása:**
- Method: `PUT`
- URL: `http://localhost:5000/api/todo/1`
- Body: (módosított adatok)

**4. Todo törlése:**
- Method: `DELETE`
- URL: `http://localhost:5000/api/todo/1`

---

## 🎓 Tanulási Lépések

### 1. Értsd meg a Controller pattern-t
- Mi a különbség Minimal API és Controller API között?
- Mikor használj Controller-t?

### 2. Gyakorold a CRUD műveleteket
- Hozz létre új endpoint-okat
- Add hozzá a User kezelést (UserController)

### 3. Bővítsd a validációt
- Adj hozzá több szabályt (DueDate jövőben legyen, stb.)
- Egyedi validátor metódusok

### 4. Próbáld perzisztens adattárolással
- Entity Framework Core
- Dapper + SQL Server

---

## 💡 Következő Lépések

1. ✅ Add hozzá az **Authentication** projektet - JWT token kezelés
2. ✅ Implementálj **UserController**-t
3. ✅ Használj **Entity Framework Core**-t
4. ✅ Add hozzá **logging**-ot (Serilog)
5. ✅ Nézd meg a **MinimalAPIDemo** projektet - összehasonlítás

---

## ⚠️ Gyakori Hibák

### 1. Singleton DataStore Thread-Safety

**Probléma:** Dictionary nem thread-safe!

**Megoldás:**
```csharp
readonly ConcurrentDictionary<int, TodoItem> todoItems = new();
```

### 2. Validáció nem fut le automatikusan

**Ok:** FluentValidation nem fut automatikusan!

**Megoldás:** Manuális `validator.Validate()` hívás (ahogy a példában is van)

### 3. PUT update nem működik

**Probléma:** DataStore.Update() return értéke fordított!

```csharp
// ❌ ROSSZ
if(dataStore.Update(value))
{
    return NotFound();  // TRUE esetén NotFound?!
}

// ✅ HELYES
if(!dataStore.Update(value))
{
    return NotFound();  // FALSE esetén NotFound
}
```

---

**Készítve tanulási célból** 🚀
**Következő:** Authentication projekt - Haladó JWT kezelés
