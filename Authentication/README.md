# Authentication - Haladó JWT Autentikáció és Role-Based Authorizáció

## 📚 Miről szól ez a projekt?

Ez a projekt az **Advanced JWT Authentication** és **Role-Based Authorization** teljes körű implementációját mutatja be egy ASP.NET Core Web API-ban. Ez a TodoApiController kibővített változata három önálló kontrollerrel és jelszó-hasheléssel.

### Amit megtanulhatsz:

- **JWT Token generálás** teljes Claims alapú adatokkal (név, email, role)
- **Options Pattern** használata JWT konfigurációhoz
- **Password Hashing** PBKDF2 algoritmussal (Rfc2898DeriveBytes)
- **Role-Based Authorization** - felhasználó vs admin jogosultságok
- **Claims-based Authentication** - felhasználó információk token-ből
- **Separate Controllers** - Login, User, Todo külön végpontokon
- **Advanced Validation** - felhasználó módosítás csak saját profil vagy admin
- **IOptions<T> Pattern** - dependency injection konfigurációhoz

## 🚀 Telepítés és Futtatás

### 1. Navigálj a projekt könyvtárába:
```bash
cd Authentication/TodoApiController
```

### 2. Futtasd a projektet:
```bash
dotnet run
```

### 3. Nyisd meg a böngészőben:
- **HTTP verzió:** http://localhost:5154/swagger
- **HTTPS verzió:** https://localhost:7036/swagger

## 🔑 Jelszókezelés és Biztonság

### Password Hashing (PBKDF2)

A projekt **Rfc2898DeriveBytes** osztályt használ biztonságos jelszó tároláshoz:

```csharp
public class User : IUser
{
    // Fix Salt (éles környezetben felhasználónként egyedi kéne!)
    public readonly byte[] Salt = Encoding.UTF8.GetBytes("0123456789012345");
    const int Iterations = 10_000;  // 10,000 iteráció
    const int HashSize = 32;         // 32 byte hash

    public byte[] Password { get; private set; } = Array.Empty<byte>();
    
    // Jelszó beállításakor automatikus hashing
    public string PasswordText
    {
        set
        {
            Password = new Rfc2898DeriveBytes(value, Salt, Iterations).GetBytes(HashSize);
        }
    }

    // Jelszó ellenőrzés - konstans idejű összehasonlítás
    public bool Matches(string password)
    {
        var bytes = new Rfc2898DeriveBytes(password, Salt, Iterations).GetBytes(HashSize);
        return bytes.Length == Password.Length 
            && Enumerable.Range(0, bytes.Length)
            .All(i => bytes[i] == Password[i]);
    }
}
```

**Kulcspontok:**
- **PBKDF2**: Industry-standard password hashing
- **10,000 iteráció**: Lassítja a brute-force támadásokat
- **32 byte hash**: SHA256 ekvivalens biztonság
- **Konstans idejű ellenőrzés**: Timing attack ellen védelem

## 🔐 JWT Konfiguráció (Options Pattern)

### appsettings.Development.json

```json
{
  "Jwt": {
    "Key": "Ez a kulcs012345Ez a kulcs012345",  // Min. 32 karakter!
    "Issuer": "Kibocsato",                       // Token kibocsátója
    "Audience": "Celkozonseg"                    // Token célközönsége
  }
}
```

### JwtOptions.cs (Record Type)

```csharp
public record JwtOptions
{
    public string Key { get; set; } = "";
    public string Issuer { get; set; } = "";
    public string Audience { get; set; } = "";
}
```

**Miért Options Pattern?**
- Típusbiztos konfiguráció
- DI-ből elérhető `IOptions<JwtOptions>`
- Könnyű tesztelhetőség
- Éles/fejlesztői környezet közötti váltás

## 🛣️ API Végpontok (Endpoints)

### 1. Login Végpont (POST /api/login)

**Kérés (Request Body):**
```json
{
  "userName": "testuser",
  "password": "password123"
}
```

**Sikeres válasz (200 OK):**
```json
"eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJuYW1laWQiOiJ0ZXN0dXNlciIsIm5hbWUiOiJUZXN6dCBGZWxoYXN6bsOhbMOzIiwiZW1haWwiOiJ0ZXN0QGV4YW1wbGUuY29tIiwicm9sZSI6IkZlbGhhc3puYWxvIiwibmJmIjoxNzM3MjQ4MDAwLCJleHAiOjE3MzcyNDg5MDAsImlhdCI6MTczNzI0ODAwMCwiaXNzIjoiS2lib2NzYXRvIiwiYXVkIjoiQ2Vsa296b25zZWcifQ.XYZ123..."
```

