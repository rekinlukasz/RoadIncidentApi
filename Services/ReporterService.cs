using Microsoft.EntityFrameworkCore;
using RoadIncidentApi.Data;
using RoadIncidentApi.DTOs;
using RoadIncidentApi.Exceptions;
using RoadIncidentApi.Mapping;

namespace RoadIncidentApi.Services;

public class ReporterService : IReporterService
{
    private readonly AppDbContext _db;

    public ReporterService(AppDbContext db) => _db = db;

    public async Task<IReadOnlyList<ReporterDto>> GetAllAsync()
    {
        var reporters = await _db.Reporters
            .Include(r => r.Reports)
            .AsNoTracking()
            .OrderBy(r => r.FullName)
            .ToListAsync();

        return reporters.Select(r => r.ToDto()).ToList();
    }

    public async Task<ReporterDto> GetByIdAsync(int id)
    {
        var reporter = await _db.Reporters
            .Include(r => r.Reports)
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == id);

        if (reporter is null)
            throw new NotFoundException($"Nie znaleziono zgłaszającego o id {id}.");

        return reporter.ToDto();
    }

    public async Task<ReporterDto> CreateAsync(CreateReporterDto dto)
    {
        // Reguła pomocnicza: unikalność adresu e-mail.
        var emailNormalized = dto.Email.Trim().ToLowerInvariant();
        bool emailExists = await _db.Reporters
            .AnyAsync(r => r.Email.ToLower() == emailNormalized);

        if (emailExists)
            throw new DomainException($"Zgłaszający z adresem e-mail '{dto.Email}' już istnieje.");

        var entity = dto.ToEntity();
        _db.Reporters.Add(entity);
        await _db.SaveChangesAsync();

        return entity.ToDto();
    }

    public async Task DeleteAsync(int id)
    {
        var reporter = await _db.Reporters.FindAsync(id);
        if (reporter is null)
            throw new NotFoundException($"Nie znaleziono zgłaszającego o id {id}.");

        _db.Reporters.Remove(reporter);
        await _db.SaveChangesAsync();
    }
}
