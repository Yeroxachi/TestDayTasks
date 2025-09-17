namespace TileGameEngine.Presentation.DTOs.Map;

public record FillAreaDto
{
    public int X { get; set; }
    public int Y { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public string TileType { get; set; } = string.Empty;
}