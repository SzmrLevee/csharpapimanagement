using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TodoApiController.Model;
using TodoApiController.Validators;

namespace TodoApiController.Controllers
{
    /// <summary>
    /// UserController - Felhasználó kezelés CRUD műveletekkel és role-based authorizációval
    /// 
    /// Felelősség:
    /// - User CRUD műveletek (Create, Read, Update, Delete)
    /// - Claims-based authorization (saját profil módosítás)
    /// - Role-based authorization (Admin-only műveletek)
    /// - FluentValidation integráció
    /// </summary>
    [Route("api/[controller]")]  // Route: /api/user
    [ApiController]              // Automatikus model binding és validáció
    public class UserController : ControllerBase
    {
        // In-memory adattároló a User objektumokhoz
        readonly IDataStore dataStore;
        
        // FluentValidation validator a User objektumok validálásához
        readonly IValidator<User> validator;

        /// <summary>
        /// Konstruktor - DI injektálja a dataStore-t és a validator-t
        /// </summary>
#pragma warning disable IDE0290
        public UserController(IDataStore dataStore, IValidator<User> validator)
        {
            this.dataStore = dataStore ?? throw new ArgumentNullException(nameof(dataStore));
            this.validator = validator ?? throw new ArgumentNullException(nameof(validator));
        }
#pragma warning restore IDE0290

        /// <summary>
        /// GET /api/user - Összes felhasználó listázása
        /// 
        /// Nincs authentikáció/authorizáció - bárki meghívhatja (publikus endpoint)
        /// </summary>
        /// <returns>User objektumok listája</returns>
        [HttpGet]
        public IEnumerable<User> Get()
        {
            // IDataStore-t IItemStore<User>-re castoljuk a User specifikus műveletek eléréséhez
            return ((IItemStore<User>)dataStore).GetAll();
        }

        /// <summary>
        /// GET /api/user/{username} - Egyedi felhasználó lekérése username alapján
        /// </summary>
        /// <param name="username">Felhasználónév (pl. "testuser")</param>
        /// <returns>
        /// 200 OK - User objektum
        /// 404 Not Found - Nem létezik ilyen username
        /// </returns>
        [HttpGet("{username}")]
        public IActionResult Get(string username)
        {
            // Keresés username alapján
            var item = Get().FirstOrDefault(x => x.UserName == username);
            
            // Ternary operator: ha null, akkor NotFound(), különben Ok(item)
            return item == null ? NotFound() : Ok(item);
        }

        /// <summary>
        /// POST /api/user - Új felhasználó regisztrálása
        /// 
        /// Nincs authentikáció - bárki regisztrálhat új felhasználót (mint egy nyilvános signup)
        /// </summary>
        /// <param name="value">User objektum (UserName, Email, Name, PasswordText kötelező!)</param>
        /// <returns>
        /// 200 OK - Sikeresen létrehozott User objektum
        /// 400 Bad Request - Validációs hibák (FluentValidation eredmény)
        /// </returns>
        [HttpPost]
        public IActionResult Post([FromBody] User value)
        {
            // FluentValidation: User objektum validálása (UserValidator szabályok szerint)
            var result = validator.Validate(value);
            
            // Ha nem valid, visszaadjuk a validációs hibákat
            if (!result.IsValid)
            {
                return BadRequest(result);  // result tartalmazza az összes hibát (property név + hibaüzenet)
            }
            
            // Új user hozzáadása az in-memory DataStore-hoz
            dataStore.Add(value);
            
            // Sikeres regisztráció esetén visszaadjuk a létrehozott User objektumot
            return Ok(value);
        }

