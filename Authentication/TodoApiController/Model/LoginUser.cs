namespace TodoApiController.Model
{
    /// <summary>
    /// LoginUser - Bejelentkezési adatok model osztály
    /// 
    /// Egyszerű DTO (Data Transfer Object) a login endpoint-hoz.
    /// Csak a username és password mezőket tartalmazza (nem kell email, név, stb.)
    /// </summary>
    public class LoginUser
    {
        /// <summary>
        /// Felhasználónév (egyedi azonosító)
        /// </summary>
        public string UserName { get; set; } = "";
        
        /// <summary>
        /// Jelszó PLAIN TEXT formában
        /// Csak átmenetileg él a memóriában a HTTP request során!
        /// Hash-elés a LoginController-ben történik validáláskor
        /// </summary>
        public string Password { get; set; } = "";
    }
}
