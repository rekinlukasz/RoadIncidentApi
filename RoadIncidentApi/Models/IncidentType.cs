namespace RoadIncidentApi.Models;

/// <summary>
/// Rodzaj zgłaszanego zdarzenia drogowego.
/// </summary>
public enum IncidentType
{
    /// <summary>Wypadek drogowy / kolizja.</summary>
    Wypadek = 0,

    /// <summary>Roboty drogowe / remont.</summary>
    RobotyDrogowe = 1,

    /// <summary>Przeszkoda na drodze (np. powalone drzewo, dziura).</summary>
    Przeszkoda = 2,

    /// <summary>Awaria infrastruktury (sygnalizacja, oświetlenie).</summary>
    Awaria = 3,

    /// <summary>Inne zdarzenie.</summary>
    Inne = 4
}
