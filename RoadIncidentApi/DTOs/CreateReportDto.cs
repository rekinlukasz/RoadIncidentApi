using System.ComponentModel.DataAnnotations;
using RoadIncidentApi.Models;

namespace RoadIncidentApi.DTOs;

/// <summary>Dane wejściowe do utworzenia nowego zgłoszenia.</summary>
public class CreateReportDto
{
    [Required(ErrorMessage = "Tytuł jest wymagany.")]
    [StringLength(150, MinimumLength = 5, ErrorMessage = "Tytuł musi mieć od 5 do 150 znaków.")]
    public string Title { get; set; } = string.Empty;

    [StringLength(2000, ErrorMessage = "Opis może mieć maksymalnie 2000 znaków.")]
    public string? Description { get; set; }

    [Required(ErrorMessage = "Rodzaj zdarzenia jest wymagany.")]
    [EnumDataType(typeof(IncidentType), ErrorMessage = "Nieprawidłowy rodzaj zdarzenia.")]
    public IncidentType Type { get; set; }

    [Required(ErrorMessage = "Miasto jest wymagane.")]
    [StringLength(100)]
    public string City { get; set; } = string.Empty;

    [StringLength(150)]
    public string? Street { get; set; }

    [Range(-90, 90, ErrorMessage = "Szerokość geograficzna musi być w zakresie -90..90.")]
    public double Latitude { get; set; }

    [Range(-180, 180, ErrorMessage = "Długość geograficzna musi być w zakresie -180..180.")]
    public double Longitude { get; set; }

    [Required(ErrorMessage = "Data wystąpienia zdarzenia jest wymagana.")]
    public DateTime OccurredAt { get; set; }

    [Required(ErrorMessage = "Identyfikator zgłaszającego jest wymagany.")]
    public int ReporterId { get; set; }
}
