# 📚 C# API Dolgozat Segédlet - Teljes Kódgyűjtemény

> **Utolsó frissítés:** 2025. november 9.  
> **Cél:** Minden kód és lépés egy helyen, hogy bármelyik API típust meg tudd valósítani.

---

## 📋 Tartalomjegyzék

1. [Projekt Létrehozás Alapok](#1-projekt-létrehozás-alapok)
2. [REST API Kliens (HttpClient)](#2-rest-api-kliens-httpclient)
3. [Controller-based API (CRUD)](#3-controller-based-api-crud)
4. [JWT Autentikáció (3 verzió)](#4-jwt-autentikáció)
5. [FluentValidation](#5-fluentvalidation)
6. [In-Memory DataStore](#6-in-memory-datastore)
7. [Password Hashing (PBKDF2)](#7-password-hashing-pbkdf2)
8. [Role-Based Authorization](#8-role-based-authorization)
9. [Minimal API](#9-minimal-api)
10. [FastEndpoints](#10-fastendpoints)
11. [Gyakori Hibák és Megoldások](#11-gyakori-hibák-és-megoldások)

---

## 1. Projekt Létrehozás Alapok

### 1.1 Console App (REST Kliens)

```bash
# Projekt létrehozása
dotnet new console -n RestApiHasznalat
cd RestApiHasznalat

# Futtatás
dotnet run
```

### 1.2 Web API Projekt

```bash
# ASP.NET Core Web API projekt
dotnet new webapi -n TodoApiController
cd TodoApiController

# NuGet csomagok telepítése
dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer
dotnet add package Microsoft.IdentityModel.Tokens
dotnet add package System.IdentityModel.Tokens.Jwt
dotnet add package FluentValidation.AspNetCore
dotnet add package Swashbuckle.AspNetCore

# Futtatás
dotnet run
```

### 1.3 Projekt Struktúra Létrehozása

```bash
# Mappák létrehozása
mkdir Controllers
mkdir Model
mkdir Validators
mkdir Options

# Fájlok létrehozása
touch Program.cs
touch appsettings.json
touch appsettings.Development.json
```

---

## 2. REST API Kliens (HttpClient)

### 2.1 HttpClient Handler Osztály

**Fájl:** `ChuckApiHandler.cs`

```csharp
using System.Net.Http.Json;

public class ChuckApiHandler
{
    private readonly HttpClient client;
    private const string BaseUrl = "https://api.chucknorris.io/jokes/random";

    public ChuckApiHandler()
    {
        client = new HttpClient();
    }

    /// <summary>
    /// GET kérés - Véletlenszerű vicc lekérése
    /// </summary>
    public async Task<JokeResponse?> GetJokeAsync()
    {
        try
        {
            // HTTP GET kérés
            var response = await client.GetAsync(BaseUrl);
            
            // Sikeres válasz ellenőrzése
            response.EnsureSuccessStatusCode();
            
            // JSON deserializálás
            return await response.Content.ReadFromJsonAsync<JokeResponse>();
        }
        catch (HttpRequestException e)
        {
            Console.WriteLine($"HTTP hiba: {e.Message}");
            return null;
        }
    }

    /// <summary>
    /// PATCH kérés - Generikus típussal
    /// </summary>
    public async Task<T?> PatchJokeAsync<T>(string url, T data)
    {
        var content = JsonContent.Create(data);
        var response = await client.PatchAsync(url, content);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<T>();
    }
}

// Response model
public class JokeResponse
{
    public string Id { get; set; } = "";
    public string Value { get; set; } = "";
    public string Url { get; set; } = "";
}
```

### 2.2 Program.cs (Console App)

```csharp
using System;

var handler = new ChuckApiHandler();

Console.WriteLine("Chuck Norris vicc lekérése...");

// Async task futtatása szinkron környezetben
var joke = handler.GetJokeAsync().Result;

if (joke != null)
{
    Console.WriteLine($"Vicc: {joke.Value}");
    Console.WriteLine($"URL: {joke.Url}");
}
else
{
    Console.WriteLine("Hiba történt a lekérés során.");
}
```

---

## 3. Controller-based API (CRUD)

### 3.1 Model Osztály

**Fájl:** `Model/TodoItem.cs`

```csharp
namespace TodoApiController.Model;

/// <summary>
/// Todo feladat adatmodell
/// </summary>
public class TodoItem
{
    /// <summary>Egyedi azonosító</summary>
    public int Id { get; set; }
    
    /// <summary>Feladat címe (10-200 karakter)</summary>
    public string Title { get; set; } = "";
    
    /// <summary>Részletes leírás (kötelező mező)</summary>
    public string Description { get; set; } = "";
    
    /// <summary>Határidő</summary>
    public DateTime DueDate { get; set; }
}
```

### 3.2 Controller Teljes CRUD

**Fájl:** `Controllers/TodoController.cs`

```csharp
using Microsoft.AspNetCore.Mvc;
using TodoApiController.Model;
using FluentValidation;

namespace TodoApiController.Controllers;

[Route("api/[controller]")]  // Route: /api/todo
[ApiController]
public class TodoController : ControllerBase
{
    private readonly IDataStore dataStore;
    private readonly IValidator<TodoItem> validator;

    public TodoController(IDataStore dataStore, IValidator<TodoItem> validator)
    {
        this.dataStore = dataStore;
        this.validator = validator;
    }

    /// <summary>
    /// GET /api/todo - Összes elem lekérése
    /// </summary>
    [HttpGet]
    public IEnumerable<TodoItem> Get()
    {
        return ((IItemStore<TodoItem>)dataStore).GetAll();
    }

    /// <summary>
    /// GET /api/todo/{id} - Egy elem lekérése
    /// </summary>
    [HttpGet("{id}")]
    public IActionResult Get(int id)
    {
        var item = Get().FirstOrDefault(x => x.Id == id);
        return item == null ? NotFound() : Ok(item);
    }

    /// <summary>
    /// POST /api/todo - Új elem létrehozása
    /// </summary>
    [HttpPost]
    public IActionResult Post([FromBody] TodoItem value)
    {
        // FluentValidation
        var result = validator.Validate(value);
        if (!result.IsValid)
        {
            return BadRequest(result.Errors);
        }

        dataStore.Add(value);
        return Ok(value);
    }

    /// <summary>
    /// PUT /api/todo/{id} - Elem módosítása
    /// </summary>
    [HttpPut("{id}")]
    public IActionResult Put(int id, [FromBody] TodoItem value)
    {
        // ID egyezés ellenőrzése
        if (id != value.Id)
        {
            return BadRequest("ID mismatch");
        }

        // Validáció
        var result = validator.Validate(value);
        if (!result.IsValid)
        {
            return BadRequest(result.Errors);
        }

        // Frissítés
        if (!dataStore.Update(value))
        {
            return NotFound();
        }

        return Ok(value);
    }

    /// <summary>
    /// DELETE /api/todo/{id} - Elem törlése
    /// </summary>
    [HttpDelete("{id}")]
    public IActionResult Delete(int id)
    {
        var item = Get().FirstOrDefault(x => x.Id == id);
        if (item == null)
        {
            return NotFound();
        }

        dataStore.Delete(item);
        return Ok();
    }
}
```

### 3.3 Program.cs (Controller Setup)

```csharp
using FluentValidation;
using TodoApiController.Model;
using TodoApiController.Validators;

var builder = WebApplication.CreateBuilder(args);

// Controller-ek aktiválása
builder.Services.AddControllers();

// DataStore regisztrálása (Singleton)
builder.Services.AddSingleton<IDataStore, DataStore>();

// FluentValidation
builder.Services.AddValidatorsFromAssemblyContaining<TodoItemValidator>();

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Middleware pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();
app.MapControllers();

app.Run();
```

---

## 4. JWT Autentikáció

### 4.1 appsettings.Development.json

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "Jwt": {
    "Key": "EzAKulcs012345EzAKulcs012345",
    "Issuer": "MyIssuer",
    "Audience": "MyAudience"
  }
}
```

### 4.2 JwtOptions Osztály

**Fájl:** `Options/JwtOptions.cs`

```csharp
namespace TodoApiController.Options;

public record JwtOptions
{
    public string Key { get; set; } = "";
    public string Issuer { get; set; } = "";
    public string Audience { get; set; } = "";
}
```

### 4.3 JWT Konfiguráció Program.cs-ben

```csharp
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using TodoApiController.Options;

var builder = WebApplication.CreateBuilder(args);

// JWT konfiguráció betöltése
var jwt = builder.Configuration.GetSection("Jwt").Get<JwtOptions>()!;
builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection("Jwt"));

// JWT Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwt.Issuer,
            ValidAudience = jwt.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwt.Key)
            )
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddControllers();

var app = builder.Build();

// FONTOS: Sorrend kritikus!
app.UseAuthentication();  // ELŐSZÖR
app.UseAuthorization();   // UTÁNA
app.MapControllers();

app.Run();
```

### 4.4 LoginController - Token Generálás

**Fájl:** `Controllers/LoginController.cs`

```csharp
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using TodoApiController.Options;

namespace TodoApiController.Controllers;

[Route("api/[controller]")]
[ApiController]
public class LoginController : ControllerBase
{
    private readonly JwtOptions jwtOptions;

    public LoginController(IOptions<JwtOptions> options)
    {
        jwtOptions = options.Value;
    }

    [HttpPost]
    public IActionResult Post([FromBody] LoginRequest request)
    {
        // Egyszerű validáció (demo célból)
        if (request.UserName != request.Password)
        {
            return BadRequest("Invalid credentials");
        }

        // JWT Token generálás
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Key));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, request.UserName),
            new Claim(ClaimTypes.Name, request.UserName),
            new Claim(ClaimTypes.Role, "User")
        };

        var token = new JwtSecurityToken(
            issuer: jwtOptions.Issuer,
            audience: jwtOptions.Audience,
            claims: claims,
            expires: DateTime.Now.AddMinutes(15),
            signingCredentials: credentials
        );

        var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
        return Ok(new { token = tokenString });
    }
}

public class LoginRequest
{
    public string UserName { get; set; } = "";
    public string Password { get; set; } = "";
}
```

### 4.5 Védett Endpoint Használata

```csharp
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Route("api/[controller]")]
[ApiController]
[Authorize]  // JWT token kötelező!
public class SecureController : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        // User információk kinyerése a token-ből
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var userName = User.FindFirst(ClaimTypes.Name)?.Value;
        var role = User.FindFirst(ClaimTypes.Role)?.Value;

        return Ok(new { userId, userName, role });
    }
}
```

---

## 5. FluentValidation

### 5.1 Validator Osztály

**Fájl:** `Validators/TodoItemValidator.cs`

```csharp
using FluentValidation;
using TodoApiController.Model;

