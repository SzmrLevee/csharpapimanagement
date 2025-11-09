# MinimalAPIDemo - Alapvető Minimal API JWT Autentikációval

## 📋 Projekt Leírás

Ez a projekt bemutatja az ASP.NET Core **Minimal API** alapjait JWT (JSON Web Token) autentikációval. 
A Minimal API egy egyszerűbb, lambda-alapú megközelítés endpoint-ok definiálására, kevesebb boilerplate kóddal.

**Port:** `http://localhost:5091`

---

## 🎯 Mit tanulhatsz meg ebből a projektből?

1. **Minimal API alapok** - Endpoint-ok definiálása `MapGet` és `MapPost` segítségével
2. **JWT autentikáció** - Token generálás és validálás
3. **Bearer Token használat** - Authorization header kezelés
4. **Claims-based autorizáció** - Felhasználó azonosítás JWT-vel
5. **Swagger integráció** - Automatikus API dokumentáció
6. **Options Pattern** - Konfiguráció kezelés (`JwtSettings`)

---

## 🏗️ Projekt Struktúra

```
MinimalAPIDemo/
├── Program.cs              # Fő fájl - összes konfiguráció és endpoint itt van
├── JwtSettings.cs          # JWT konfiguráció osztály
├── JwtSettingsValidator.cs # FluentValidation a JWT config ellenőrzésére
├── appsettings.json        # Konfiguráció (JWT secret, issuer, audience)
└── appsettings.Development.json
```

---

## 🔑 Fő Komponensek

### 1. Program.cs - Alkalmazás Belépési Pont

#### Konfiguráció Betöltése

```csharp
var builder = WebApplication.CreateBuilder(args);
var jwt = builder.Configuration.GetSection("jwt").Get<JwtSetting>()!;
```

**Mit csinál?**
- `builder.Configuration` - elérjük az `appsettings.json` tartalmát
- `GetSection("jwt")` - a "jwt" szekciót vesszük ki
- `Get<JwtSetting>()` - deszerializáljuk a `JwtSetting` objektumba
- `!` - null-forgiving operator (biztosak vagyunk benne, hogy nem null)

---

#### JWT Authentication Beállítása

```csharp
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new()
        {
            ValidateIssuer = true,              // Ellenőrzi a token kibocsátóját
            ValidateAudience = true,            // Ellenőrzi a címzettet
            ValidateLifetime = true,            // Ellenőrzi a lejárati időt
            ValidateIssuerSigningKey = true,    // Ellenőrzi az aláírást
            ValidIssuer = jwt.Issuer,
            ValidAudience = jwt.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwt.Key))
        };
    });
```

**Fontos fogalmak:**
- **Issuer** - Ki hozta létre a token-t? (pl. "MinimalAPIDemo")
- **Audience** - Kinek szól a token? (pl. "MinimalAPIDemo.Clients")
- **IssuerSigningKey** - Titkos kulcs a token aláírásához/ellenőrzéséhez
- **Symmetric Key** - Ugyanaz a kulcs az aláíráshoz és ellenőrzéshez

---

### 2. API Endpoint-ok

#### GET /weatherforecast - Publikus Időjárás Endpoint

```csharp
app.MapGet("/weatherforecast", () =>
{
    var forecast = Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast(
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast")
.WithOpenApi();
```

**Mit csinál?**
- Lambda kifejezés `() => { ... }` - ez az endpoint logikája
- `Enumerable.Range(1, 5)` - generál 5 elemet (1-től 5-ig)
- `Select` - mindegyiket átalakítja `WeatherForecast` objektummá
- `.WithName()` - endpoint elnevezése
- `.WithOpenApi()` - Swagger dokumentációhoz

**Nincs autentikáció** - bárki hívhatja!

---

#### POST /login - Bejelentkezés és Token Generálás

```csharp
app.MapPost("/login", (string user, string password) =>
{
    // Egyszerű validáció - NEM production ready!
    if (user != password)
    {
        return Results.Forbid();
    }
    
    // JWT Token generálása
    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Key));
    var cred = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
    
    // Claims - a token tartalmazza ezeket az információkat
    var claims = new[]
    {
        new Claim(ClaimTypes.NameIdentifier, user),  // Felhasználó azonosító
        new Claim(ClaimTypes.Role, "Finance"),       // Szerepkör
    };
    
    // Token összeállítása
    var token = new JwtSecurityToken(
        issuer: jwt.Issuer,
        audience: jwt.Audience,
        claims: claims,
        expires: DateTime.Now.AddMinutes(15),        // 15 perc lejárat
        signingCredentials: cred
    );
    
    // Token string formátumba konvertálása
    var tokenStr = new JwtSecurityTokenHandler().WriteToken(token);
    return Results.Accepted(tokenStr);
})
.WithOpenApi()
.AllowAnonymous();
```

**Fontos fogalmak:**
- **Claims** - A token-ben tárolt kulcs-érték párok (ki a felhasználó, milyen szerepe van)
- **SigningCredentials** - Aláírási algoritmus (HMAC SHA256)
- **Expires** - Token érvényességi ideje
- **JwtSecurityTokenHandler** - Token generálás/feldolgozás

**⚠️ FIGYELEM:** Ez csak oktatási célú! Production-ben:
- Ne legyen `user == password`
- Hash-eld a jelszót (bcrypt, Argon2)
- Rate limiting
- Account lockout

---

#### GET /id_alapjan/{id} - Védett Endpoint

