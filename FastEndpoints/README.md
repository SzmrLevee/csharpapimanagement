# FastEndpoints - Modern API Architektúra Fast

Endpoints Keretrendszerrel

## 📚 Miről szól ez a projekt?

Ez a projekt a **FastEndpoints** nevű modern C# keretrendszert mutatja be, ami egy **alternatíva** a hagyományos ASP.NET Core Controller-ek és Minimal API pattern helyett.

### Amit megtanulhatsz:

- **FastEndpoints framework** használata - gyorsabb és típusbiztosabb, mint a Controller-ek
- **Endpoint-based architektúra** - minden végpont saját osztály (Single Responsibility)
- **Built-in validáció** - FluentValidation integráció endpoint szinten
- **Request/Response típusosság** - compile-time type safety
- **JWT Authentication** FastEndpoints módra
- **Cancellation Token** támogatás beépített módon
- **Endpoint konfigur áció** fluent API-val (`Configure()` metódus)

## 🚀 Telepítés és Futtatás

### 1. Navigálj a projekt könyvtárába:
```bash
cd FastEndpoints/MinimalAPIDemo
```

### 2. Futtasd a projektet:
```bash
dotnet run
```

### 3. Nyisd meg a böngészőben:
- **HTTP:** http://localhost:5091/swagger (vagy konfiguráció szerinti port)

## 🔑 FastEndpoints vs. Controller vs. Minimal API

| Jellemző | **FastEndpoints** | **Controller** | **Minimal API** |
|----------|-------------------|----------------|-----------------|
| Endpoint per osztály | ✅ Igen | ❌ Nem | ❌ Nem |
| Type-safe request/response | ✅ Igen | ⚠️ Részben | ⚠️ Részben |
| Built-in validáció | ✅ Igen | ⚠️ Manuális | ⚠️ Manuális |
| Kód organizáció | ✅ Kiváló | ⚠️ Jó | ❌ Gyenge |
| Performancia | ✅ Gyors | ⚠️ Közepes | ✅ Gyors |
| Learning curve | ⚠️ Új framework | ✅ Jól ismert | ✅ Egyszerű |

**FastEndpoints előnyei:**
- Minden endpoint **saját osztály** → Single Responsibility Principle
- **Típusbiztos** request és response objektumok (generikusan megadható)
- **Beépített validáció** FluentValidation-nel
- **CancellationToken** automatikusan injektálva
- **Fluent API** az endpoint konfiguráláshoz

**Mikor használd?**
- Nagyobb projektek, ahol sok endpoint van
- Ha szereted a CLEAN architektúrát és SOLID elveket
- Ha típusbiztos API-t akarsz fordítási időben

## 🏗️ Projekt Architektúra

```
FastEndpoints/MinimalAPIDemo/
├── Endpoints/                # Minden endpoint külön fájlban!
│   ├── LoginEndPoint.cs      # POST /login - JWT token generálás
│   ├── GetWeather.cs         # GET /weather - Időjárás adatok
│   ├── IdAlapjan.cs          # GET /id_alapjan/{id} - ID alapú lekérdezés
│   └── UjBeallitas.cs        # POST /uj_beallitas - Új beállítás feltöltés
├── Program.cs                # FastEndpoints konfiguráció
├── JwtSettings.cs            # JWT beállítások model
└── JwtSettingsValidator.cs   # FluentValidation a JWT config-hoz
```

## 📖 FastEndpoints Endpoint Szerkezete

Minden endpoint **három fő részből** áll:

### 1. Endpoint Osztály Definíció
```csharp
public class LoginEndPoint : Endpoint<LoginData, string>
//                            ^^^^^^^^ ^^^^^^^^^  ^^^^^^
//                            Ősosztály  Request   Response
```

- **`Endpoint<TRequest, TResponse>`**: Generikus ősosztály
- **`TRequest`**: Bejövő kérés típusa (LoginData)
- **`TResponse`**: Válasz típusa (string - a JWT token)

### 2. Configure() Metódus - Endpoint Beállítások
```csharp
public override void Configure()
{
    Post("/login");        // HTTP metódus + útvonal
    AllowAnonymous();      // Nincs authentikáció szükséges
    // Roles("Admin");     // vagy: csak Admin role-lal elérhető
}
```

