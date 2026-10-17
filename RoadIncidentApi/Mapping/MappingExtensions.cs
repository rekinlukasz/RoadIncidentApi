using RoadIncidentApi.DTOs;
using RoadIncidentApi.Models;

namespace RoadIncidentApi.Mapping;

/// <summary>
/// Ręczne mapowanie między encjami a DTO (separacja warstwy domeny
/// od kontraktu API). Świadomie zrezygnowano z AutoMappera na rzecz
/// jawnych, łatwych do prześledzenia konwersji.
/// </summary>
public static class MappingExtensions
{
    public static ReportDto ToDto(this IncidentReport entity) => new()
    {
        Id = entity.Id,
        Title = entity.Title,
        Description = entity.Description,
        Type = entity.Type,
        Status = entity.Status,
        City = entity.City,
        Street = entity.Street,
        Latitude = entity.Latitude,
        Longitude = entity.Longitude,
        OccurredAt = entity.OccurredAt,
        CreatedAt = entity.CreatedAt,
        UpdatedAt = entity.UpdatedAt,
        ReporterId = entity.ReporterId,
        ReporterName = entity.Reporter?.FullName
    };

    public static IncidentReport ToEntity(this CreateReportDto dto) => new()
    {
        Title = dto.Title.Trim(),
        Description = dto.Description?.Trim(),
        Type = dto.Type,
        Status = ReportStatus.Nowe,
        City = dto.City.Trim(),
        Street = dto.Street?.Trim(),
        Latitude = dto.Latitude,
        Longitude = dto.Longitude,
        OccurredAt = dto.OccurredAt,
        CreatedAt = DateTime.UtcNow,
        ReporterId = dto.ReporterId
    };

    public static ReporterDto ToDto(this Reporter entity) => new()
    {
        Id = entity.Id,
        FullName = entity.FullName,
        Email = entity.Email,
        PhoneNumber = entity.PhoneNumber,
        CreatedAt = entity.CreatedAt,
        ReportsCount = entity.Reports?.Count ?? 0
    };

    public static Reporter ToEntity(this CreateReporterDto dto) => new()
    {
        FullName = dto.FullName.Trim(),
        Email = dto.Email.Trim(),
        PhoneNumber = dto.PhoneNumber?.Trim(),
        CreatedAt = DateTime.UtcNow
    };
}