namespace TodoApiController.Validators;

public class TodoItemValidator : AbstractValidator<TodoItem>
{
    public TodoItemValidator()
    {
        // Title: kötelező, 10-200 karakter
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required")
            .MinimumLength(10).WithMessage("Title min 10 chars")
            .MaximumLength(200).WithMessage("Title max 200 chars");

        // Description: kötelező
        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Description is required");

        // DueDate: jövőbeli dátum kell
        RuleFor(x => x.DueDate)
            .GreaterThan(DateTime.Now)
            .WithMessage("Due date must be in the future");
    }
}
```

### 5.2 Program.cs Regisztráció

```csharp
using FluentValidation;

// FluentValidation automatikus regisztráció
builder.Services.AddValidatorsFromAssemblyContaining<TodoItemValidator>();
```

### 5.3 Használat Controller-ben

```csharp
[HttpPost]
public IActionResult Post([FromBody] TodoItem value)
{
    var result = validator.Validate(value);
    
    if (!result.IsValid)
    {
        // Hibák visszaadása
        return BadRequest(result.Errors);
    }

    // Sikeres validáció esetén
    dataStore.Add(value);
    return Ok(value);
}
```

---

## 6. In-Memory DataStore

### 6.1 Interface Definíciók

**Fájl:** `Model/IDataStore.cs`

```csharp
namespace TodoApiController.Model;

