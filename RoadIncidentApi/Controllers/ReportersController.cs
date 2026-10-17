using Microsoft.AspNetCore.Mvc;
using RoadIncidentApi.DTOs;
using RoadIncidentApi.Services;

namespace RoadIncidentApi.Controllers;

[ApiController]
[Route("api/reporters")]
public class ReportersController : ControllerBase
{
    private readonly IReporterService _service;

    public ReportersController(IReporterService service) => _service = service;

    /// <summary>Pobiera listę wszystkich zgłaszających.</summary>
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ReporterDto>>> GetAll()
        => Ok(await _service.GetAllAsync());

    /// <summary>Pobiera zgłaszającego po identyfikatorze.</summary>
    [HttpGet("{id:int}")]
    public async Task<ActionResult<ReporterDto>> GetById(int id)
        => Ok(await _service.GetByIdAsync(id));

    /// <summary>Tworzy nowego zgłaszającego.</summary>
    [HttpPost]
    public async Task<ActionResult<ReporterDto>> Create([FromBody] CreateReporterDto dto)
    {
        var created = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    /// <summary>Usuwa zgłaszającego (kaskadowo usuwa jego zgłoszenia).</summary>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _service.DeleteAsync(id);
        return NoContent();
    }
}
