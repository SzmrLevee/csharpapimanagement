// TodoApiController - Controller-based ASP.NET Core Web API
// RESTful Todo lista kezelés FluentValidation-nel

using FluentValidation;
using TodoApiController.Model;
using TodoApiController.Validators;

// WebApplication Builder létrehozása
var builder = WebApplication.CreateBuilder(args);

// === SZOLGÁLTATÁSOK REGISZTRÁLÁSA (Dependency Injection) ===

// Controller-ök regisztrálása
// Ez lehetővé teszi a [ApiController] osztályok használatát
builder.Services.AddControllers();

// ⭐ FONTOS! IDataStore implementáció regisztrálása SINGLETON-ként
// Singleton = egy példány az egész alkalmazás életciklusa alatt
// Ez az in-memory adattár - újraindításkor elvesznek az adatok!
builder.Services.AddSingleton<IDataStore, DataStore>();

// ⭐ FluentValidation regisztrálása
// NuGet package szükséges: FluentValidation.DependencyInjectionExtensions
// Automatikusan megtalálja az összes AbstractValidator<T> osztályt az assembly-ben
builder.Services.AddValidatorsFromAssemblyContaining<TodoItemValidator>();

// Swagger/OpenAPI dokumentáció szolgáltatások
// Learn more: https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// === ALKALMAZÁS PIPELINE ÉPÍTÉSE ===

var app = builder.Build();

// === MIDDLEWARE KONFIGURÁCIÓ ===

// Development környezetben Swagger UI engedélyezése
// Production-ben ez ki van kapcsolva!
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();        // Swagger JSON endpoint (/swagger/v1/swagger.json)
    app.UseSwaggerUI();      // Swagger UI (/swagger)
}

// HTTPS redirection (jelenleg kikommentezve - HTTP-n fut)
// Production-ben MINDIG kapcsold be!
//app.UseHttpsRedirection();

// Authorization middleware
// Jogosultság ellenőrzés (ha később JWT-t használunk)
app.UseAuthorization();

// Controller route-ok mapping-je
// Megkeresi az összes [ApiController] osztályt és route-olja őket
app.MapControllers();

// Alkalmazás indítása
app.Run();
