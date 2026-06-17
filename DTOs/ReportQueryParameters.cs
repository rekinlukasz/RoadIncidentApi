using RoadIncidentApi.Models;

namespace RoadIncidentApi.DTOs;

/// <summary>
/// Parametry zapytania listy zgłoszeń: paginacja oraz filtrowanie.
/// Funkcjonalność dodatkowa: paginacja wyników.
/// </summary>
public class ReportQueryParameters
{
    private const int MaxPageSize = 100;
    private int _pageSize = 10;
    private int _page = 1;

    /// <summary>Numer strony (od 1).</summary>
    public int Page
    {
        get => _page;
        set => _page = value < 1 ? 1 : value;
    }

    /// <summary>Rozmiar strony (1..100).</summary>
    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = value < 1 ? 1 : (value > MaxPageSize ? MaxPageSize : value);
    }

    /// <summary>Filtr po statusie (opcjonalny).</summary>
    public ReportStatus? Status { get; set; }

    /// <summary>Filtr po rodzaju zdarzenia (opcjonalny).</summary>
    public IncidentType? Type { get; set; }

    /// <summary>Filtr po mieście (opcjonalny, dopasowanie częściowe).</summary>
    public string? City { get; set; }
}
