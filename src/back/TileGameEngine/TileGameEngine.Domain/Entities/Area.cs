using MemoryPack;

namespace TileGameEngine.Domain.Entities;

[MemoryPackable]
public readonly partial struct Area(int x1, int y1, int x2, int y2)
{
    public int X1 { get; } = Math.Min(x1, x2);
    public int Y1 { get; } = Math.Min(y1, y2);
    public int X2 { get; } = Math.Max(x1, x2);
    public int Y2 { get; } = Math.Max(y1, y2);
    
    public int Width => X2 - X1 + 1;
    public int Height => Y2 - Y1 + 1;
    public int TotalArea => Width * Height;
    
    public bool Contains(TileCoordinate coordinate)
    {
        return coordinate.X >= X1 && coordinate.X <= X2 && 
               coordinate.Y >= Y1 && coordinate.Y <= Y2;
    }
        
    public bool Contains(int x, int y) => Contains(new TileCoordinate(x, y));
        
    public IEnumerable<TileCoordinate> GetCoordinates()
    {
        for (var x = X1; x <= X2; x++)
        {
            for (var y = Y1; y <= Y2; y++)
            {
                yield return new TileCoordinate(x, y);
            }
        }
    }
}