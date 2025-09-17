namespace TileGameEngine.Presentation.DTOs.GameObject;

public record CheckObjectInAreaDto
{
    public Guid Id { get; init; }
    public AreaDto? Area { get; init; }
}