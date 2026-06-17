using System.ComponentModel.DataAnnotations;
using RoadIncidentApi.Models;

namespace RoadIncidentApi.DTOs;

/// <summary>Dane wejściowe do zmiany statusu zgłoszenia.</summary>
public class UpdateReportStatusDto
{
    [Required(ErrorMessage = "Nowy status jest wymagany.")]
    [EnumDataType(typeof(ReportStatus), ErrorMessage = "Nieprawidłowy status.")]
    public ReportStatus NewStatus { get; set; }
}
