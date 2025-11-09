// ASP.NET Core JWT Authentication importok
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using static MinimalAPIDemo.JwtSettings;

// Szükséges NuGet package-ek:
// - Microsoft.AspNetCore.Authentication.JwtBearer
// - Microsoft.IdentityModel.Tokens
// - System.IdentityModel.Tokens.Jwt

// WebApplication Builder létrehozása - ez a belépési pont
var builder = WebApplication.CreateBuilder(args);

// JWT konfiguráció betöltése az appsettings.json-ből
// A "jwt" szekció tartalmazza: Issuer, Audience, Key
// JWT konfiguráció betöltése az appsettings.json-ből
// A "jwt" szekció tartalmazza: Issuer, Audience, Key
var jwt = builder.Configuration.GetSection("jwt").Get<JwtSetting>()!;

// === SZOLGÁLTATÁSOK REGISZTRÁLÁSA (Dependency Injection) ===

// API Explorer - Swagger dokumentációhoz szükséges
builder.Services.AddEndpointsApiExplorer();

// Swagger - Automatikus API dokumentáció generálás
builder.Services.AddSwaggerGen();

// Autorizáció szolgáltatás - Jogosultság kezelés
builder.Services.AddAuthorization();

// JWT Bearer Autentikáció beállítása
// JWT Bearer Autentikáció beállítása
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        // Token validálási paraméterek
        options.TokenValidationParameters = new()
        {
            // Issuer validálás - ellenőrzi, hogy ki hozta létre a token-t
            ValidateIssuer = true,
            
            // Audience validálás - ellenőrzi, hogy kinek szól a token
            ValidateAudience = true,
            
            // Lifetime validálás - ellenőrzi, hogy nem járt-e le
            ValidateLifetime = true,
            
            // Aláírás validálás - ellenőrzi, hogy nem módosították-e
            ValidateIssuerSigningKey = true,
            
            // Elvárt értékek
            ValidIssuer = jwt.Issuer,                                                   // pl. "MinimalAPIDemo"
            ValidAudience = jwt.Audience,                                               // pl. "MinimalAPIDemo.Clients"
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Key)), // Titkos kulcs
        };
    });

// === ALKALMAZÁS PIPELINE ÉPÍTÉSE ===

var app = builder.Build();

// === MIDDLEWARE KONFIGURÁCIÓ ===

// Development környezetben Swagger UI engedélyezése
// Production-ben ezt ki kell kapcsolni!
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();        // Swagger JSON endpoint
    app.UseSwaggerUI();      // Swagger UI felület
}

// Időjárás leírások tömbje - példa adatok
var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

// === API ENDPOINT-OK ===

// ✅ PUBLIKUS ENDPOINT - GET /weatherforecast
// Bárki hívhatja, nincs szükség autentikációra
// ✅ PUBLIKUS ENDPOINT - GET /weatherforecast
// Bárki hívhatja, nincs szükség autentikációra
app.MapGet("/weatherforecast", () =>
{
    // 5 napra előre generálunk random időjárást
    var forecast = Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),  // Dátum: holnaptól 5 napra
            Random.Shared.Next(-20, 55),                         // Random hőmérséklet -20 és 55 között
            summaries[Random.Shared.Next(summaries.Length)]      // Random leírás a tömbből
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast")  // Swagger-ben ez lesz a név
.WithOpenApi();                  // OpenAPI dokumentációhoz hozzáadás

// 🔒 VÉDETT ENDPOINT - GET /id_alapjan/{id}
// CSAK JWT token-nel hívható! RequireAuthorization() miatt
app.MapGet("/id_alapjan/{id:int}", (HttpContext context, int id) =>
{
    // A bejelentkezett felhasználó adatai a JWT token-ből
    var identify = context.User.Identity as ClaimsIdentity;
    
    // NameIdentifier claim kiolvasása - ez a felhasználónév
    var user = identify!.Claims.First(x => x.Type == ClaimTypes.NameIdentifier).Value;
    
    // Validálás: ID nem lehet negatív vagy nagyobb mint a tömb mérete
    if(id < 0 || id >= summaries.Length)
    {
        return Results.NotFound($"{user}: ID not found: {id}");
    }
    
    // Sikeres válasz: felhasználónév + időjárás leírás
    return Results.Ok(user + summaries[id]);
})
    .WithOpenApi()
    .WithName("IDBased")
    .RequireAuthorization();  // 🔒 KÖTELEZŐ JWT TOKEN!

// 🔓 LOGIN ENDPOINT - POST /login
// Token generálás - bárki hívhatja (AllowAnonymous)
app.MapPost("/login", (string user, string password) =>
{
    // ⚠️ EGYSZERŰ VALIDÁCIÓ - NEM PRODUCTION READY!
    // Production-ben: bcrypt/Argon2 hashing, rate limiting, account lockout
    if (user != password)
    {
        return Results.Forbid();  // 403 Forbidden
    }
    
    // JWT Token generálása
    
    // 1. Aláírási kulcs létrehozása (Symmetric - ugyanaz íráshoz és olvasáshoz)
    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Key));
    
    // 2. Signing credentials - HMAC SHA256 algoritmus
    var cred = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
    
    // 3. Claims - token tartalmazza ezeket az adatokat
    var claims = new[]
    {
        new Claim(ClaimTypes.NameIdentifier, user),  // Ki a felhasználó?
        new Claim(ClaimTypes.Role, "Finance"),       // Milyen szerepköre van?
    };
    
    // 4. JWT Token összeállítása
    var token = new JwtSecurityToken(
        issuer: jwt.Issuer,              // Ki hozta létre? (pl. "MinimalAPIDemo")
        audience: jwt.Audience,          // Kinek szól? (pl. "MinimalAPIDemo.Clients")
        claims: claims,                  // Benne levő adatok
        expires: DateTime.Now.AddMinutes(15),  // Lejárat: 15 perc múlva
        signingCredentials: cred         // Aláírás
    );
    
    // 5. Token string formátumba konvertálása
    var tokenStr = new JwtSecurityTokenHandler().WriteToken(token);
    
    // 6. Token visszaadása a kliensnek
    return Results.Accepted(tokenStr);
})
    .WithOpenApi()
    .AllowAnonymous();  // Bárki hívhatja, nincs autentikáció

// POST /uj_beallitas - JWT Settings módosítás (példa endpoint)
app.MapPost("/uj_beallitas", (JwtSetting jwt) => 
{ 
    return Results.Accepted($"{jwt.Issuer}"); 
});

// POST /feltoltes - Fájl feltöltés Base64-ben (példa endpoint)
app.MapPost("/feltoltes", async (HttpRequest req) =>
{
    // Request body beolvasása StreamReader-rel
    using StreamReader reader = new StreamReader(req.Body);
    
    // Base64 string dekódolása byte array-vé
    var data = Convert.FromBase64String(await reader.ReadToEndAsync());
    
    // Feltöltött adat méretének visszaadása
    return Results.Accepted(data?.Length.ToString());
});

// === ALKALMAZÁS INDÍTÁSA ===
app.Run();

// === MODELLEK ===

// WeatherForecast record - Időjárás előrejelzés modell
// Record = immutable (nem módosítható) osztály, value equality
internal record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    // Computed property - Fahrenheit konverzió
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
