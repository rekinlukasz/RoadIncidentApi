namespace RoadIncidentApi.Exceptions;

/// <summary>
/// Wyjątek sygnalizujący brak zasobu.
/// Przechwytywany przez middleware i zamieniany na odpowiedź HTTP 404.
/// </summary>
public class NotFoundException : Exception
{
    public NotFoundException(string message) : base(message) { }
}
