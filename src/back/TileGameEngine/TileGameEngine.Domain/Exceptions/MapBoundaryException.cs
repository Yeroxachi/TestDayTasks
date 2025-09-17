namespace TileGameEngine.Domain.Exceptions;

public class MapBoundaryException : Exception
{
    public MapBoundaryException(string message) : base(message) { }
    public MapBoundaryException(string message, Exception innerException) : base(message, innerException) { }
}