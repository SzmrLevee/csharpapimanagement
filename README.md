# C# ASP.NET Core API Tanulási Projektek

Ez a repository C# ASP.NET Core API fejlesztés tanulására készült projektek gyűjteménye. Minden projekt egy-egy fontos koncepciót mutat be a modern web API fejlesztésben.

## 📚 Projektek Áttekintése és Elérhető Endpoint-ok

### 1. **RestApiHasznalat** - REST API Kliens
**Tanulási Fókusz:** Külső API-k fogyasztása C#-ban

**Mit tanulhatsz meg:**
- HttpClient használata
- REST API hívások (GET, PATCH)
- JSON deszerializáció
- Async/await pattern külső API-kkal
- Error handling HTTP kérésekben
- Third-party API integráció

**Főbb technológiák:**
- HttpClient
- System.Net.Http
- JSON parsing
- Async programming

**Példa API:** Chuck Norris Jokes API integráció

**Futtatás:** Console alkalmazás, nincs saját endpoint (kliens oldal)

📖 **[Részletes README →](./RestApiHasznalat/README.md)**

---

### 2. **TodoApiController** - Controller-based API
**Tanulási Fókusz:** Hagyományos Controller alapú API, MVC pattern

**Mit tanulhatsz meg:**
- Controller osztályok és Action metódusok
- RESTful API tervezés (GET, POST, PUT, DELETE)
- In-memory data store implementálás
- FluentValidation a controller context-ben
- CRUD műveletek teljes implementációja

**Főbb technológiák:**
- ASP.NET Core Controllers
- FluentValidation
- In-memory DataStore
- Swagger/OpenAPI

**🌐 Elérés:**
- **Port:** http://localhost:5000 (alapértelmezett)
- **Swagger UI:** http://localhost:5000/swagger

**📍 API Endpoint-ok:**
- `GET /api/todo` - Összes todo elem lekérése
- `GET /api/todo/{id}` - Egy todo elem lekérése ID alapján
- `POST /api/todo` - Új todo elem létrehozása
- `PUT /api/todo/{id}` - Meglévő todo elem módosítása
- `DELETE /api/todo/{id}` - Todo elem törlése

📖 **[Részletes README →](./TodoApiController/README.md)**

---

### 3. **Authentication** - Haladó JWT Autentikáció
**Tanulási Fókusz:** Autentikáció, autorizáció, role-based access control

**Mit tanulhatsz meg:**
- JWT Options pattern (`JwtOptions` osztály)
- PBKDF2 password hashing (Rfc2898DeriveBytes)
- Role-Based Authorization (Felhasználó vs. Administrator)
- Claims-based authentication
- User management implementálás
- Interface-based architecture (`IUser`, `IDataStore`)
- Secure token generálás és validálás

**Főbb technológiák:**
- JWT Bearer Authentication
- Options Pattern
- PBKDF2 Password Hashing
- FluentValidation
- Interface Segregation Principle

**🌐 Elérés:**
- **HTTP Port:** http://localhost:5154
- **HTTPS Port:** https://localhost:7036
- **Swagger UI:** http://localhost:5154/swagger

**📍 API Endpoint-ok:**

**Login:**
- `POST /api/login` - JWT token generálás (Body: `{"userName":"user","password":"pass"}`)

**User Management:**
- `GET /api/user` - Összes felhasználó listázása (publikus)
- `GET /api/user/{username}` - Egyedi felhasználó lekérése (publikus)
- `POST /api/user` - Új felhasználó regisztrálása (publikus)
- `PUT /api/user/{username}` - Felhasználó módosítása **[🔒 VÉDETT]** (saját profil vagy Admin)
- `DELETE /api/user/{username}` - Felhasználó törlése **[🔒 ADMIN ONLY]**

**Todo (JWT védelem alatt):**
- `GET /api/todo` - Összes todo elem
- `GET /api/todo/{id}` - Egy todo elem
- `POST /api/todo` - Új todo létrehozása
- `PUT /api/todo/{id}` - Todo módosítás
- `DELETE /api/todo/{id}` - Todo törlés

📖 **[Részletes README →](./Authentication/README.md)**

---

### 4. **MinimalAPIDemo** - Minimal API Alapok
**Tanulási Fókusz:** Minimal API pattern, lambda-based endpoint-ok, JWT

**Mit tanulhatsz meg:**
- Minimal API endpoint-ok létrehozása (`MapGet`, `MapPost`)
- JWT (JSON Web Token) alapú autentikáció implementálása
- Token generálás és validálás
- Authorization middleware használata
- Swagger/OpenAPI dokumentáció
- Route paraméterek kezelése (pl. `/{id}`)
- Claims-based autorizáció
- Lambda expressions endpoint definíciókhoz

**Főbb technológiák:**
- ASP.NET Core Minimal API
- JWT Bearer Authentication
- Microsoft.IdentityModel.Tokens
- Swagger UI
- Lambda-based routing

