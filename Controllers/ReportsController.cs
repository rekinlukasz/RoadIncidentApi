using Microsoft.AspNetCore.Mvc;
using RoadIncidentApi.DTOs;
using RoadIncidentApi.Services;

namespace RoadIncidentApi.Controllers;

[ApiController]
[Route("api/reports")]
public class ReportsController : ControllerBase
{
    private readonly IReportService _service;
    private readonly ICsvExportService _csv;

    public ReportsController(IReportService service, ICsvExportService csv)
    {
        _service = service;
        _csv = csv;
    }

    /// <summary>
    /// Pobiera stronicowaną, opcjonalnie filtrowaną listę zgłoszeń.
    /// Przykład: /api/reports?page=1&amp;pageSize=10&amp;status=Nowe&amp;type=Wypadek&amp;city=Gdynia
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<PagedResult<ReportDto>>> Get([FromQuery] ReportQueryParameters query)
        => Ok(await _service.GetAsync(query));

    /// <summary>Pobiera zgłoszenie po identyfikatorze.</summary>
    [HttpGet("{id:int}")]
    public async Task<ActionResult<ReportDto>> GetById(int id)
        => Ok(await _service.GetByIdAsync(id));

    /// <summary>Tworzy nowe zgłoszenie.</summary>
    [HttpPost]
    public async Task<ActionResult<ReportDto>> Create([FromBody] CreateReportDto dto)
    {
        var created = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    /// <summary>Zmienia status zgłoszenia (z walidacją dozwolonych przejść).</summary>
    [HttpPatch("{id:int}/status")]
    public async Task<ActionResult<ReportDto>> ChangeStatus(int id, [FromBody] UpdateReportStatusDto dto)
        => Ok(await _service.ChangeStatusAsync(id, dto));

    /// <summary>Usuwa zgłoszenie.</summary>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _service.DeleteAsync(id);
        return NoContent();
    }

    /// <summary>Eksportuje wszystkie zgłoszenia do pliku CSV.</summary>
    [HttpGet("export")]
    public async Task<IActionResult> ExportCsv()
    {
        var reports = await _service.GetAllForExportAsync();
        var bytes = _csv.ExportReports(reports);
        var fileName = $"zgloszenia_{DateTime.UtcNow:yyyyMMdd_HHmm}.csv";
        return File(bytes, "text/csv", fileName);
    }
}