public interface IDataStore
{
    void Add<T>(T item) where T : class;
    bool Update<T>(T item) where T : class;
    void Delete<T>(T item) where T : class;
}

public interface IItemStore<T> where T : class
{
    IEnumerable<T> GetAll();
}
```

### 6.2 DataStore Implementáció

**Fájl:** `Model/DataStore.cs`

```csharp
namespace TodoApiController.Model;

public class DataStore : IDataStore, IItemStore<TodoItem>, IItemStore<User>
{
    private readonly Dictionary<int, TodoItem> todos = new();
    private readonly Dictionary<string, User> users = new();
    private int todoIdCounter = 1;

    public DataStore()
    {
        // Kezdeti adatok feltöltése
        var todo1 = new TodoItem
        {
            Id = todoIdCounter++,
            Title = "Első feladat",
            Description = "Ez egy teszt feladat",
            DueDate = DateTime.Now.AddDays(7)
        };
        todos.Add(todo1.Id, todo1);
    }

    // Todo műveletek
    IEnumerable<TodoItem> IItemStore<TodoItem>.GetAll()
    {
        return todos.Values;
    }

    public void Add<T>(T item) where T : class
    {
        if (item is TodoItem todo)
        {
            todo.Id = todoIdCounter++;
            todos.Add(todo.Id, todo);
        }
        else if (item is User user)
        {
            users.Add(user.UserName, user);
        }
    }

