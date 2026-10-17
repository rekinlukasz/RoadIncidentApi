using System.ComponentModel.DataAnnotations;

namespace RoadIncidentApi.DTOs;

/// <summary>Dane wejściowe do utworzenia zgłaszającego.</summary>
public class CreateReporterDto
{
    [Required(ErrorMessage = "Imię i nazwisko jest wymagane.")]
    [StringLength(120, MinimumLength = 3, ErrorMessage = "Imię i nazwisko musi mieć od 3 do 120 znaków.")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Adres e-mail jest wymagany.")]
    [EmailAddress(ErrorMessage = "Nieprawidłowy format adresu e-mail.")]
    [StringLength(200)]
    public string Email { get; set; } = string.Empty;

    [Phone(ErrorMessage = "Nieprawidłowy numer telefonu.")]
    [StringLength(30)]
    public string? PhoneNumber { get; set; }
}
