namespace RoadIncidentApi.DTOs;

/// <summary>Dane zgłaszającego zwracane przez API.</summary>
public class ReporterDto
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public DateTime CreatedAt { get; set; }
    public int ReportsCount { get; set; }
}