    public bool Update<T>(T item) where T : class
    {
        if (item is TodoItem todo)
        {
            if (!todos.ContainsKey(todo.Id))
                return false;
            
            todos[todo.Id] = todo;
            return true;
        }
        else if (item is User user)
        {
            if (!users.ContainsKey(user.UserName))
                return false;
            
            users[user.UserName] = user;
            return true;
        }
        return false;
    }

    public void Delete<T>(T item) where T : class
    {
        if (item is TodoItem todo)
        {
            todos.Remove(todo.Id);
        }
        else if (item is User user)
        {
            users.Remove(user.UserName);
        }
    }

    // User műveletek
    IEnumerable<User> IItemStore<User>.GetAll()
    {
        return users.Values;
    }
}
```

---

## 7. Password Hashing (PBKDF2)

### 7.1 User Model Password Hasheléssel

**Fájl:** `Model/User.cs`

```csharp
using System.Security.Cryptography;
using System.Text;

namespace TodoApiController.Model;

public class User
{
    // FIGYELEM: Éles környezetben felhasználónként egyedi Salt kell!
    public readonly byte[] Salt = Encoding.UTF8.GetBytes("0123456789012345");
    const int Iterations = 10_000;
    const int HashSize = 32;

    public string UserName { get; set; } = "";
    public string Email { get; set; } = "";
    public string Name { get; set; } = "";
    public byte[] Password { get; private set; } = Array.Empty<byte>();

    // Jelszó beállításakor automatikus hashing
    public string PasswordText
    {
        set
        {
            Password = new Rfc2898DeriveBytes(value, Salt, Iterations)
                .GetBytes(HashSize);
        }
    }

    // Jelszó ellenőrzés
    public bool Matches(string password)
    {
        var bytes = new Rfc2898DeriveBytes(password, Salt, Iterations)
            .GetBytes(HashSize);
        
        return bytes.Length == Password.Length 
            && Enumerable.Range(0, bytes.Length)
                .All(i => bytes[i] == Password[i]);
    }
}
```

### 7.2 Használat

```csharp
// Új user létrehozása
var user = new User
{
    UserName = "testuser",
    Email = "test@example.com",
    Name = "Test User",
    PasswordText = "myPassword123"  // Automatikusan hash-elődik!
};

