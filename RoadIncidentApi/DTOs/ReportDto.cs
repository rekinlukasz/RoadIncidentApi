using RoadIncidentApi.Models;

namespace RoadIncidentApi.DTOs;

/// <summary>Dane zgłoszenia zwracane przez API.</summary>
public class ReportDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public IncidentType Type { get; set; }
    public ReportStatus Status { get; set; }
    public string City { get; set; } = string.Empty;
    public string? Street { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public DateTime OccurredAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public int ReporterId { get; set; }
    public string? ReporterName { get; set; }
}
