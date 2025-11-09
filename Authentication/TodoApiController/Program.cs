using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using TodoApiController.Model;
using TodoApiController.Options;
using TodoApiController.Validators;

// Szükséges NuGet csomagok:
// - Microsoft.AspNetCore.Authentication.JwtBearer - JWT token validálás
// - Microsoft.IdentityModel.Tokens - Token signing és validálás
// - System.IdentityModel.Tokens.Jwt - JWT token generálás

// WebApplicationBuilder létrehozása - ez építi fel az alkalmazás konfigurációját
var builder = WebApplication.CreateBuilder(args);

// ========== JWT AUTENTIKÁCIÓ BEÁLLÍTÁSA ==========

// 1. JWT konfigurációk betöltése az appsettings.Development.json fájlból
// A Get<JwtOptions>() deserialize-olja a "Jwt" szekciót JwtOptions objektummá
var jwt = builder.Configuration.GetSection("Jwt").Get<JwtOptions>()!;

// 2. JwtOptions regisztrálása a DI konténerbe Options Pattern szerint
// Így bármelyik osztály kérheti IOptions<JwtOptions> formában
builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection("Jwt"));

// 3. JWT Authentication middleware beállítása
// JwtBearerDefaults.AuthenticationScheme = "Bearer" séma használata
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        // Token validálási paraméterek megadása
        options.TokenValidationParameters = new()
        {
            ValidateIssuer = true,            // Ellenőrzi, hogy a token a várt kibocsátótól jön-e
            ValidateAudience = true,          // Ellenőrzi, hogy a token a megfelelő célközönségnek szól-e
            ValidateLifetime = true,          // Ellenőrzi, hogy a token nem járt-e le
            ValidateIssuerSigningKey = true,  // Ellenőrzi a token digitális aláírását
            ValidIssuer = jwt.Issuer,         // Várt kibocsátó (Issuer) értéke
            ValidAudience = jwt.Audience,     // Várt célközönség (Audience) értéke
            // A token aláírásához használt titkos kulcs (HMAC-SHA256 algoritmussal)
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Key)),
        };
    });

// ========== CONTROLLER ÉS DEPENDENCY INJECTION BEÁLLÍTÁSOK ==========

// Controller-ek aktiválása (LoginController, UserController, TodoController)
builder.Services.AddControllers();

// FONTOS: DataStore regisztrálása Singleton-ként
// Singleton = egyetlen példány az app teljes életciklusa alatt
// Ez biztosítja, hogy minden kérés ugyanazt az in-memory adattárolót használja
builder.Services.AddSingleton<IDataStore, DataStore>();

// FluentValidation validátorok automatikus regisztrálása
// NuGet: FluentValidation.DependencyInjectionExtensions csomag szükséges!
// Megkeresi az assembly-ben az összes IValidator<T> implementációt és regisztrálja
builder.Services.AddValidatorsFromAssemblyContaining<TodoItemValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<UserValidator>();

// ========== SWAGGER/OPENAPI DOKUMENTÁCIÓ ==========

// Swagger endpoint felderítés (API végpontok automatikus detektálása)
builder.Services.AddEndpointsApiExplorer();

// Swagger UI generálása (interaktív API dokumentáció böngészőben)
// Swagger UI generálása (interaktív API dokumentáció böngészőben)
builder.Services.AddSwaggerGen();

// ========== ALKALMAZÁS ÉPÍTÉSE ==========

// WebApplication példány létrehozása a beállított service-ekkel
var app = builder.Build();

// ========== MIDDLEWARE PIPELINE KONFIGURÁLÁSA ==========

// Csak Development környezetben engedélyezzük a Swagger UI-t
// (production-ben ne legyen elérhető az API dokumentáció)
if (app.Environment.IsDevelopment())
{
    // Swagger JSON endpoint aktiválása (/swagger/v1/swagger.json)
    app.UseSwagger();
    
    // Swagger UI aktiválása (interaktív dokumentáció a böngészőben)
    // Elérhető: http://localhost:5154/swagger
    app.UseSwaggerUI();
}

// HTTPS átirányítás kikapcsolva (teszteléshez HTTP is elég)
// Éles környezetben engedélyezd: app.UseHttpsRedirection();
//app.UseHttpsRedirection();

// FONTOS: A middleware-ek sorrendje kritikus!
// 1. UseAuthentication - Azonosítja a felhasználót a JWT token alapján
//    Ez olvassa be a token-t, validálja, és kitölti a HttpContext.User-t
app.UseAuthentication();

// 2. UseAuthorization - Ellenőrzi a felhasználó jogosultságait
//    Ez nézi meg az [Authorize] attribútumokat és a role-okat
//    MINDIG az Authentication UTÁN kell jönnie!
app.UseAuthorization();

// Controller endpoint-ok regisztrálása (LoginController, UserController, TodoController)
// Ez teszi elérhetővé az /api/login, /api/user, /api/todo végpontokat
app.MapControllers();

// Alkalmazás indítása és kérések fogadása
// Blokkoló hívás - az app futni fog, amíg le nem állítjuk (Ctrl+C)
app.Run();