**🌐 Elérés:**
- **Port:** http://localhost:5091
- **Swagger UI:** http://localhost:5091/swagger

**📍 API Endpoint-ok:**
- `POST /login` - JWT token generálás (Query params: `?user=admin&password=admin`)
- `GET /weatherforecast` - Időjárás adatok lekérése **[🔒 VÉDETT]** (Authorization header szükséges)
- `GET /id_alapjan/{id}` - Adat lekérése ID alapján **[🔒 VÉDETT]**
- `POST /uj_beallitas` - Új beállítás feltöltés **[🔒 VÉDETT]**
- `POST /feltoltes` - Fájl feltöltés **[🔒 VÉDETT]**

**Példa JWT használat:**
```bash
# 1. Token megszerzése
curl -X POST "http://localhost:5091/login?user=admin&password=admin"

# 2. Védett endpoint hívás
curl -H "Authorization: Bearer <TOKEN>" \
     http://localhost:5091/id_alapjan/3
```

📖 **[Részletes README →](./MinimalAPIDemo/README.md)**

---

### 5. **FastEndpoints** - Modern Endpoint Architecture
**Tanulási Fókusz:** FastEndpoints framework, típusbiztos endpoint osztályok

**Mit tanulhatsz meg:**
- FastEndpoints framework használata
- Endpoint-per-class architektúra (Single Responsibility)
- Típusbiztos request/response objektumok (`Endpoint<TRequest, TResponse>`)
- Beépített FluentValidation integráció
- Dependency Injection endpoint-okban
- Strukturált hibakezelés
- CancellationToken automatikus kezelés
- Fluent API endpoint konfiguráláshoz (`Configure()` metódus)

**Főbb technológiák:**
- FastEndpoints 5.30.0
- FluentValidation
- JWT Authentication
- Options pattern
- Type-safe API design

**Különbségek a Minimal API-hoz képest:**
- ✅ Endpoint-ok osztály alapúak, nem inline lambda-k
- ✅ Beépített validáció támogatás
- ✅ Jobb kód szervezés nagyobb projektekhez
- ✅ Type-safe request/response handling
- ✅ Automatikus CancellationToken injektálás

**🌐 Elérés:**
- **Port:** http://localhost:5091 (vagy projekt-specifikus port)
- **Swagger UI:** http://localhost:5091/swagger

**📍 API Endpoint-ok:**
- `POST /login` - JWT token generálás (Body: `{"user":"test","password":"test"}`)
  - **Endpoint Class:** `LoginEndPoint.cs`
  - **Request Type:** `LoginData { user, password }`
  - **Response Type:** `string` (JWT token)
  
- `GET /weather` - Időjárás adatok lekérése **[🔒 VÉDETT]**
  - **Endpoint Class:** `GetWeather.cs`
  
- `GET /id_alapjan/{id}` - ID alapú lekérdezés **[🔒 VÉDETT]**
  - **Endpoint Class:** `IdAlapjan.cs`
  
- `POST /uj_beallitas` - Új beállítás feltöltés **[🔒 VÉDETT]**
  - **Endpoint Class:** `UjBeallitas.cs`

**FastEndpoints Endpoint Szerkezet Példa:**
```csharp
public class LoginEndPoint : Endpoint<LoginData, string>
{
    public override void Configure()
    {
        Post("/login");
        AllowAnonymous();
    }

    public override async Task HandleAsync(LoginData req, CancellationToken ct)
    {
        // Validáció + token generálás
        await SendAsync(token, cancellation: ct);
    }
}
```

📖 **[Részletes README →](./FastEndpoints/README.md)**

---

## 🚀 Gyors Kezdés

### Előfeltételek
- .NET 8.0 SDK
- Visual Studio Code vagy Visual Studio 2022
- Postman vagy hasonló API tesztelő eszköz (opcionális)

### Projekt Futtatása

Minden projekt külön-külön futtatható. A projektek általában 2 szintű mappában vannak:

```bash
# 1. Belépés a belső mappába
cd <ProjektNév>/<ProjektNév>
dotnet run

# 2. VAGY megadni a projekt útvonalát
cd <ProjektNév>
dotnet run --project <ProjektNév>/<ProjektNév>.csproj
```

**Példa - MinimalAPIDemo futtatása:**
```bash
cd MinimalAPIDemo/MinimalAPIDemo
dotnet run
```

vagy

```bash
cd MinimalAPIDemo
dotnet run --project MinimalAPIDemo/MinimalAPIDemo.csproj
```

### Swagger UI Elérése

A legtöbb projekt tartalmaz Swagger UI-t a könnyebb teszteléshez:
```
http://localhost:<PORT>/swagger
```

Például: `http://localhost:5091/swagger`

---

## 🎓 Tanulási Útvonal Javaslat

### Kezdő Szint
1. **RestApiHasznalat** - Kezdd itt! API-k fogyasztása
2. **MinimalAPIDemo** - Alapvető API készítés

### Középhaladó Szint
3. **TodoApiController** - Controller-based API pattern
4. **Authentication** - Autentikáció mélyebb megértése

