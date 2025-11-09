# RestApiHasznalat - REST API Kliens C#-ban

## 📋 Projekt Leírás

Ez a projekt bemutatja, hogyan **fogyassz külső REST API-kat C#-ban**. 
Egy egyszerű konzol alkalmazás, amely a Chuck Norris API-ból tölt le vicceket HTTP GET kéréssel.

**Típus:** Console Application (.NET 8.0)

---

## 🎯 Mit tanulhatsz meg ebből a projektből?

1. **HttpClient használata** - HTTP kérések küldése C#-ban
2. **Async/Await pattern** - Aszinkron programozás
3. **JSON deszerializáció** - API válasz feldolgozása
4. **CancellationToken** - Aszinkron műveletek megszakítása
5. **Record típus** - Immutable data model
6. **Error handling** - Try-catch és null handling

---

## 🏗️ Projekt Struktúra

```
RestApiHasznalat/
├── Program.cs          # Fő belépési pont - API hívás példa
└── ChuckApiHandler.cs  # HttpClient wrapper osztály
```

---

## 🔑 Fő Komponensek

### 1. Program.cs - Fő Belépési Pont

```csharp
// ChuckApiHandler példány létrehozása a base URL-lel
ChuckApiHandler chuckApi = new("https://api.chucknorris.io");

// CancellationToken - megszakítható aszinkron művelethez
CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

// Aszinkron API hívás - Task<JokeResponse?> visszatérési értékkel
var jokeTask = chuckApi.GetJokeAsync("jokes/random", cancellationTokenSource.Token);

// .Result - szinkron várakozás az aszinkron műveletre (blokkoló!)
var joke = jokeTask.Result;
```

**Fontos fogalmak:**
- `ChuckApiHandler` - Custom HTTP client wrapper
- `CancellationTokenSource` - Token forrás a művelet megszakításához
- `Task<T>` - Aszinkron művelet eredménye
- `.Result` - Blokkoló várakozás (konzol appban OK, web API-ban TILOS!)

**API hívás megszakítása (kommentelt):**
```csharp
cancellationTokenSource.Cancel();  // Megszakítja a folyamatban lévő kérést
```

**Válasz feldolgozása:**
```csharp
if (joke == null)
{
    Console.WriteLine("Hiba");  // API hiba vagy cancel
}
else
{
    // Joke kiírása formázott dátummal és kategóriákkal
    Console.WriteLine($"Joke: {joke.value} ({DateTime.Parse(joke.created_at):yyyy-MM-dd}), cat: {string.Join(",", joke.categories)}");
}
```

---

### 2. ChuckApiHandler.cs - HTTP Client Osztály

#### HttpClient Inicializálása

```csharp
readonly HttpClient _httpClient;

public ChuckApiHandler(string base_url)
{
    _httpClient = new HttpClient();
    _httpClient.BaseAddress = new Uri(base_url);  // Base URL beállítása
    
    // Accept header beállítása - JSON formátumot várunk
    _httpClient.DefaultRequestHeaders.Accept.Clear();
    _httpClient.DefaultRequestHeaders.Accept.Add(
        new MediaTypeWithQualityHeaderValue("application/json")
    );
}
```

**Fontos:** 
- `HttpClient` egy **readonly** field - egyszer inicializáljuk
- `BaseAddress` - minden kérés ehhez adódik hozzá
- `Accept: application/json` - megmondjuk, hogy JSON választ várunk

**⚠️ Production tipp:** HttpClient-et singleton-ként vagy IHttpClientFactory-val használd!

---

#### JokeResponse Record

```csharp
public record JokeResponse(string[] categories, string value, string created_at);
```

**Mit csinál ez?**
- **Record típus** - Immutable (nem módosítható) osztály
- **Pozícionális paraméterek** - Egyszerű property definíció
- **Value equality** - Két record egyenlő, ha minden property értéke egyenlő
- **JSON deszerializációhoz** - A property nevek egyeznek az API válasszal

**API válasz példa:**
```json
{
  "categories": [],
  "created_at": "2020-01-05 13:42:19.324003",
  "value": "Chuck Norris can kill two stones with one bird."
}
```

---

#### GET Kérés - GetJokeAsync

```csharp
public async Task<JokeResponse?> GetJokeAsync(string path, CancellationToken cancellationToken)
{
    JokeResponse? response = null;  // Nullable return érték
    try
    {
        // HTTP GET kérés küldése
        HttpResponseMessage responseMessage = await _httpClient.GetAsync(path, cancellationToken);
        
        // Sikeres válasz ellenőrzése (200-299 status code)
        if (responseMessage.IsSuccessStatusCode)
        {
            // Response body beolvasása string-ként
            string str = await responseMessage.Content.ReadAsStringAsync(cancellationToken);
            
            // JSON string deszerializálása JokeResponse objektummá
            response = JsonSerializer.Deserialize<JokeResponse>(str);
        }
    }
    catch 
    { 
        // Bármilyen hiba esetén null-t adunk vissza
        // Production-ben: log the error!
    }
    return response;
}
```

**Fontos lépések:**
1. `await _httpClient.GetAsync()` - Aszinkron GET kérés
2. `IsSuccessStatusCode` - Ellenőrzi, hogy 2xx válasz érkezett-e
3. `ReadAsStringAsync()` - Response body beolvasása
4. `JsonSerializer.Deserialize<T>()` - JSON → C# objektum

**Async/Await:**
- `async` - A metódus aszinkron
- `await` - Vár az aszinkron művelet befejeződésére (nem blokkolja a thread-et!)
- `Task<T>` - Aszinkron művelet eredménye

---

#### PATCH Kérés - PatchJokeAsync (példa)