```csharp
app.MapGet("/id_alapjan/{id:int}", (HttpContext context, int id) =>
{
    // Claims kiolvasása a bejelentkezett felhasználóról
    var identify = context.User.Identity as ClaimsIdentity;
    var user = identify!.Claims
        .First(x => x.Type == ClaimTypes.NameIdentifier)
        .Value;
    
    // Validáció
    if(id < 0 || id >= summaries.Length)
    {
        return Results.NotFound($"{user}: ID not found: {id}");
    }
    
    return Results.Ok(user + summaries[id]);
})
.WithOpenApi()
.WithName("IDBased")
.RequireAuthorization();  // 🔒 CSAK JWT token-nel hívható!
```

**Mit csinál?**
- `{id:int}` - Route constraint, csak integer id-t fogad el
- `HttpContext` - ASP.NET Core context objektum
- `context.User.Identity` - A bejelentkezett felhasználó
- `.RequireAuthorization()` - **Kötelező JWT token!**

**Hogyan működik?**
1. Kliens elküldi a kérést Authorization header-rel: `Bearer <token>`
2. Middleware validálja a token-t
3. Ha valid, feltölti a `context.User` objektumot
4. Az endpoint hozzáfér a Claims-ekhez

---

### 3. JwtSettings.cs - Konfiguráció Osztály

```csharp
public class JwtSettings
{
    public class JwtSetting
    {
        public string Issuer { get; set; } = "";
        public string Audience { get; set; } = "";
        public string Key { get; set; } = "";
    }
}
```

**Mit csinál?**
- Egyszerű POCO (Plain Old CLR Object) osztály
- Az `appsettings.json` "jwt" szekciója deserializálódik ebbe

**appsettings.json példa:**
```json
{
  "jwt": {
    "Issuer": "MinimalAPIDemo",
    "Audience": "MinimalAPIDemo.Clients",
    "Key": "SuperSecretKey123456789012345678901234567890"
  }
}
```

⚠️ A Key legalább **256 bit** (32 karakter) legyen HMAC SHA256-hoz!

---

## 🚀 Hogyan Használd?

### 1. Alkalmazás Indítása

```bash
cd MinimalAPIDemo/MinimalAPIDemo
dotnet run
```

A console-ban látni fogod:
```
Now listening on: http://localhost:5091
```

---

### 2. Swagger UI Megnyitása

Böngészőben:
```
http://localhost:5091/swagger
```

Itt látod az összes endpoint-ot és tesztelheted őket!

---

### 3. Login - Token Beszerzése

**cURL:**
```bash
curl -X POST "http://localhost:5091/login?user=testuser&password=testuser"
```

**Válasz:**
```
eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJuYW1laWQiOiJ0ZXN0dXNlciIsInJvbGUiOiJGaW5hbmNlIiwibmJmIjoxNjk5...
```

Ez a JWT token! Másold ki és használd a következő lépésben.

---

### 4. Védett Endpoint Hívása

**cURL:**
```bash
curl -H "Authorization: Bearer YOUR_TOKEN_HERE" \
     http://localhost:5091/id_alapjan/3
```

**Válasz:**
```
testuserCool
```

**Hibás token esetén:**
```
HTTP 401 Unauthorized
```

---

## 🔐 JWT Token Felépítése

Egy JWT token 3 részből áll, pontokkal elválasztva:

```
HEADER.PAYLOAD.SIGNATURE
```

### Header
```json
{
  "alg": "HS256",
  "typ": "JWT"
}
```

### Payload (Claims)
```json
{
  "nameid": "testuser",
  "role": "Finance",
  "nbf": 1699...,
  "exp": 1699...,
  "iss": "MinimalAPIDemo",
  "aud": "MinimalAPIDemo.Clients"
}
```

### Signature
```
HMACSHA256(
  base64UrlEncode(header) + "." + base64UrlEncode(payload),
  secret
)
```

**Debuggolás:** [jwt.io](https://jwt.io) - Illeszd be a token-t és látod a tartalmát!

---

## 📊 Middleware Pipeline

A kérések így haladnak át a rendszeren:

```
Request
  ↓
[Routing] → Melyik endpoint?
  ↓
[Authentication] → Van token? Valid?
  ↓
[Authorization] → Van joga meghívni?
  ↓
[Endpoint Logic] → Végrehajtás
  ↓
Response
```

**Program.cs-ben:**
```csharp
var app = builder.Build();

app.UseSwagger();        // Swagger middleware
app.UseSwaggerUI();      // Swagger UI
// app.UseAuthentication(); - Implicit a MapGet/Post hívásokkal
// app.UseAuthorization();  - Implicit a RequireAuthorization()-nel

app.MapGet(...);         // Endpoint definíciók
app.Run();               // Alkalmazás indítása
```

---

## 🧪 Tesztelési Példák

### Postman Collection

**1. Login Request:**
- Method: `POST`
- URL: `http://localhost:5091/login?user=admin&password=admin`
- Save response → token

**2. Protected Request:**
- Method: `GET`
- URL: `http://localhost:5091/id_alapjan/3`
- Headers: `Authorization: Bearer {{token}}`

---

## 🎓 Következő Lépések

Miután megértetted ezt a projektet:
1. ✅ Próbálj hozzáadni új endpoint-okat
2. ✅ Változtasd meg a token lejárati időt
3. ✅ Add hozzá több Role-t (Admin, User, stb.)
4. ✅ Nézd meg a **TodoApiController** projektet - Controller-based megközelítés
5. ✅ Nézd meg a **FastEndpoints** projektet - Strukturáltabb endpoint kezelés

---

## ⚠️ Biztonsági Megjegyzések

Ez egy **oktatási projekt**! Production használathoz szükséges:
- ✅ Titkos kulcs environment variable-ből
- ✅ HTTPS kötelező
- ✅ Password hashing (bcrypt, Argon2)
- ✅ Refresh token implementálás
- ✅ Token revocation
- ✅ Rate limiting
- ✅ Input validáció
- ✅ Error logging

---

**Készítve tanulási célból** 🚀
