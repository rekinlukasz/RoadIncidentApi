namespace RoadIncidentApi.Models;

/// <summary>
/// Status obsługi zgłoszenia. Kolejność wartości odzwierciedla naturalny
/// przepływ pracy (workflow) i jest wykorzystywana w regule biznesowej
/// walidującej dozwolone przejścia między statusami.
/// </summary>
public enum ReportStatus
{
    /// <summary>Nowe, nieprzetworzone zgłoszenie.</summary>
    Nowe = 0,

    /// <summary>Zgłoszenie zweryfikowane przez dyspozytora.</summary>
    Zweryfikowane = 1,

    /// <summary>Zgłoszenie w trakcie obsługi (np. służby na miejscu).</summary>
    WTrakcie = 2,

    /// <summary>Zgłoszenie zakończone / usunięte zagrożenie.</summary>
    Zakonczone = 3,

    /// <summary>Zgłoszenie odrzucone (np. fałszywe, duplikat).</summary>
    Odrzucone = 4
}
