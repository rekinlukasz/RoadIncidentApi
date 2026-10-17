using System.Text.Json;
using RoadIncidentApi.Exceptions;

namespace RoadIncidentApi.Middleware;

/// <summary>
/// Globalny middleware przechwytujący wyjątki i zamieniający je
/// na czytelne odpowiedzi JSON (zgodnie z formatem ProblemDetails).
/// </summary>
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (DomainException ex)
        {
            await WriteProblem(context, StatusCodes.Status400BadRequest,
                "Naruszenie reguły biznesowej", ex.Message);
        }
        catch (NotFoundException ex)
        {
            await WriteProblem(context, StatusCodes.Status404NotFound,
                "Nie znaleziono zasobu", ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Nieobsłużony wyjątek.");
            await WriteProblem(context, StatusCodes.Status500InternalServerError,
                "Błąd serwera", "Wystąpił nieoczekiwany błąd.");
        }
    }

    private static Task WriteProblem(HttpContext context, int status, string title, string detail)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = status;

        var payload = JsonSerializer.Serialize(new
        {
            status,
            title,
            detail
        });

        return context.Response.WriteAsync(payload);
    }
}
