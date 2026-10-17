namespace RoadIncidentApi.Exceptions;

/// <summary>
/// Wyjątek sygnalizujący naruszenie reguły biznesowej.
/// Przechwytywany przez middleware i zamieniany na odpowiedź HTTP 400.
/// </summary>
public class DomainException : Exception
{
    public DomainException(string message) : base(message) { }
}