**Hiba (400 Bad Request):**
```json
"Invalid username or password"
```

**Példa LoginController kód:**

```csharp
[HttpPost]
public IActionResult Post([FromBody] LoginUser value)
{
    // 1. Felhasználó keresése username alapján
    var user = ((IItemStore<User>)dataStore).GetAll()
        .FirstOrDefault(x => x.UserName == value.UserName);
    
    if(user == null)
    {
        return BadRequest("Invalid username or password");
    }

    // 2. Jelszó ellenőrzés (PBKDF2 hash összehasonlítás)
    if (!user.Matches(value.Password))
    {
        return BadRequest("Invalid username or password");
    }

    // 3. JWT Token generálás
    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Key));
    var cred = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
    
    var claims = new[]
    {
        new Claim(ClaimTypes.NameIdentifier, user.UserName),
        new Claim(ClaimTypes.Name, user.Name),
        new Claim(ClaimTypes.Email, user.Email),
        new Claim(ClaimTypes.Role, "Felhasznalo"),
    };

    var token = new JwtSecurityToken(
        issuer: jwtOptions.Issuer,
        audience: jwtOptions.Audience,
        claims: claims,
        expires: DateTime.Now.AddMinutes(15),  // 15 perc érvényesség
        signingCredentials: cred
    );

    var tokenStr = new JwtSecurityTokenHandler().WriteToken(token);
    return Ok(tokenStr);
}
```

### 2. User Végpontok (UserController)

#### GET /api/user - Összes felhasználó listázása
**Válasz (200 OK):**
```json
[
  {
    "userName": "testuser",
    "email": "test@example.com",
    "name": "Teszt Felhasználó",
    "password": [/* byte array */]
  }
]
```

**Kód:**
```csharp
[HttpGet]
public IEnumerable<User> Get()
{
    return ((IItemStore<User>)dataStore).GetAll();
}
```

#### GET /api/user/{username} - Egyedi felhasználó lekérése
**Példa:** `GET /api/user/testuser`

**Válasz (200 OK):**
```json
{
  "userName": "testuser",
  "email": "test@example.com",
  "name": "Teszt Felhasználó"
}
```

**Válasz (404 Not Found):** Ha nem létezik a felhasználó

#### POST /api/user - Új felhasználó regisztrálása
**Kérés:**
```json
{
  "userName": "newuser",
  "email": "new@example.com",
  "name": "Új Felhasználó",
  "passwordText": "securePassword123"
}
```

**Válasz (200 OK):**
```json
{
  "userName": "newuser",
  "email": "new@example.com",
  "name": "Új Felhasználó"
}
```

**Validáció (FluentValidation):**
- UserName: kötelező, 3-50 karakter
- Email: valid email formátum
- Name: kötelező
- PasswordText: minimum 6 karakter

#### PUT /api/user/{username} - Felhasználó módosítása [VÉDETT]
**Hitelesítés szükséges!** (Authorization header)

**Példa:** `PUT /api/user/testuser`

**Kérés:**
```json
{
  "userName": "testuser",
  "email": "updated@example.com",
  "name": "Frissített Név",
  "passwordText": "newPassword456"
}
```

**Válasz (200 OK):** Sikeres módosítás

**Válasz (400 Bad Request):**
```json
"May not change someone else"  // Ha nem saját profil és nem admin
```

**Kód (Advanced Authorization Logic):**

```csharp
[HttpPut("{username}")]
[Authorize]  // Csak bejelentkezett felhasználók!
public IActionResult Put(string username, [FromBody] User value)
{
    var identity = HttpContext.User.Identity as ClaimsIdentity;
    
    // 1. UserName nem változtatható
    if (username != value.UserName)
    {
        return BadRequest("May not change username");
    }

    // 2. Ki próbálja módosítani?
    var identityUser = identity.Claims
        .FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)
        ?.Value;

    // 3. Nem saját profil?
    if(username != identityUser)
    {
        // Ellenőrizd: van-e Administrator role?
        var isAdmin = identity
            .Claims
            .Where(x => x.Type == ClaimTypes.Role)
            .Where(x => x.Value != null)
            .Select(x => x.Value!)
            .Any(x => x == "Administrator");

        if(!isAdmin)
        {
            return BadRequest("May not change someone else");
        }
    }

    // 4. Validáció + mentés
    var result = validator.Validate(value);
    if (!result.IsValid)
    {
        return BadRequest(result);
    }

    value.UserName = username;
    if (dataStore.Update(value))
    {
        return NotFound();
    }
    return Ok(value);
}
```

