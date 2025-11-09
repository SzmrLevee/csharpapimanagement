// System.Net.Http - HTTP kérésekhez szükséges
// System.Text.Json - JSON serialize/deserialize
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace RestApiHasznalat
{
    /// <summary>
    /// ChuckApiHandler - HttpClient wrapper osztály
    /// Chuck Norris API-hoz (vagy bármilyen REST API-hoz használható)
    /// </summary>
    internal class ChuckApiHandler
    {
        // HttpClient példány - SINGLETON pattern!
        // Egy alkalmazásban NE hozz létre sok HttpClient-et (socket exhaustion!)
        readonly HttpClient _httpClient;

        /// <summary>
        /// Constructor - HttpClient inicializálása
        /// </summary>
        /// <param name="base_url">Base URL (pl. https://api.chucknorris.io)</param>
        public ChuckApiHandler(string base_url)
        {
            // HttpClient példány létrehozása
            _httpClient = new HttpClient();
            
            // Base URL beállítása - minden kérés ehhez adódik hozzá
            // pl. BaseAddress = "https://api.chucknorris.io"
            //     GetAsync("jokes/random") → "https://api.chucknorris.io/jokes/random"
            _httpClient.BaseAddress = new Uri(base_url);

            // Accept header beállítása - megmondjuk, hogy JSON választ várunk
            // "Accept: application/json"
            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json")
            );
        }

        /// <summary>
        /// JokeResponse record - API válasz modell
        /// Record = immutable (nem módosítható) osztály
        /// Property nevek egyeznek az API JSON kulcsaival
        /// </summary>
        /// <param name="categories">Kategóriák tömbje (lehet üres)</param>
        /// <param name="value">A vicc szövege</param>
        /// <param name="created_at">Létrehozás dátuma (string formátum)</param>
        public record JokeResponse(string[] categories, string value, string created_at);

        /// <summary>
        /// GET kérés - Vicc lekérése az API-ból
        /// </summary>
        /// <param name="path">Endpoint path (pl. "jokes/random")</param>
        /// <param name="cancellationToken">Token a művelet megszakításához</param>
        /// <returns>JokeResponse vagy null hiba esetén</returns>
        public async Task<JokeResponse?> GetJokeAsync(string path, CancellationToken cancellationToken)
        {
            // Nullable return érték - null = hiba történt
            JokeResponse? response = null;
            
            try
            {
                // HTTP GET kérés küldése aszinkron módon
                // await - megvárja a válaszét, de nem blokkolja a thread-et
                // cancellationToken - ha Cancel()-t hívunk, megszakítja a kérést
                HttpResponseMessage responseMessage = await _httpClient.GetAsync(path, cancellationToken);
                
                // Sikeres válasz ellenőrzése (200-299 status code)
                if (responseMessage.IsSuccessStatusCode)
                {
                    // Response body beolvasása string-ként (JSON)
                    string str = await responseMessage.Content.ReadAsStringAsync(cancellationToken);
                    
                    // JSON string deszerializálása C# objektummá
                    // JsonSerializer automatikusan mapping-eli a property neveket
                    response = JsonSerializer.Deserialize<JokeResponse>(str);
                }
            }
            catch 
            { 
                // Bármilyen hiba esetén null-t adunk vissza
                // Lehetséges hibák:
                // - Network error
                // - Timeout
                // - JSON parse error
                // - OperationCanceledException (ha Cancel()-t hívtak)
                // ⚠️ Production-ben: Logolja a hibát!
            }
            
            return response;
        }

        /// <summary>
        /// PATCH kérés - Részleges adatmódosítás (példa metódus)
        /// Generic típus: bármilyen objektumot küldhetsz
        /// </summary>
        /// <typeparam name="T">Küldendő adat típusa</typeparam>
        /// <param name="path">Endpoint path</param>
        /// <param name="data">Küldendő objektum (automatikusan JSON-ná alakul)</param>
        /// <param name="cancellationToken">Token a megszakításhoz</param>
        /// <returns>JokeResponse vagy null</returns>
        public async Task<JokeResponse?> PatchJokeAsync<T>(string path, T data, CancellationToken cancellationToken)
        {
            JokeResponse? response = null;
            
            // C# objektum → JSON string szerializálás
            var datastr = JsonSerializer.Serialize(data);
            
            // HTTP PATCH kérés küldése
            // StringContent - JSON string request body-ként
            HttpResponseMessage responseMessage = await _httpClient.PatchAsync(
                path, 
                new StringContent(datastr),  // Request body
                cancellationToken
            );
            
            // Sikeres válasz feldolgozása
            if (responseMessage.IsSuccessStatusCode)
            {
                // Response body beolvasása
                string str = await responseMessage.Content.ReadAsStringAsync(cancellationToken);
                
                // Ellenőrizzük, hogy közben nem lett-e cancelezve
                if (!cancellationToken.IsCancellationRequested)
                {
                    // JSON → C# objektum deszerializálás
                    response = JsonSerializer.Deserialize<JokeResponse>(str);
                }
            }
            
            return response;
        }
    }
}