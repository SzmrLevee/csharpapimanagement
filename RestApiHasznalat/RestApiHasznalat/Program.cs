// RestApiHasznalat - Külső REST API fogyasztása C#-ban
// Példa: Chuck Norris API - random viccek letöltése

using RestApiHasznalat;

// ChuckApiHandler példány létrehozása a Chuck Norris API base URL-lel
// Ez egy wrapper osztály a HttpClient köré
ChuckApiHandler chuckApi = new("https://api.chucknorris.io");

// CancellationTokenSource - aszinkron műveletek megszakításához
// Ha .Cancel()-t hívunk rajta, megszakítja a folyamatban lévő HTTP kérést
CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

// Aszinkron API hívás indítása
// GetJokeAsync() visszaad egy Task<JokeResponse?> objektumot
// "jokes/random" - a base URL-hez hozzáadódó path (https://api.chucknorris.io/jokes/random)
var jokeTask = chuckApi.GetJokeAsync("jokes/random", cancellationTokenSource.Token);

// OPCIONÁLIS: Művelet megszakítása (jelenleg kommentelt)
// Ha itt meghívnánk, a HTTP kérés megszakadna
//cancellationTokenSource.Cancel();

// .Result - szinkron várakozás az aszinkron művelet befejezésére
// ⚠️ FIGYELEM: Blokkolja a jelenlegi thread-et!
// Konzol alkalmazásban OK, de ASP.NET Core-ban DEADLOCK-ot okozhat!
var joke = jokeTask.Result;

// Válasz ellenőrzése és kiírása
if (joke == null)
{
    // null = hiba történt (pl. network error, API down, vagy cancel)
    Console.WriteLine("Hiba");
}
else
{
    // Sikeres válasz feldolgozása
    // joke.value - a vicc szövege
    // joke.created_at - létrehozás dátuma (string formátum)
    // joke.categories - kategóriák tömbje (lehet üres)
    Console.WriteLine($"Joke: {joke.value} ({DateTime.Parse(joke.created_at):yyyy-MM-dd}), cat: {string.Join(",", joke.categories)}");
}