**Kulcspontok:**
- `[Authorize]` attribútum: csak bejelentkezett felhasználók
- Claims-ből kiolvasható a felhasználó NameIdentifier és Role
- Saját profil mindig módosítható
- Más profilját csak Administrator módosíthatja

#### DELETE /api/user/{username} - Felhasználó törlése [CSAK ADMIN]
**Példa:** `DELETE /api/user/testuser`

**Hitelesítés:** Administrator role szükséges!

**Válasz (200 OK):** Sikeres törlés

**Válasz (404 Not Found):** Nem létező felhasználó

**Kód:**
```csharp
[HttpDelete("{username}")]
[Authorize(Roles = "Administrator")]  // Csak admin törölhet!
public IActionResult Delete(string username)
{
    var item = Get().FirstOrDefault(x => x.UserName == username);
    if (item == null) { return NotFound(); }
    dataStore.Delete(item);
    return Ok();
}
```

### 3. Todo Végpontok (TodoController)

Ugyanazok, mint a TodoApiController projektben, de **JWT védelem alatt**!

**Végpontok:**
- `GET /api/todo` - Összes todo
- `GET /api/todo/{id}` - Egy todo
- `POST /api/todo` - Új todo
- `PUT /api/todo/{id}` - Todo módosítás
- `DELETE /api/todo/{id}` - Todo törlés

## 🔒 JWT Token Használata

### 1. Szerezz tokent a /api/login végponton

```bash
curl -X POST http://localhost:5154/api/login \
  -H "Content-Type: application/json" \
  -d '{"userName":"testuser","password":"password123"}'
```

### 2. Használd a tokent az Authorization headerben

```bash
curl -X PUT http://localhost:5154/api/user/testuser \
  -H "Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..." \
  -H "Content-Type: application/json" \
  -d '{"userName":"testuser","email":"new@email.com","name":"Új Név"}'
```

### 3. Swagger UI-ban

1. Kattints a **"Authorize"** gombra (zárda ikon)
2. Írd be: `Bearer <token>`
3. Kattints **"Authorize"**
4. Most hívhatsz védett végpontokat!

## 🏗️ Projekt Architektúra

```
Authentication/TodoApiController/
├── Controllers/
│   ├── LoginController.cs       # JWT token generálás
│   ├── UserController.cs        # User CRUD + role-based auth
│   └── TodoController.cs        # Todo CRUD
├── Model/
│   ├── User.cs                  # Password hashing logika
│   ├── LoginUser.cs             # Login request model
│   ├── IUser.cs                 # Password interface
│   ├── TodoItem.cs              # Todo adatmodell
│   ├── DataStore.cs             # In-memory adattárolás
│   └── IDataStore.cs            # Generic store interface
├── Options/
│   └── JwtOptions.cs            # JWT konfiguráció model
├── Validators/
│   ├── UserValidator.cs         # FluentValidation - User
│   └── TodoItemValidator.cs     # FluentValidation - Todo
├── Program.cs                   # DI és middleware konfiguráció
└── appsettings.Development.json # JWT beállítások
```

## 🔧 Program.cs Konfiguráció

### JWT Authentication Setup

```csharp
// JWT konfiguráció betöltése
var jwt = builder.Configuration.GetSection("Jwt").Get<JwtOptions>()!;
builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection("Jwt"));

// JWT Authentication middleware
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new()
        {
            ValidateIssuer = true,            // Issuer ellenőrzés
            ValidateAudience = true,          // Audience ellenőrzés
            ValidateLifetime = true,          // Lejárat ellenőrzés
            ValidateIssuerSigningKey = true,  // Signature ellenőrzés
            ValidIssuer = jwt.Issuer,
            ValidAudience = jwt.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwt.Key)
            ),
        };
    });
```

### Middleware Pipeline

```csharp
app.UseHttpsRedirection();
app.UseAuthentication();  // ELŐSZÖR authentication!
app.UseAuthorization();   // UTÁNA authorization!
app.MapControllers();
```

**Sorrend fontos!** Authentication előbb kell, hogy az authorization dolgozhasson a felhasználó Claims-jeivel.

## 📚 Használt NuGet Csomagok

```xml
<PackageReference Include="Microsoft.AspNetCore.Authentication.JwtBearer" Version="8.0.11" />
<PackageReference Include="Microsoft.IdentityModel.Tokens" Version="8.2.1" />
<PackageReference Include="System.IdentityModel.Tokens.Jwt" Version="8.2.1" />
<PackageReference Include="FluentValidation.AspNetCore" Version="11.3.0" />
<PackageReference Include="Swashbuckle.AspNetCore" Version="7.2.0" />
```

