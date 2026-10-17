using RoadIncidentApi.DTOs;

namespace RoadIncidentApi.Services;

/// <summary>Funkcjonalność dodatkowa: eksport zgłoszeń do pliku CSV.</summary>
public interface ICsvExportService
{
    byte[] ExportReports(IEnumerable<ReportDto> reports);
}
