using RoadIncidentApi.DTOs;

namespace RoadIncidentApi.Services;

public interface IReportService
{
    Task<PagedResult<ReportDto>> GetAsync(ReportQueryParameters query);
    Task<ReportDto> GetByIdAsync(int id);
    Task<ReportDto> CreateAsync(CreateReportDto dto);
    Task<ReportDto> ChangeStatusAsync(int id, UpdateReportStatusDto dto);
    Task DeleteAsync(int id);

    /// <summary>Zwraca wszystkie zgłoszenia (na potrzeby eksportu CSV).</summary>
    Task<IReadOnlyList<ReportDto>> GetAllForExportAsync();
}
