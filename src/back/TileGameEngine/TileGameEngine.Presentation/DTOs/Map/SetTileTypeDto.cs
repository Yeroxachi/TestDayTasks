namespace TileGameEngine.Presentation.DTOs.Map;

public record SetTileTypeDto : TileCoordinateDto
{
    public string TileType { get; set; } = string.Empty;  
}