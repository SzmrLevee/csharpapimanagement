# C# ASP.NET Core API Tanulási Projektek

Ez a repository C# ASP.NET Core API fejlesztés tanulására készült projektek gyűjteménye. Minden projekt egy-egy fontos koncepciót mutat be a modern web API fejlesztésben.

## 📚 Projektek Áttekintése

### 1. **MinimalAPIDemo** - Alapvető Minimal API
**Tanulási Fókusz:** Minimal API alapok, JWT autentikáció, middleware pipeline

**Mit tanulhatsz meg:**
- Minimal API endpoint-ok létrehozása (`MapGet`, `MapPost`)
- JWT (JSON Web Token) alapú autentikáció implementálása
- Token generálás és validálás
- Authorization middleware használata
- Swagger/OpenAPI dokumentáció
- Route paraméterek kezelése
- Claims-based autorizáció

**Főbb technológiák:**
- ASP.NET Core Minimal API
- JWT Bearer Authentication
- Microsoft.IdentityModel.Tokens
- Swagger UI

**Port:** `http://localhost:5091`

---

### 2. **FastEndpoints** - FastEndpoints Framework
**Tanulási Fókusz:** FastEndpoints library, endpoint szervezés, FluentValidation

**Mit tanulhatsz meg:**
- FastEndpoints framework használata
- Endpoint osztályok létrehozása és szervezése
- Dependency Injection endpoint-okban
- FluentValidation integráció
- Strukturált hibakezelés
- Type-safe endpoint konfigurálás
- Constructor injection endpoint-okban

**Főbb technológiák:**
- FastEndpoints 5.30.0
- FluentValidation
- JWT Authentication
- Options pattern

**Különbségek a Minimal API-hoz képest:**
- Endpoint-ok osztály alapúak, nem inline lambda-k
- Beépített validáció támogatás
- Jobb kód szervezés nagyobb projektekhez
- Type-safe request/response handling

---

### 3. **TodoApiController** - Controller-based API
**Tanulási Fókusz:** Hagyományos Controller alapú API, MVC pattern

**Mit tanulhatsz meg:**
- Controller osztályok és Action metódusok
- RESTful API tervezés (GET, POST, PUT, DELETE)
- In-memory data store implementálás
- FluentValidation a controller context-ben
- LoginController - autentikáció
- TodoController - CRUD műveletek
- UserController - felhasználó kezelés
- Model validáció

**Főbb technológiák:**
- ASP.NET Core Controllers
- FluentValidation
- JWT Authentication
- Custom data store interface

**API Endpoint-ok:**
- `/api/login` - Bejelentkezés
- `/api/todo` - Todo CRUD műveletek
- `/api/user` - Felhasználó kezelés

---

### 4. **Authentication** - Haladó Autentikáció
**Tanulási Fókusz:** Autentikáció és autorizáció részletesen

**Mit tanulhatsz meg:**
- JWT Options pattern (`JwtOptions` osztály)
- Secure token generálás
- User management implementálás
- Interface-based architecture (`IUser`, `IDataStore`)
- Dependency Injection advanced patterns
- Claims és Roles kezelés
- Password handling (egyszerűsített, tanulási célra)

**Főbb technológiák:**
- Options Pattern
- Interface Segregation Principle
- Custom authentication logic
- In-memory user store

**Architektúra jellemzők:**
- Szeparált modellek (LoginUser, User, TodoItem)
- Interface-based design
- Validator osztályok külön fájlokban

---

### 5. **RestApiHasznalat** - REST API Kliens
**Tanulási Fókusz:** Külső API-k fogyasztása C#-ban

**Mit tanulhatsz meg:**
- HttpClient használata
- REST API hívások (GET)
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

## 📖 Tanulási Útvonal Javaslat

### Kezdő Szint
1. **MinimalAPIDemo** - Kezdd itt! Alapvető API koncepciók
2. **RestApiHasznalat** - Tanuld meg, hogyan használj API-kat

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
