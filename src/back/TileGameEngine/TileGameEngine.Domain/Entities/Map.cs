using MemoryPack;
using TileGameEngine.Domain.Enums;
using TileGameEngine.Domain.Exceptions;

namespace TileGameEngine.Domain.Entities;

[MemoryPackable]
public partial struct Map
{
    private readonly byte[] _tiles;
    private readonly int _width;
    private readonly int _height;
    private readonly Lock _lock = new Lock();

    public int Width => _width;
    public int Height => _height;
    public long MemoryUsageBytes => _tiles?.Length ?? 0;

    public Map(int width, int height)
    {
        if (width <= 0 || height <= 0)
            throw new ArgumentException("Width and height must be positive");

        _width = width;
        _height = height;
        _tiles = new byte[width * height];
        
        Array.Fill(_tiles, (byte)SurfaceType.Plain);
    }
    
    public SurfaceType GetTileType(int x, int y)
    {
        if (!IsValidCoordinate(x, y))
            throw new MapBoundaryException($"Coordinates ({x}, {y}) are out of bounds");

        lock (_lock)
        {
            return (SurfaceType)_tiles[y * _width + x];
        }
    }

    public SurfaceType GetTileType(TileCoordinate coordinate) => GetTileType(coordinate.X, coordinate.Y);

    public void SetTileType(int x, int y, SurfaceType tileType)
    {
        if (!IsValidCoordinate(x, y))
            throw new MapBoundaryException($"Coordinates ({x}, {y}) are out of bounds");

        lock (_lock)
        {
            _tiles[y * _width + x] = (byte)tileType;
        }
    }

    public void SetTileType(TileCoordinate coordinate, SurfaceType tileType) =>
        SetTileType(coordinate.X, coordinate.Y, tileType);

    public bool CanPlaceObject(int x, int y)
    {
        var tileType = GetTileType(x, y);
        return tileType == SurfaceType.Plain;
    }

    public bool CanPlaceObjectInArea(Area area)
    {
        var coords = area.GetCoordinates();
        foreach (var tile in coords)
        {
            if (!CanPlaceObject(tile.X, tile.Y))
            {
                return false;
            }
        }

        return true;
    }

    public void FillArea(Area area, SurfaceType tileType)
    {
        lock (_lock)
        {
            foreach (var coord in area.GetCoordinates())
            {
                if (IsValidCoordinate(coord.X, coord.Y))
                {
                    _tiles[coord.Y * _width + coord.X] = (byte)tileType;
                }
            }
        }
    }

    public static Map FromTileArray(SurfaceType[,] tiles)
    {
        var width = tiles.GetLength(0);
        var height = tiles.GetLength(1);
        var map = new Map(width, height);

        for (var x = 0; x < width; x++)
        {
            for (var y = 0; y < height; y++)
            {
                map.SetTileType(x, y, tiles[x, y]);
            }
        }

        return map;
    }

    public static Map FromTileList(List<(int x, int y, SurfaceType type)> tiles, int width, int height)
    {
        var map = new Map(width, height);

        foreach (var (x, y, type) in tiles)
        {
            map.SetTileType(x, y, type);
        }

        return map;
    }

    private bool IsValidCoordinate(int x, int y)
    {
        return x >= 0 && x < _width && y >= 0 && y < _height;
    }
}