**Lehetséges konfigurációk:**
- `Get("/path")`, `Post("/path")`, `Put("/path")`, `Delete("/path")` - HTTP metódus
- `AllowAnonymous()` - Nincs auth szükséges
- `Roles("Admin", "User")` - Role-based authorization
- `Policies("PolicyName")` - Policy-based authorization

### 3. HandleAsync() Metódus - Üzleti Logika
```csharp
public override async Task HandleAsync(LoginData req, CancellationToken ct)
{
    // Validáció (automatikus a FluentValidation miatt, ha van validator)
    if (req.user != req.password)
    {
        await SendAsync("Forbidden", statusCode: 403, cancellation: ct);
        return;
    }
    
    // Token generálás...
    await SendAsync(tokenStr, cancellation: ct);
}
```

**Built-in metódusok:**
- `SendAsync(response, ct)` - 200 OK válasz
- `SendAsync(response, statusCode, ct)` - Custom status code
- `SendErrorAsync(ct)` - Validation error válasz
- `ThrowError("message")` - Exception dobása

## 🛣️ API Végpontok (Endpoints)

### 1. POST /login - JWT Token Generálás

**Endpoint osztály:** `LoginEndPoint.cs`

**Request Model:**
```csharp
public class LoginData
{
    public string user { get; set; } = "";
    public string password { get; set; } = "";
}
```

**Példa kérés:**
```json
{
  "user": "testuser",
  "password": "testuser"
}
```

**Válasz (200 OK):**
```json
"eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJuYW1laWQiOiJ0ZXN0dXNlciIsInJvbGUiOiJGaW5hbmNlIiwibmJmIjoxNzM3MjQ4MDAwLCJleHAiOjE3MzcyNDg5MDAsImlhdCI6MTczNzI0ODAwMCwiaXNzIjoiS2lib2NzYXRvIiwiYXVkIjoiQ2Vsa296b25zZWcifQ.XYZ123..."
```

**Válasz (403 Forbidden):**
```json
"Forbidden"
```

**Logika:**
- Egyszerű validáció: `user == password` (demo célból)
- Sikeres login esetén JWT token generálás
- Claims: NameIdentifier (username) + Role ("Finance")
- Token érvényesség: 15 perc

**Teljes kód:**
```csharp
public class LoginEndPoint : Endpoint<LoginData, string>
{
    private readonly JwtSettings jwt;

    public LoginEndPoint(IOptions<JwtSettings> options)
    {
        jwt = options.Value;  // DI injektálja a JWT beállításokat
    }

    public override void Configure()
    {
        Post("/login");        // POST metódus, /login útvonal
        AllowAnonymous();      // Nincs authentikáció szükséges
    }

    public override async Task HandleAsync(LoginData req, CancellationToken ct)
    {
        // Validáció: user és password egyezés (demo célból)
        if (req.user != req.password)
        {
            await SendAsync("Forbidden", statusCode: 403, cancellation: ct);
            return;
        }

        // JWT token generálás (ugyanúgy, mint Controller-eknél)
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Key));
        var cred = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, req.user),
            new Claim(ClaimTypes.Role, "Finance"),
        };
        var token = new JwtSecurityToken(
            issuer: jwt.Issuer,
            audience: jwt.Audience,
            claims: claims,
            expires: DateTime.Now.AddMinutes(15),
            signingCredentials: cred
        );

        var tokenStr = new JwtSecurityTokenHandler().WriteToken(token);
        await SendAsync(tokenStr, cancellation: ct);  // 200 OK + token
    }
}
```

### 2. GET /weather - Időjárás Adatok (Védett)

**Endpoint osztály:** `GetWeather.cs`

**Példa válasz:**
```json
[
  {
    "date": "2024-01-15",
    "temperatureC": 25,
    "temperatureF": 76,
    "summary": "Warm"
  }
]
```

**Védelem:**
- JWT authentikáció szükséges
- Authorization header: `Bearer <token>`

### 3. GET /id_alapjan/{id} - ID Alapú Lekérdezés

**Endpoint osztály:** `IdAlapjan.cs`

**Request:** URL paraméter - `id` (int)

**Példa:** `GET /id_alapjan/123`

**Válasz:** ID-specifikus adat

### 4. POST /uj_beallitas - Új Beállítás Feltöltés

