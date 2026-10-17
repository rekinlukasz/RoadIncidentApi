using Microsoft.EntityFrameworkCore;
using RoadIncidentApi.Data;
using RoadIncidentApi.DTOs;
using RoadIncidentApi.Exceptions;
using RoadIncidentApi.Mapping;
using RoadIncidentApi.Models;

namespace RoadIncidentApi.Services;

/// <summary>
/// Serwis obsługujący zgłoszenia. Tutaj znajduje się logika biznesowa
/// systemu (kontrolery pozostają "chude").
/// </summary>
public class ReportService : IReportService
{
    private readonly AppDbContext _db;

    public ReportService(AppDbContext db) => _db = db;

    /// <summary>
    /// REGUŁA BIZNESOWA #2: dozwolona macierz przejść między statusami.
    /// Status może zmieniać się tylko zgodnie z naturalnym przepływem;
    /// nie można cofnąć zgłoszenia zakończonego/odrzuconego ani przeskakiwać etapów wstecz.
    /// </summary>
    private static readonly Dictionary<ReportStatus, ReportStatus[]> AllowedTransitions = new()
    {
        [ReportStatus.Nowe] = new[] { ReportStatus.Zweryfikowane, ReportStatus.Odrzucone },
        [ReportStatus.Zweryfikowane] = new[] { ReportStatus.WTrakcie, ReportStatus.Odrzucone },
        [ReportStatus.WTrakcie] = new[] { ReportStatus.Zakonczone, ReportStatus.Odrzucone },
        [ReportStatus.Zakonczone] = Array.Empty<ReportStatus>(),
        [ReportStatus.Odrzucone] = Array.Empty<ReportStatus>()
    };

    public async Task<PagedResult<ReportDto>> GetAsync(ReportQueryParameters query)
    {
        var q = _db.Reports
            .Include(r => r.Reporter)
            .AsNoTracking()
            .AsQueryable();

        if (query.Status.HasValue)
            q = q.Where(r => r.Status == query.Status.Value);

        if (query.Type.HasValue)
            q = q.Where(r => r.Type == query.Type.Value);

        if (!string.IsNullOrWhiteSpace(query.City))
        {
            var city = query.City.ToLower();
            q = q.Where(r => r.City.ToLower().Contains(city));
        }

        int total = await q.CountAsync();

        var items = await q
            .OrderByDescending(r => r.OccurredAt)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync();

        return new PagedResult<ReportDto>
        {
            Items = items.Select(r => r.ToDto()).ToList(),
            Page = query.Page,
            PageSize = query.PageSize,
            TotalItems = total
        };
    }

    public async Task<ReportDto> GetByIdAsync(int id)
    {
        var report = await _db.Reports
            .Include(r => r.Reporter)
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == id);

        if (report is null)
            throw new NotFoundException($"Nie znaleziono zgłoszenia o id {id}.");

        return report.ToDto();
    }

    public async Task<ReportDto> CreateAsync(CreateReportDto dto)
    {
        // Walidacja relacji: zgłaszający musi istnieć.
        bool reporterExists = await _db.Reporters.AnyAsync(r => r.Id == dto.ReporterId);
        if (!reporterExists)
            throw new DomainException($"Nie istnieje zgłaszający o id {dto.ReporterId}.");

        // REGUŁA BIZNESOWA #1: data wystąpienia nie może być z przyszłości.
        if (dto.OccurredAt > DateTime.UtcNow.AddMinutes(5)) // 5 min tolerancji na różnice zegarów
            throw new DomainException("Data wystąpienia zdarzenia nie może być z przyszłości.");

        // REGUŁA BIZNESOWA #3 (dodatkowa): brak duplikatów aktywnych zgłoszeń
        // tego samego typu w tej samej lokalizacji (w promieniu ok. 100 m).
        var activeStatuses = new[] { ReportStatus.Nowe, ReportStatus.Zweryfikowane, ReportStatus.WTrakcie };
        var candidates = await _db.Reports
            .Where(r => r.Type == dto.Type && activeStatuses.Contains(r.Status))
            .ToListAsync();

        bool duplicate = candidates.Any(r =>
            DistanceMeters(r.Latitude, r.Longitude, dto.Latitude, dto.Longitude) < 100);

        if (duplicate)
            throw new DomainException(
                "Istnieje już aktywne zgłoszenie tego samego typu w tej lokalizacji.");

        var entity = dto.ToEntity();
        _db.Reports.Add(entity);
        await _db.SaveChangesAsync();

        // Doładowanie zgłaszającego do odpowiedzi.
        await _db.Entry(entity).Reference(e => e.Reporter).LoadAsync();
        return entity.ToDto();
    }

    public async Task<ReportDto> ChangeStatusAsync(int id, UpdateReportStatusDto dto)
    {
        var report = await _db.Reports
            .Include(r => r.Reporter)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (report is null)
            throw new NotFoundException($"Nie znaleziono zgłoszenia o id {id}.");

        if (report.Status == dto.NewStatus)
            throw new DomainException($"Zgłoszenie ma już status '{dto.NewStatus}'.");

        // REGUŁA BIZNESOWA #2: sprawdzenie dozwolonego przejścia.
        var allowed = AllowedTransitions[report.Status];
        if (!allowed.Contains(dto.NewStatus))
        {
            var allowedText = allowed.Length == 0
                ? "brak (status końcowy)"
                : string.Join(", ", allowed);
            throw new DomainException(
                $"Niedozwolona zmiana statusu z '{report.Status}' na '{dto.NewStatus}'. " +
                $"Dozwolone przejścia: {allowedText}.");
        }

        report.Status = dto.NewStatus;
        report.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        return report.ToDto();
    }

    public async Task DeleteAsync(int id)
    {
        var report = await _db.Reports.FindAsync(id);
        if (report is null)
            throw new NotFoundException($"Nie znaleziono zgłoszenia o id {id}.");

        _db.Reports.Remove(report);
        await _db.SaveChangesAsync();
    }

    public async Task<IReadOnlyList<ReportDto>> GetAllForExportAsync()
    {
        var reports = await _db.Reports
            .Include(r => r.Reporter)
            .AsNoTracking()
            .OrderByDescending(r => r.OccurredAt)
            .ToListAsync();

        return reports.Select(r => r.ToDto()).ToList();
    }

    /// <summary>Przybliżona odległość między dwoma punktami (wzór haversine, w metrach).</summary>
    private static double DistanceMeters(double lat1, double lon1, double lat2, double lon2)
    {
        const double r = 6371000; // promień Ziemi w metrach
        double dLat = ToRad(lat2 - lat1);
        double dLon = ToRad(lon2 - lon1);
        double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                   Math.Cos(ToRad(lat1)) * Math.Cos(ToRad(lat2)) *
                   Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
        double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        return r * c;
    }

    private static double ToRad(double deg) => deg * Math.PI / 180.0;
}