```csharp
public async Task<JokeResponse?> PatchJokeAsync<T>(string path, T data, CancellationToken cancellationToken)
{
    JokeResponse? response = null;
    
    // C# objektum → JSON string
    var datastr = JsonSerializer.Serialize(data);
    
    // HTTP PATCH kérés küldése JSON body-val
    HttpResponseMessage responseMessage = await _httpClient.PatchAsync(
        path, 
        new StringContent(datastr),  // Request body
        cancellationToken
    );
    
    // Válasz feldolgozása
    if (responseMessage.IsSuccessStatusCode)
    {
        string str = await responseMessage.Content.ReadAsStringAsync(cancellationToken);
        
        // Ellenőrizzük, hogy nem lett-e közben cancelezve
        if (!cancellationToken.IsCancellationRequested)
        {
            response = JsonSerializer.Deserialize<JokeResponse>(str);
        }
    }
    return response;
}
```

**Generic típus:** `<T>` - Bármilyen típusú objektumot küldhetsz

**PATCH vs POST vs PUT:**
- **POST** - Új erőforrás létrehozása
- **PUT** - Teljes erőforrás frissítése
- **PATCH** - Részleges frissítés

---

## 🚀 Hogyan Használd?

### 1. Projekt Futtatása

```bash
cd RestApiHasznalat/RestApiHasznalat
dotnet run
```

**Kimenet példa:**
```
Joke: Chuck Norris can kill two stones with one bird. (2020-01-05), cat: 
```

---

### 2. Saját API Hívás Készítése

```csharp
// 1. Handler létrehozása
ChuckApiHandler api = new("https://api.example.com");

// 2. CancellationToken forrás
CancellationTokenSource cts = new();

// 3. Aszinkron hívás
var result = await api.GetJokeAsync("endpoint/path", cts.Token);

// 4. Feldolgozás
if (result != null)
{
    Console.WriteLine(result.value);
}
```

---

## 📊 Async/Await Pattern Magyarázat

### Szinkron vs Aszinkron

**Szinkron (rossz konzol appban, de egyszerű):**
```csharp
var joke = chuckApi.GetJokeAsync("jokes/random", ct).Result;  // BLOKKOLJA A THREAD-ET!
```

**Aszinkron (helyes):**
```csharp
var joke = await chuckApi.GetJokeAsync("jokes/random", ct);   // NEM BLOKKOLJA!
```

**⚠️ FIGYELEM:**
- `.Result` - Konzol appban OK, de web API-ban **deadlock**-ot okozhat!
- `async Main` - .NET 6+ támogatja, használd ha lehet

---

## 🔐 CancellationToken Használata

```csharp
CancellationTokenSource cts = new();

// Időzített cancel (5 másodperc után)
cts.CancelAfter(TimeSpan.FromSeconds(5));

// Vagy manuális cancel
// cts.Cancel();

try
{
    var result = await api.GetJokeAsync("jokes/random", cts.Token);
}
catch (OperationCanceledException)
{
    Console.WriteLine("Művelet megszakítva!");
}
```

**Miért fontos?**
- **Timeout** - Ne várjon a végtelenségig
- **User cancel** - Felhasználó megszakíthatja
- **Resource cleanup** - Erőforrások felszabadítása

---

## 🧪 Chuck Norris API Dokumentáció

**Base URL:** `https://api.chucknorris.io`

**Endpoint-ok:**
- `GET /jokes/random` - Random vicc
- `GET /jokes/categories` - Kategóriák listája
- `GET /jokes/random?category={category}` - Kategória szerinti vicc

**Válasz formátum:**
```json
{
  "categories": ["dev"],
  "created_at": "2020-01-05 13:42:19.324003",
  "icon_url": "https://assets.chucknorris.host/img/avatar/chuck-norris.png",
  "id": "abc123",
  "updated_at": "2020-01-05 13:42:19.324003",
  "url": "https://api.chucknorris.io/jokes/abc123",
  "value": "Chuck Norris writes code that optimizes itself."
}
```

---

## 🎓 Következő Lépések

Miután megértetted ezt a projektet:
1. ✅ Próbálj más API-kat hívni (pl. OpenWeatherMap, GitHub API)
2. ✅ Implementálj POST/PUT/DELETE metódusokat
3. ✅ Add hozzá error handling-et és logging-ot
4. ✅ Használj `IHttpClientFactory`-t (DI pattern)
5. ✅ Nézd meg a **TodoApiController** projektet - saját API készítés

---

## 💡 Gyakori Hibák és Megoldások

### 1. HttpClient Singleton Pattern

**❌ Rossz:**
```csharp
using (HttpClient client = new HttpClient())  // NE!
{
    // Socket exhaustion!
}
```

**✅ Helyes:**
```csharp
// Egy példány az alkalmazás életciklusa alatt
private static readonly HttpClient _httpClient = new();
```

### 2. Deadlock .Result használatával

**❌ Rossz (ASP.NET Core-ban):**
```csharp
var result = GetDataAsync().Result;  // DEADLOCK!
```

**✅ Helyes:**
```csharp
var result = await GetDataAsync();
```

### 3. Exception nem kezelve

**❌ Rossz:**
```csharp
var response = await client.GetAsync(url);  // Mi van hálózati hiba esetén?
```

**✅ Helyes:**
```csharp
try
{
    var response = await client.GetAsync(url);
    response.EnsureSuccessStatusCode();  // Dobja HttpRequestException-t
}
catch (HttpRequestException ex)
{
    Console.WriteLine($"Error: {ex.Message}");
}
```

---

**Készítve tanulási célból** 🚀
**Következő:** TodoApiController - Saját API létrehozása
