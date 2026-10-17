namespace RoadIncidentApi.Models;

/// <summary>
/// Encja główna systemu — zgłoszenie dotyczące zdarzenia drogowego
/// (wypadek, roboty drogowe, przeszkoda itp.).
/// </summary>
public class IncidentReport
{
    public int Id { get; set; }

    /// <summary>Krótki tytuł zgłoszenia.</summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>Szczegółowy opis zdarzenia.</summary>
    public string? Description { get; set; }

    /// <summary>Rodzaj zdarzenia.</summary>
    public IncidentType Type { get; set; }

    /// <summary>Aktualny status obsługi.</summary>
    public ReportStatus Status { get; set; } = ReportStatus.Nowe;

    /// <summary>Miasto / miejscowość.</summary>
    public string City { get; set; } = string.Empty;

    /// <summary>Ulica / lokalizacja szczegółowa (opcjonalna).</summary>
    public string? Street { get; set; }

    /// <summary>Szerokość geograficzna.</summary>
    public double Latitude { get; set; }

    /// <summary>Długość geograficzna.</summary>
    public double Longitude { get; set; }

    /// <summary>Data i czas wystąpienia zdarzenia.</summary>
    public DateTime OccurredAt { get; set; }

    /// <summary>Data utworzenia zgłoszenia w systemie.</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>Data ostatniej modyfikacji (np. zmiany statusu).</summary>
    public DateTime? UpdatedAt { get; set; }

    // --- Relacja do zgłaszającego (klucz obcy) ---

    /// <summary>Identyfikator zgłaszającego.</summary>
    public int ReporterId { get; set; }

    /// <summary>Właściwość nawigacyjna do zgłaszającego.</summary>
    public Reporter? Reporter { get; set; }
}
