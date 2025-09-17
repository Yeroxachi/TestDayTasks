namespace TileGameEngine.Presentation.DTOs.GameObject;

public record CreateGameObjectDto
{
    public int X { get; init; }
    public int Y { get; init; }
    public int Width { get; init; }
    public int Height { get; init; }
}