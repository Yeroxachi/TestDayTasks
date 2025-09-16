using MemoryPack;

namespace TileGameEngine.Domain.Entities;

[MemoryPackable]
public partial class GameObject
{
    public uint Id { get; }
    public TileCoordinate Position { get; private set; }
    public int Width { get; }
    public int Height { get; }
    public string Type { get; }

    public GameObject(uint id, TileCoordinate position, int width, int height, string type)
    {
        Id = id;
        Position = position;
        Width = width;
        Height = height;
        Type = type ?? throw new ArgumentNullException(nameof(type));
    }

    public void MoveTo(TileCoordinate newPosition)
    {
        Position = newPosition;
    }

    public Area GetBoundingArea()
    {
        return new Area(Position.X, Position.Y,
            Position.X + Width - 1, Position.Y + Height - 1);
    }

    public bool IntersectsWith(Area area)
    {
        var objectArea = GetBoundingArea();

        return !(objectArea.X2 < area.X1 || objectArea.X1 > area.X2 ||
                 objectArea.Y2 < area.Y1 || objectArea.Y1 > area.Y2);
    }
}