// Jelszó ellenőrzés
if (user.Matches("myPassword123"))
{
    Console.WriteLine("Helyes jelszó!");
}
```

---

## 8. Role-Based Authorization

### 8.1 Claims Hozzáadása Token-hez

```csharp
var claims = new[]
{
    new Claim(ClaimTypes.NameIdentifier, user.UserName),
    new Claim(ClaimTypes.Name, user.Name),
    new Claim(ClaimTypes.Email, user.Email),
    new Claim(ClaimTypes.Role, "Administrator"),  // Role hozzáadása
    new Claim(ClaimTypes.Role, "User")            // Több role is lehet
};
```

### 8.2 [Authorize] Attribútum Használata

```csharp
// 1. Bármilyen bejelentkezett felhasználó
[Authorize]
public IActionResult Get() { ... }

// 2. Konkrét role megkövetelése
[Authorize(Roles = "Administrator")]
public IActionResult Delete(int id) { ... }

// 3. Több role közül bármelyik
[Authorize(Roles = "Administrator,Manager")]
public IActionResult SpecialAction() { ... }
```

### 8.3 Kód Szintű Role Ellenőrzés

```csharp
[HttpPut("{id}")]
[Authorize]
public IActionResult Put(int id, [FromBody] Item value)
{
    var identity = HttpContext.User.Identity as ClaimsIdentity;
    
    // Bejelentkezett user ID
    var currentUserId = identity.Claims
        .FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value;
    
    // Saját adat módosítása vagy Admin?
    if (id.ToString() != currentUserId)
    {
        // Admin role ellenőrzése
        var isAdmin = identity.Claims
            .Where(x => x.Type == ClaimTypes.Role)
            .Any(x => x.Value == "Administrator");
        
        if (!isAdmin)
        {
            return Forbid("Only admins can modify others' data");
        }
    }
    
    // Módosítás...
    return Ok(value);
}
```

---

## 9. Minimal API

### 9.1 Alap Minimal API Program.cs

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// GET endpoint
app.MapGet("/weatherforecast", () =>
{
    var forecasts = new[]
    {
        new WeatherForecast(DateTime.Now, 25, "Warm"),
        new WeatherForecast(DateTime.Now.AddDays(1), 30, "Hot")
    };
    return forecasts;
});

// POST endpoint
app.MapPost("/login", (LoginRequest request) =>
{
    if (request.User == request.Password)
    {
        return Results.Ok("Login successful");
    }
    return Results.Unauthorized();
});

// Route paraméterrel
app.MapGet("/items/{id}", (int id) =>
{
    return Results.Ok(new { Id = id, Name = $"Item {id}" });
});

app.Run();

record WeatherForecast(DateTime Date, int TemperatureC, string Summary);
record LoginRequest(string User, string Password);
```

### 9.2 Minimal API + JWT

```csharp
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

var jwtKey = "EzAKulcs012345EzAKulcs012345";
var jwtIssuer = "MyIssuer";
var jwtAudience = "MyAudience";

// JWT setup
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

// Login endpoint - token generálás
app.MapPost("/login", (string user, string password) =>
{
    if (user != password) return Results.Forbid();

    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
    var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
    
    var claims = new[]
    {
        new Claim(ClaimTypes.NameIdentifier, user),
        new Claim(ClaimTypes.Role, "User")
    };

    var token = new JwtSecurityToken(
        issuer: jwtIssuer,
        audience: jwtAudience,
        claims: claims,
        expires: DateTime.Now.AddMinutes(15),
        signingCredentials: credentials
    );

    var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
    return Results.Ok(tokenString);
});

// Védett endpoint
app.MapGet("/secure", () => "This is secure!")
    .RequireAuthorization();

app.Run();
```

---

## 10. FastEndpoints

### 10.1 NuGet Csomag

```bash
dotnet add package FastEndpoints --version 5.30.0
```

### 10.2 Program.cs FastEndpoints Setup

```csharp
using FastEndpoints;

var builder = WebApplication.CreateBuilder(args);

// FastEndpoints regisztrálása
builder.Services.AddFastEndpoints();

var app = builder.Build();

// FastEndpoints middleware
app.UseFastEndpoints();

app.Run();
```