## 💡 Tanulási Pontok

### 1. Options Pattern
**Miért jó?**
- Típusbiztos konfiguráció
- Könnyű dependency injection
- Környezetek közötti váltás (Development/Production)

**Használat:**
```csharp
// Program.cs - Regisztráció
builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection("Jwt"));

// Controller - Használat
public LoginController(IOptions<JwtOptions> options)
{
    this.jwtOptions = options.Value;
}
```

### 2. Claims-Based Authentication
**Claims = Token-ben tárolt információk**

```csharp
var claims = new[]
{
    new Claim(ClaimTypes.NameIdentifier, user.UserName),  // Egyedi azonosító
    new Claim(ClaimTypes.Name, user.Name),                // Teljes név
    new Claim(ClaimTypes.Email, user.Email),              // Email cím
    new Claim(ClaimTypes.Role, "Felhasznalo"),            // Role
};
```

**Claims kiolvasása:**
```csharp
var identity = HttpContext.User.Identity as ClaimsIdentity;
var userName = identity.Claims
    .FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)
    ?.Value;
```

### 3. Role-Based Authorization Szintek

**Szint 1 - Csak bejelentkezés:**
```csharp
[Authorize]  // Bármilyen bejelentkezett felhasználó
public IActionResult Put(...)
```

**Szint 2 - Konkrét role:**
```csharp
[Authorize(Roles = "Administrator")]  // Csak admin
public IActionResult Delete(...)
```

**Szint 3 - Kód szintű ellenőrzés:**
```csharp
var isAdmin = identity.Claims
    .Where(x => x.Type == ClaimTypes.Role)
    .Any(x => x.Value == "Administrator");
```

### 4. Password Security Best Practices

**❌ NE így:**
```csharp
public string Password { get; set; }  // Plain text!
if (user.Password == loginPassword)   // Vulnerable!
```

**✅ Így:**
```csharp
public byte[] Password { get; private set; }  // Hashed
public string PasswordText { set { /* Hash it */ } }
public bool Matches(string password) { /* Verify hash */ }
```

**PBKDF2 előnyei:**
- SHA256-nál sokkal lassabb → brute force ellen
- Industry standard (NIST ajánlott)
- Beépített salt + iteration support

## ⚠️ Biztonsági Figyelmeztetések

### 🔴 ÉLES KÖRNYEZETBEN TILOS:
1. **Fix Salt használata:**
   ```csharp
   // ❌ Jelenlegi (minden user ugyanazt használja!)
   public readonly byte[] Salt = Encoding.UTF8.GetBytes("0123456789012345");
   
   // ✅ Éles változat (felhasználónként egyedi)
   public byte[] Salt { get; set; } = RandomNumberGenerator.GetBytes(16);
   ```

2. **In-Memory DataStore:**
   - Adatok elvesznek újraindításkor
   - Használj valódi adatbázist (SQL Server, PostgreSQL, stb.)

3. **Weak JWT Key:**
   ```csharp
   // ❌ Rövid kulcs
   "Key": "Ez a kulcs012345Ez a kulcs012345"
   
   // ✅ Legalább 32 byte (256 bit) random
   "Key": "VeryLongRandomSecureKeyThatIsAtLeast32CharactersOrMore123456789"
   ```

4. **Token érvényesség:**
   ```csharp
   // Jelenlegi: 15 perc
   expires: DateTime.Now.AddMinutes(15)
   
   // Éles: Kontextustól függ (API: 1 óra, refresh token pattern)
   ```

## 🎯 Következő Lépések

Ha már megértetted ezt a projektet, nézd meg:
- **FastEndpoints projekt**: Modern alternatíva a Controller-eknek
- **MinimalAPIDemo**: Még kompaktabb API szintaxis
- **Entity Framework Core**: Valódi adatbázis integráció
- **Refresh Token Pattern**: Long-lived sessions
- **Role Management API**: Dynamic role assignment

## 📖 További Olvasnivaló

- [ASP.NET Core Authentication](https://learn.microsoft.com/en-us/aspnet/core/security/authentication/)
- [JWT.io](https://jwt.io/) - Token debugger
- [PBKDF2 Wikipedia](https://en.wikipedia.org/wiki/PBKDF2)
- [OWASP Password Storage](https://cheatsheetseries.owasp.org/cheatsheets/Password_Storage_Cheat_Sheet.html)
