namespace RoadIncidentApi.Models;

/// <summary>
/// Użytkownik systemu — osoba zgłaszająca zdarzenia drogowe.
/// Pełni rolę "Owner/Client" zgodnie z wymaganiami projektu.
/// </summary>
public class Reporter
{
    public int Id { get; set; }

    /// <summary>Imię i nazwisko zgłaszającego.</summary>
    public string FullName { get; set; } = string.Empty;

    /// <summary>Adres e-mail (kontakt zwrotny).</summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>Numer telefonu (opcjonalny).</summary>
    public string? PhoneNumber { get; set; }

    /// <summary>Data rejestracji zgłaszającego w systemie.</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Relacja 1:N — jeden zgłaszający może utworzyć wiele zgłoszeń.
    /// </summary>
    public ICollection<IncidentReport> Reports { get; set; } = new List<IncidentReport>();
}
