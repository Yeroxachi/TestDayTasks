namespace TileGameEngine.Presentation.DTOs.Region;

public record CheckTileInRegionDto
{
    public Guid RegionId { get; init; }
    public int X { get; init; }
    public int Y { get; init; }
}