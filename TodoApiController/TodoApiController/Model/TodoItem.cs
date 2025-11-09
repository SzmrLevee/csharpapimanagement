namespace TodoApiController.Model;

/// <summary>
/// TodoItem - Todo lista elem modell
/// Egy teendő feladat adatait tárolja
/// </summary>
public class TodoItem
{
    /// <summary>
    /// Egyedi azonosító
    /// Automatikusan generálódik POST műveletkor
    /// </summary>
    public int Id { get; set; }
    
    /// <summary>
    /// Todo címe
    /// Validáció: 10-200 karakter között (TodoItemValidator)
    /// </summary>
    public string Title { get; set; } = string.Empty;
    
    /// <summary>
    /// Todo részletes leírása
    /// Validáció: nem lehet üres (TodoItemValidator)
    /// </summary>
    public string Description { get; set; } = string.Empty;
    
    /// <summary>
    /// Határidő dátum
    /// DateTime formátum: "2025-11-10T00:00:00"
    /// </summary>
    public DateTime DueDate { get; set; }
}
