using RoadIncidentApi.DTOs;

namespace RoadIncidentApi.Services;

public interface IReporterService
{
    Task<IReadOnlyList<ReporterDto>> GetAllAsync();
    Task<ReporterDto> GetByIdAsync(int id);
    Task<ReporterDto> CreateAsync(CreateReporterDto dto);
    Task DeleteAsync(int id);
}
