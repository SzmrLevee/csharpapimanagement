using System.Security.Cryptography;
using System.Text;

namespace TodoApiController.Model;

/// <summary>
/// User osztály - Felhasználói adatok és PBKDF2-alapú biztonságos jelszó kezelés
/// 
/// Felelősség:
/// - Felhasználói adatok tárolása (username, email, név)
/// - Jelszó biztonságos hash-elése (PBKDF2 algoritmus)
/// - Jelszó ellenőrzés konstans idejű összehasonlítással
/// </summary>
public class User : IUser
{
    /// <summary>
    /// Salt (só) érték a PBKDF2 hash-hez
    /// 
    /// FONTOS BIZTONSÁGI FIGYELMEZTETÉS:
    /// Ez a FIX salt MINDEN felhasználónál UGYANAZ - ez NEM biztonságos!
    /// Éles környezetben minden felhasználónak EGYEDI random salt kell!
    /// 
    /// Helyes megvalósítás:
    /// public byte[] Salt { get; set; } = RandomNumberGenerator.GetBytes(16);
    /// </summary>
    public readonly byte[] Salt = Encoding.UTF8.GetBytes("0123456789012345");
    
    /// <summary>
    /// PBKDF2 iterációk száma - minél több, annál lassabb és biztonságosabb
    /// 10,000 iteráció megfelelő védelem nyújt brute-force támadások ellen
    /// </summary>
    const int Iterations = 10_000;
    
    /// <summary>
    /// Hash mérete byte-okban (32 byte = 256 bit, SHA256 ekvivalens biztonság)
    /// </summary>
    const int HashSize = 32;
    
    /// <summary>
    /// Felhasználónév - egyedi azonosító (unique key az adatbázisban)
    /// </summary>
    public string UserName { get; set; } = "";
    
    /// <summary>
    /// Email cím - kommunikációs végpont
    /// </summary>
    public string Email { get; set; } = "";
    
    /// <summary>
    /// Teljes név - emberi olvasható formátum
    /// </summary>
    public string Name { get; set; } = "";
    
    /// <summary>
    /// Hash-elt jelszó (PBKDF2 kimenet)
    /// private set: csak a PasswordText property tudja beállítani
    /// SOHA ne tárold plain text jelszavakat!
    /// </summary>
    public byte[] Password { get; private set; } = Array.Empty<byte>();
    
    /// <summary>
    /// Jelszó beállító property - AUTOMATIKUS HASH-ELÉS
    /// 
    /// Használat:
    /// var user = new User { PasswordText = "myPassword123" };
    /// // A Password property automatikusan kitöltődik a hash-sel
    /// </summary>
    public string PasswordText
    {
        set
        {
            // PBKDF2 (Password-Based Key Derivation Function 2) használata
            // - value: plain text jelszó
            // - Salt: random érték (itt fix, de kellene random!)
            // - Iterations: hányszor futtassuk a hash algoritmust (lassítás)
            Password = new Rfc2898DeriveBytes(value, Salt, Iterations).GetBytes(HashSize);
        }
    }
    
    /// <summary>
    /// Jelszó ellenőrzés - konstans idejű összehasonlítás
    /// 
    /// Miért konstans idejű?
    /// - Timing attack ellen véd
    /// - Ha byte-onként hasonlítanánk össze és az első eltérésnél visszatérnénk,
    ///   egy támadó mérheti a végrehajtási időt és következtethet a jelszóra
    /// - All() végigmegy az ÖSSZES byte-on, függetlenül attól, hogy mikor talál eltérést
    /// </summary>
    /// <param name="password">Ellenőrizendő plain text jelszó</param>
    /// <returns>true = helyes jelszó, false = hibás jelszó</returns>
    public bool Matches(string password)
    {
        // 1. Ugyanazzal a Salt + Iterations kombinációval hash-eljük a bejövő jelszót
        var bytes = new Rfc2898DeriveBytes(password, Salt, Iterations).GetBytes(HashSize);
        
        // 2. Konstans idejű összehasonlítás:
        //    - Ellenőrizzük, hogy a hossz egyezik (ha nem, azonnal hamis)
        //    - Végigmegyünk 0-tól bytes.Length-ig
        //    - Minden indexnél ellenőrizzük, hogy bytes[i] == Password[i]
        //    - All() csak akkor ad true-t, ha MINDEN elem megfelel
        return bytes.Length == Password.Length 
            && Enumerable.Range(0, bytes.Length)
            .All(i => bytes[i] == Password[i]);
    }
}