        /// <summary>
        /// PUT /api/user/{username} - Felhasználó módosítása (VÉDETT ENDPOINT!)
        /// 
        /// AUTHORIZATION SZABÁLYOK:
        /// 1. Saját profil MINDIG módosítható (ha be vagy jelentkezve)
        /// 2. Más felhasználó profilját CSAK Administrator role-lal lehet módosítani
        /// 3. UserName SOHA nem változtatható meg (business rule)
        /// </summary>
        /// <param name="username">Módosítandó felhasználó username-je</param>
        /// <param name="value">Frissített User objektum</param>
        /// <returns>
        /// 200 OK - Sikeres módosítás
        /// 400 Bad Request - Username változtatás kísérlete, vagy nincs jogosultság
        /// 401 Unauthorized - Nincs bejelentkezve
        /// 404 Not Found - Nem létező user
        /// </returns>
        [HttpPut("{username}")]
        [Authorize]  // KRITIKUS: Csak bejelentkezett felhasználók hívhatják meg!
        public IActionResult Put(string username, [FromBody] User value)
        {
            // ========== 1. AUTHENTIKÁCIÓ ELLENŐRZÉSE ==========
            // HttpContext.User.Identity tartalmazza a JWT token-ből kinyert Claims-eket
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            
            // Ha valamilyen oknál fogva nincs identity (nem kellene megtörténnie [Authorize] miatt)
            if(identity == null)
            {
                return BadRequest("Something went wrong");
            }

            // ========== 2. USERNAME VÁLTOZTATÁS MEGAKADÁLYOZÁSA ==========
            // Business rule: username immutable (nem módosítható)
            if (username != value.UserName)
            {
                return BadRequest("May not change username");
            }

            // ========== 3. AUTHORIZATION LOGIKA ==========
            // Kérdés: Ki a bejelentkezett felhasználó?
            // A JWT token-ben ClaimTypes.NameIdentifier-ként tároltuk a username-t
            var identityUser = identity.Claims
                .FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)
                ?.Value;  // pl. "testuser"

            // ========== 4. SAJÁT PROFIL vs. MÁS FELHASZNÁLÓ ELLENŐRZÉSE ==========
            if(username != identityUser)
            {
                // Ha NEM saját profilt próbálja módosítani, ellenőrizzük az Administrator role-t
                
                // Claims-ből kiszűrjük az összes ClaimTypes.Role értéket
                var isAdmin = identity
                    .Claims
                    .Where(x => x.Type == ClaimTypes.Role)   // Csak role claim-ek
                    .Where(x => x.Value != null)             // Null-check
                    .Select(x => x.Value!)                   // Value kinyerése
                    .Any(x => x == "Administrator");         // Van-e "Administrator" role?

                // Ha nincs Administrator role, tiltjuk a műveletet
                if(!isAdmin)
                {
                    return BadRequest("May not change someone else");
                }
                // Ha van Administrator role, folytatódik a metódus (engedélyezzük a módosítást)
            }
            // Ha saját profil (username == identityUser), akkor is folytatódik

            // ========== 5. VALIDÁCIÓ (FluentValidation) ==========
            var result = validator.Validate(value);
            if (!result.IsValid)
            {
                return BadRequest(result);  // Validációs hibák visszaadása
            }

            // ========== 6. ADATBÁZIS FRISSÍTÉS ==========
            // Username biztosítása (már tudjuk, hogy nem változott, de explicit beállítjuk)
            value.UserName = username;
            
            // DataStore.Update() visszatérési értéke: true = sikeres, false = nem létezik
            // FIGYELEM: Ez ellentétes a TodoController-ben látott logikával!
            // (Lehet bug a DataStore implementációban)
            if (dataStore.Update(value))
            {
                return NotFound();  // Ha true-val tér vissza, azt NotFound-ként kezeljük
            }

            // Sikeres frissítés - visszaadjuk a módosított User objektumot
            return Ok(value);
        }

        // DELETE api/<UserController>/valaki
        [HttpDelete("{username}")]
        [Authorize(Roles = "Administrator")]
        public IActionResult Delete(string username)
        {
            var item = Get().FirstOrDefault(x => x.UserName == username);
            if (item == null) { return NotFound(); }
            dataStore.Delete(item);
            return Ok();
        }
    }
}