### 10.3 Endpoint Osztály Sablon

**Fájl:** `Endpoints/GetTodosEndpoint.cs`

```csharp
using FastEndpoints;

namespace MyApi.Endpoints;

public class GetTodosEndpoint : Endpoint<EmptyRequest, List<TodoItem>>
{
    public override void Configure()
    {
        Get("/todos");
        AllowAnonymous();  // vagy Roles("Admin")
    }

    public override async Task HandleAsync(EmptyRequest req, CancellationToken ct)
    {
        var todos = new List<TodoItem>
        {
            new TodoItem { Id = 1, Title = "Todo 1" },
            new TodoItem { Id = 2, Title = "Todo 2" }
        };

        await SendAsync(todos, cancellation: ct);
    }
}
```

### 10.4 Endpoint Request/Response Típusokkal

```csharp
using FastEndpoints;

// Request model
public class CreateTodoRequest
{
    public string Title { get; set; } = "";
    public string Description { get; set; } = "";
}

// Response model
public class TodoResponse
{
    public int Id { get; set; }
    public string Title { get; set; } = "";
}

// Endpoint
public class CreateTodoEndpoint : Endpoint<CreateTodoRequest, TodoResponse>
{
    public override void Configure()
    {
        Post("/todos");
        Roles("User", "Admin");
    }

    public override async Task HandleAsync(CreateTodoRequest req, CancellationToken ct)
    {
        // Validáció automatikus, ha van Validator<CreateTodoRequest>
        
        var response = new TodoResponse
        {
            Id = 123,
            Title = req.Title
        };

        await SendAsync(response, cancellation: ct);
    }
}
```

### 10.5 FastEndpoints Validator

```csharp
using FastEndpoints;
using FluentValidation;

public class CreateTodoValidator : Validator<CreateTodoRequest>
{
    public CreateTodoValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty()
            .MinimumLength(5);
        
        RuleFor(x => x.Description)
            .NotEmpty();
    }
}
```

---

## 11. Gyakori Hibák és Megoldások

### 11.1 JWT Token Nem Működik

**Hiba:** 401 Unauthorized minden védett endpoint-nál

**Megoldás:**
```csharp
// 1. Ellenőrizd a middleware sorrendet
app.UseAuthentication();  // ELŐSZÖR!
app.UseAuthorization();   // UTÁNA!
app.MapControllers();

// 2. Ellenőrizd a JWT kulcs hosszát
"Key": "Minimum32KarakterHosszuKulcsKell!"  // Min 32 char!

// 3. Ellenőrizd az Issuer és Audience egyezést
// appsettings.json és TokenValidationParameters ugyanaz kell!
```

### 11.2 CORS Hiba

**Megoldás:**
```csharp
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();
app.UseCors();  // UseAuthentication() ELŐTT!
```

### 11.3 FluentValidation Nem Fut

**Megoldás:**
```csharp
// Program.cs-ben regisztráld
builder.Services.AddValidatorsFromAssemblyContaining<TodoItemValidator>();

// Controller-ben manuálisan hívd meg
var result = validator.Validate(value);
if (!result.IsValid)
{
    return BadRequest(result.Errors);
}
```

### 11.4 DataStore Üres Újraindítás Után

**Normális viselkedés!** In-memory store = minden adat törlődik.

**Megoldás:** Inicializálás a konstruktorban
```csharp
public DataStore()
{
    // Kezdeti adatok feltöltése
    var todo = new TodoItem { Title = "Teszt", ... };
    todos.Add(1, todo);
}
```

### 11.5 FastEndpoints SendAsync vs SendOkAsync

**Hiba:** `SendOkAsync` nem létezik FastEndpoints 5.x-ben!

**Megoldás:**
```csharp
// ❌ ROSSZ (FastEndpoints 7.x)
await SendOkAsync(data, ct);

// ✅ JÓ (FastEndpoints 5.30.0)
await SendAsync(data, cancellation: ct);
```

---

## 🧪 Tesztelési Parancsok (cURL)

