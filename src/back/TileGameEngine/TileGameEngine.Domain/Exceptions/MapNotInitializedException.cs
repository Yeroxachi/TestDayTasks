namespace TileGameEngine.Domain.Exceptions;

public class MapNotInitializedException : Exception
{
    public MapNotInitializedException(string message) : base(message) { }
    public MapNotInitializedException(string message, Exception innerException) : base(message, innerException) { }
}