**Endpoint osztály:** `UjBeallitas.cs`

**Request Model:** Beállítás objektum (projekt-specifikus)

**Válasz:** Sikeres feltöltés visszaigazolás

## 🔧 Program.cs Konfiguráció

### FastEndpoints Regisztrálása

```csharp
using FastEndpoints;

var builder = WebApplication.CreateBuilder(args);

// JWT autentikáció (ugyanúgy, mint Controller-eknél)
var jwt = builder.Configuration.GetSection("jwt").Get<JwtSettings>()!;
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("Jwt"));

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new()
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwt.Issuer,
            ValidAudience = jwt.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Key)),
        };
    });

// FluentValidation validátorok regisztrálása
builder.Services.AddValidatorsFromAssemblyContaining<JwtSettingsValidator>();

// KRITIKUS: FastEndpoints aktiválása!
builder.Services.AddFastEndpoints();

var app = builder.Build();

// Middleware pipeline
app.UseAuthentication();
app.UseAuthorization();

// KRITIKUS: FastEndpoints middleware!
// Ez olvassa be az összes Endpoint<TRequest, TResponse> osztályt és regisztrálja
app.UseFastEndpoints();

app.Run();
```

**Kulcspontok:**
- `AddFastEndpoints()` - Service regisztráció
- `UseFastEndpoints()` - Middleware aktiválás (endpoint felderítés + routing)

## 📚 Használt NuGet Csomagok

```xml
<PackageReference Include="FastEndpoints" Version="5.30.0" />
<PackageReference Include="Microsoft.AspNetCore.Authentication.JwtBearer" Version="8.0.11" />
<PackageReference Include="Microsoft.IdentityModel.Tokens" Version="8.2.1" />
<PackageReference Include="System.IdentityModel.Tokens.Jwt" Version="8.2.1" />
<PackageReference Include="FluentValidation" Version="11.3.0" />
<PackageReference Include="FluentValidation.DependencyInjectionExtensions" Version="11.3.0" />
```

**FONTOS:** FastEndpoints 5.30.0 verziót használjuk! A 7.x verzió más API-t használ.

## 💡 Tanulási Pontok

### 1. Miért Endpoint osztályok?

**Hagyományos Controller probléma:**
```csharp
public class UserController : ControllerBase
{
    [HttpGet] public IActionResult GetAll() { ... }
    [HttpGet("{id}")] public IActionResult GetById(int id) { ... }
    [HttpPost] public IActionResult Create(User user) { ... }
    [HttpPut] public IActionResult Update(User user) { ... }
    [HttpDelete("{id}")] public IActionResult Delete(int id) { ... }
}
// Egyetlen osztály 5 különböző felelősséggel!
```

**FastEndpoints megoldás:**
```csharp
public class GetAllUsersEndpoint : Endpoint<EmptyRequest, List<User>> { ... }
public class GetUserByIdEndpoint : Endpoint<GetByIdRequest, User> { ... }
public class CreateUserEndpoint : Endpoint<User, User> { ... }
public class UpdateUserEndpoint : Endpoint<User, User> { ... }
public class DeleteUserEndpoint : Endpoint<DeleteRequest, EmptyResponse> { ... }
// 5 osztály, 5 felelősség - Single Responsibility Principle!
```

### 2. Típusbiztos Request/Response

**Controller - Type-unsafe:**
```csharp
[HttpPost]
public IActionResult Post([FromBody] object value)  // object? Mi ez?
{
    return Ok(someData);  // Mi a return type?
}
```

**FastEndpoints - Type-safe:**
```csharp
public class LoginEndPoint : Endpoint<LoginData, string>
//                                    ^^^^^^^^^ ^^^^^^
//                                    Request   Response
```

Fordítási időben tudjuk, hogy:
- Bejövő kérés: `LoginData` objektum
- Válasz: `string` (JWT token)

### 3. Automatikus Validáció

Ha létrehozol egy `Validator<TRequest>` osztályt, a FastEndpoints **automatikusan** validálja a kérést:

```csharp
public class LoginDataValidator : Validator<LoginData>
{
    public LoginDataValidator()
    {
        RuleFor(x => x.user).NotEmpty().MinimumLength(3);
        RuleFor(x => x.password).NotEmpty().MinimumLength(6);
    }
}
```