### Login + Token Használat

```bash
# 1. Login - token megszerzése
curl -X POST http://localhost:5000/api/login \
  -H "Content-Type: application/json" \
  -d '{"userName":"test","password":"test"}'

# Válasz: { "token": "eyJhbGci..." }

# 2. Token használata védett endpoint-nál
curl -X GET http://localhost:5000/api/secure \
  -H "Authorization: Bearer eyJhbGci..."
```

### CRUD Műveletek

```bash
# GET - Összes elem
curl http://localhost:5000/api/todo

# GET - Egy elem
curl http://localhost:5000/api/todo/1

# POST - Új elem
curl -X POST http://localhost:5000/api/todo \
  -H "Content-Type: application/json" \
  -d '{
    "title": "Új feladat",
    "description": "Leírás",
    "dueDate": "2025-12-31T00:00:00"
  }'

# PUT - Módosítás
curl -X PUT http://localhost:5000/api/todo/1 \
  -H "Content-Type: application/json" \
  -d '{
    "id": 1,
    "title": "Módosított cím",
    "description": "Új leírás",
    "dueDate": "2025-12-31T00:00:00"
  }'

# DELETE - Törlés
curl -X DELETE http://localhost:5000/api/todo/1
```

---

## 📝 Gyors Referencia Táblázat

| Funkció | Fájl | Kulcs Kódrészlet |
|---------|------|------------------|
| Controller CRUD | `Controllers/XController.cs` | `[HttpGet]`, `[HttpPost]`, `[HttpPut]`, `[HttpDelete]` |
| JWT Setup | `Program.cs` | `AddAuthentication(JwtBearerDefaults.AuthenticationScheme)` |
| Token Generálás | `LoginController.cs` | `new JwtSecurityToken(issuer, audience, claims, expires, credentials)` |
| Validáció | `Validators/XValidator.cs` | `RuleFor(x => x.Prop).NotEmpty()` |
| DataStore | `Model/DataStore.cs` | `Dictionary<int, T>` + `IDataStore` implementálás |
| Password Hash | `Model/User.cs` | `new Rfc2898DeriveBytes(password, salt, iterations)` |
| Authorize | Controller method | `[Authorize]` vagy `[Authorize(Roles = "Admin")]` |
| Minimal API | `Program.cs` | `app.MapGet("/route", () => ...)` |
| FastEndpoints | `Endpoints/XEndpoint.cs` | `Endpoint<TRequest, TResponse>` + `Configure()` + `HandleAsync()` |

---

## ✅ Dolgozat Checklist

Mielőtt beadod a kódot, ellenőrizd:

- [ ] **Projekt buildelődik** (`dotnet build` - 0 error)
- [ ] **Futtatható** (`dotnet run` - nincs crash)
- [ ] **JWT kulcs min 32 karakter** (appsettings.json)
- [ ] **Middleware sorrend helyes** (Authentication → Authorization)
- [ ] **FluentValidation regisztrálva** (Program.cs)
- [ ] **Controller route-ok helyesek** (`[Route("api/[controller]")]`)
- [ ] **HTTP method-ok megfelelőek** (GET/POST/PUT/DELETE)
- [ ] **IActionResult használat** (Ok, BadRequest, NotFound, stb.)
- [ ] **Async/await használat** (async Task<...>)
- [ ] **Try-catch error handling** (ahol szükséges)
- [ ] **Swagger elérhető** (http://localhost:XXXX/swagger)

---

## 🎯 Utolsó Tippek

1. **Kis lépésekben haladj:** Először alap projekt → Controller → Model → Validator → JWT
2. **Tesztelj gyakran:** Minden új endpoint után próbáld ki Swagger-ben vagy cURL-lel
3. **Nézd meg a hibákat:** `dotnet build` és konzol output mindig megmutatja a problémát
4. **Másold a kódot:** Ezek a kódrészletek kimásolhatók és működőképesek!
5. **Swagger a barátod:** Ott minden endpoint tesztelhető, nem kell cURL

**Sok sikert a dolgozathoz! 🚀**