### Haladó Szint
5. **FastEndpoints** - Modern, strukturált megközelítés

---

## 🔑 Fontos Koncepciók

### JWT Authentication Flow
1. **Login endpoint** - Felhasználó küldi a credentials-t
2. **Token generálás** - Server létrehozza a JWT token-t
3. **Token visszaadás** - Kliens megkapja a token-t
4. **Védett endpoint hívás** - Token küldése az Authorization header-ben
5. **Token validálás** - Server ellenőrzi a token érvényességét

### Minimal API vs Controller API

| Minimal API | Controller API |
|-------------|----------------|
| Lambda-based endpoint-ok | Class-based controllers |
| Kevesebb boilerplate | Több struktúra |
| Modern, egyszerű projektekhez | Hagyományos, enterprise projektekhez |
| Program.cs-ben definiálva | Külön Controller osztályok |

### Dependency Injection Pattern

Minden projekt használja a DI-t:
```csharp
// Regisztráció (Program.cs)
builder.Services.AddScoped<IDataStore, DataStore>();

// Használat (Constructor Injection)
public class MyController 
{
    private readonly IDataStore _dataStore;
    
    public MyController(IDataStore dataStore) 
    {
        _dataStore = dataStore;
    }
}
```

---

## 🛠️ Közös Technológiák

- **ASP.NET Core 8.0** - Modern web framework
- **JWT Bearer Authentication** - Token-based auth
- **Swagger/OpenAPI** - API dokumentáció
- **FluentValidation** - Model validáció
- **Dependency Injection** - IoC pattern

---

## 📝 Projekt Struktúra

```
csharpapi/
├── .gitignore                    # Git ignore fájl
├── README.md                     # Ez a fájl
│
├── MinimalAPIDemo/               # Minimal API projekt
│   ├── MinimalAPIDemo.sln
│   ├── MinimalAPIDemo/
│   │   ├── Program.cs           # Fő belépési pont
│   │   ├── JwtSettings.cs       # JWT konfiguráció
│   │   └── README.md            # Projekt specifikus README
│
├── FastEndpoints/                # FastEndpoints projekt
│   ├── MinimalAPIDemo.sln
│   ├── MinimalAPIDemo/
│   │   ├── Program.cs
│   │   ├── Endpoints/           # Endpoint osztályok
│   │   └── README.md
│
├── TodoApiController/            # Controller-based API
│   ├── TodoApiController.sln
│   ├── TodoApiController/
│   │   ├── Program.cs
│   │   ├── Controllers/         # API Controllers
│   │   ├── Model/              # Data modellek
│   │   ├── Validators/         # FluentValidation
│   │   └── README.md
│
├── Authentication/               # Haladó Auth projekt
│   ├── TodoApiController.sln
│   ├── TodoApiController/
│   │   ├── Program.cs
│   │   ├── Controllers/
│   │   ├── Model/
│   │   ├── Options/            # Options pattern
│   │   └── README.md
│
└── RestApiHasznalat/            # API Client projekt
    ├── RestApiHasznalat.sln
    ├── RestApiHasznalat/
    │   ├── Program.cs
    │   ├── ChuckApiHandler.cs  # HTTP Client logic
    │   └── README.md
```

---

## 🧪 Tesztelési Tippek

### cURL Példák

**Login és Token beszerzés:**
```bash
curl -X POST "http://localhost:5091/login?user=admin&password=admin"
```

**Védett endpoint hívás:**
```bash
curl -H "Authorization: Bearer YOUR_TOKEN_HERE" \
     http://localhost:5091/id_alapjan/3
```

### Postman Collection

Minden projekthez érdemes létrehozni egy Postman collection-t:
1. Import a .http fájlokat (ha vannak)
2. Environment változók: `baseUrl`, `token`
3. Pre-request script a token frissítéshez

---

## 🎓 További Tanulási Források

- [ASP.NET Core dokumentáció](https://docs.microsoft.com/aspnet/core)
- [JWT.io](https://jwt.io) - JWT debugger
- [FastEndpoints dokumentáció](https://fast-endpoints.com/)
- [FluentValidation dokumentáció](https://docs.fluentvalidation.net/)

---

## 📌 Megjegyzések

- Minden projekt **fejlesztési célra** készült, production használathoz további biztonsági intézkedések szükségesek
- A jelszavak tárolása **NEM** biztonságos (plain text vagy egyszerű összehasonlítás)
- Az in-memory data store **nem perzisztens**, újraindításkor elveszik az adat
- A JWT secret key-k konfigurációs fájlokban vannak (production-ben environment variable-ből kellene)

---

## 🤝 Contributing

Ez egy tanulási projekt. Nyugodtan kísérletezhetsz, módosíthatsz, és tanulhatsz belőle!

---

## 📄 Licenc

Oktatási célú projekt - szabadon használható és módosítható.

---

**Készítette:** Tanulási céllal 🚀
**Utolsó frissítés:** 2025. November 9.