Ha a validáció sikertelen, automatikus 400 Bad Request válasz megy!

### 4. Dependency Injection Endpoint-okban

```csharp
public class LoginEndPoint : Endpoint<LoginData, string>
{
    private readonly JwtSettings jwt;
    private readonly IUserService userService;

    public LoginEndPoint(IOptions<JwtSettings> options, IUserService userService)
    {
        jwt = options.Value;
        this.userService = userService;
    }
}
```

Ugyanúgy működik, mint Controller-eknél - DI konténer injektálja a függőségeket!

## 🔄 Migration Guide: Controller → FastEndpoints

### Előtte (Controller):
```csharp
[Route("api/[controller]")]
[ApiController]
public class WeatherController : ControllerBase
{
    [HttpGet]
    [Authorize]
    public IActionResult Get()
    {
        var data = GetWeatherData();
        return Ok(data);
    }
}
```

### Utána (FastEndpoints):
```csharp
public class GetWeatherEndpoint : Endpoint<EmptyRequest, List<WeatherForecast>>
{
    public override void Configure()
    {
        Get("/weather");
        Roles("User");  // Authorize equivalent
    }

    public override async Task HandleAsync(EmptyRequest req, CancellationToken ct)
    {
        var data = GetWeatherData();
        await SendAsync(data, cancellation: ct);
    }
}
```

## ⚠️ Gyakori Hibák

### 1. Elfelejtett UseFastEndpoints()
```csharp
// ❌ ROSSZ - nem működnek az endpoint-ok!
app.UseAuthentication();
app.UseAuthorization();
app.Run();

// ✅ JÓ
app.UseAuthentication();
app.UseAuthorization();
app.UseFastEndpoints();  // Kritikus!
app.Run();
```

### 2. Verzió inkompatibilitás
```csharp
// ❌ FastEndpoints 7.x használ más API-t!
await SendOkAsync(data, ct);  // Nincs ilyen metódus 5.x-ben!

// ✅ FastEndpoints 5.30.0
await SendAsync(data, cancellation: ct);
```

### 3. CancellationToken paraméter név
```csharp
// ❌ ROSSZ
public override async Task HandleAsync(LoginData req, CancellationToken cancellationToken)
{
    await SendAsync(data, cancellation: cancellationToken);  // Hosszú név
}

// ✅ JÓ - konvenció szerint "ct"
public override async Task HandleAsync(LoginData req, CancellationToken ct)
{
    await SendAsync(data, cancellation: ct);
}
```

## 🎯 Következő Lépések

Ha már megértetted ezt a projektet:
- **Vertical Slice Architecture** - FastEndpoints feature folder struktúrával
- **REPR Pattern** (Request-Endpoint-Response) - tiszta architektúra
- **FastEndpoints Testing** - beépített integration testing support
- **Pre/Post Processors** - middleware-like logika endpoint szinten
- **Event Publishing** - pub/sub pattern FastEndpoints-ben

## 📖 További Olvasnivaló

- [FastEndpoints Hivatalos Dokumentáció](https://fast-endpoints.com/)
- [FastEndpoints GitHub](https://github.com/FastEndpoints/FastEndpoints)
- [Comparison: Controllers vs FastEndpoints](https://fast-endpoints.com/docs/get-started#why-not-minimal-apis-or-controllers)
- [REPR Pattern](https://deviq.com/design-patterns/repr-design-pattern)

## 🚀 Miért Használd FastEndpoints-et?

**✅ Használd, ha:**
- Szereted a **CLEAN Architecture**-t
- Preferálod a **SOLID** elveket (különösen Single Responsibility)
- Nagy projektben dolgozol sok endpoint-tal
- Típusbiztos API-t akarsz fordítási időben
- Vertical Slice Architecture-t használsz

**❌ NE használd, ha:**
- Egyszerű CRUD API-t készítesz (Controller is elég)
- Csapatod még tanul ASP.NET Core-t (Controller ismerősebb)
- Nem akarsz új frameworköt tanulni

**Záró gondolat:** FastEndpoints egy **kiválóan megtervezett** framework, ami a modernebb C# API fejlesztés irányába mutat. Ha szereted a típusbiztonságot és a tiszta kód architektúrát, ez neked való!
