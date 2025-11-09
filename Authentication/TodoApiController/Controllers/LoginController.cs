using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using TodoApiController.Model;
using TodoApiController.Options;

namespace TodoApiController.Controllers
{
    /// <summary>
    /// LoginController - JWT token generálás felhasználói bejelentkezéshez
    /// 
    /// Felelősség:
    /// - Felhasználónév/jelszó hitelesítés
    /// - JWT token generálás sikeres bejelentkezés után
    /// - Token Claims-ek feltöltése felhasználói adatokkal
    /// </summary>
    [Route("api/[controller]")]  // Route: /api/login
    [ApiController]              // Automatikus model validáció és API viselkedés
    public class LoginController : ControllerBase
    {
        // In-memory adattároló a regisztrált felhasználókhoz
        readonly IDataStore dataStore;
        
        // JWT konfigurációs beállítások (Key, Issuer, Audience)
        readonly JwtOptions jwtOptions;

        /// <summary>
        /// Konstruktor - Dependency Injection segítségével kapja meg a függőségeket
        /// </summary>
        /// <param name="dataStore">Adattároló (User-ek lekérdezéséhez)</param>
        /// <param name="options">JWT beállítások (IOptions pattern szerint)</param>
#pragma warning disable IDE0290
        public LoginController(IDataStore dataStore, IOptions<JwtOptions> options)
        {
            // Null-check: biztosítjuk, hogy mindkét dependency be lett injektálva
            this.dataStore = dataStore ?? throw new ArgumentNullException(nameof(dataStore));
            this.jwtOptions = options.Value;  // IOptions<T>.Value adja a konkrét konfigurációt
        }
#pragma warning restore IDE0290

        /// <summary>
        /// POST /api/login - Bejelentkezés és JWT token generálás
        /// </summary>
        /// <param name="value">LoginUser objektum (UserName + Password)</param>
        /// <returns>
        /// 200 OK - JWT token string formában
        /// 400 Bad Request - Hibás felhasználónév vagy jelszó
        /// </returns>
        [HttpPost]
        public IActionResult Post([FromBody] LoginUser value)
        {
            // ========== 1. FELHASZNÁLÓ KERESÉSE ==========
            // IDataStore-t IItemStore<User>-re castoljuk, hogy hozzáférjünk a User specifikus műveletekhez
            var user = ((IItemStore<User>)dataStore).GetAll()
                .FirstOrDefault(x => x.UserName == value.UserName);  // Keresés username alapján
            
            // Ha nincs ilyen felhasználó, visszautasítjuk a kérést
            if(user == null)
            {
                // Biztonsági best practice: ne áruljuk el, hogy a username vagy a password rossz
                return BadRequest("Invalid username or password");
            }

            // ========== 2. JELSZÓ ELLENŐRZÉS ==========
            // User.Matches() metódus PBKDF2 hash-szel összehasonlítja a jelszavakat
            if (!user.Matches(value.Password))
            {
                return BadRequest("Invalid username or password");
            }

            // ========== 3. JWT TOKEN GENERÁLÁS ==========
            
            // 3a. Szimmetrikus kulcs létrehozása (HMAC-SHA256 algoritmushoz)
            // A jwtOptions.Key-t byte tömbbé alakítjuk (min. 32 karakter kell!)
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Key));
            
            // 3b. Aláíró credentials létrehozása (digitális aláíráshoz)
            var cred = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            
            // 3c. Claims (állítások) definiálása - a token-ben tárolt felhasználói adatok
            var claims = new[]
            {
                // ClaimTypes.NameIdentifier - Egyedi felhasználó azonosító (általában username vagy userId)
                new Claim(ClaimTypes.NameIdentifier, user.UserName),
                
                // ClaimTypes.Name - Teljes név (emberi olvasható formában)
                new Claim(ClaimTypes.Name, user.Name),
                
                // ClaimTypes.Email - Email cím
                new Claim(ClaimTypes.Email, user.Email),
                
                // ClaimTypes.Role - Szerepkör (authorization-höz használjuk)
                // Most minden bejelentkezett felhasználó "Felhasznalo" role-t kap
                new Claim(ClaimTypes.Role, "Felhasznalo"),
            };

            // 3d. JWT token objektum összeállítása
            var token = new JwtSecurityToken(
                issuer: jwtOptions.Issuer,        // Kibocsátó (pl. "Kibocsato")
                audience: jwtOptions.Audience,    // Célközönség (pl. "Celkozonseg")
                claims: claims,                   // Claims tömb (user adatok)
                expires: DateTime.Now.AddMinutes(15),  // Lejárati idő (15 perc múlva érvénytelen)
                signingCredentials: cred          // Digitális aláírás (HMAC-SHA256)
            );

            // 3e. Token konvertálása string formátumba (Base64 encoded)
            // Ez az érték kerül vissza a kliensnek, amit a későbbi kérésekben használ
            var tokenStr = new JwtSecurityTokenHandler().WriteToken(token);
            
            // ========== 4. TOKEN VISSZAADÁSA ==========
            // A kliens ezt a token-t fogja használni az Authorization headerben:
            // Authorization: Bearer <tokenStr>
            return Ok(tokenStr);
        }
    }
}
