namespace TodoApiController.Model
{
    /// <summary>
    /// IUser interface - Jelszókezelő interfész
    /// 
    /// Definiálja a jelszó-related property-ket, amiket a User osztály implementál.
    /// </summary>
    public interface IUser
    {
        /// <summary>Hash-elt jelszó byte array formában (read-only)</summary>
        byte[] Password { get; }
        
        /// <summary>Plain text jelszó setter (write-only, automatikus hash-elés)</summary>
        string PasswordText { set; }
        
        /// <summary>Felhasználónév (egyedi azonosító)</summary>
        string UserName { get; set; }
    }
}
