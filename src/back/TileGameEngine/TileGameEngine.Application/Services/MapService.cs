using TileGameEngine.Domain.Entities;
using TileGameEngine.Domain.Enums;
using TileGameEngine.Domain.Exceptions;
using TileGameEngine.Domain.Interfaces;

namespace TileGameEngine.Application.Services;

public class MapService : IMapService
{
    private Map _map;
    private readonly Lock _lock = new();
    private bool _isInitialized = false;

    public Map CurrentMap
    {
        get
        {
            lock (_lock)
            {
                return _map;
            }
        }
    }

    public int Width => CurrentMap.Width;
    public int Height => CurrentMap.Height;
    public long MemoryUsageBytes => CurrentMap.MemoryUsageBytes;

    public MapService()
    {
        _map = new Map(1, 1);
    }

    public void InitializeMap(int width, int height)
    {
        lock (_lock)
        {
            _map = new Map(width, height);
            _isInitialized = true;
        }
    }

    public void LoadMap(Map map)
    {
        lock (_lock)
        {
            _map = map;
            _isInitialized = true;
        }
    }

    public SurfaceType GetTileType(int x, int y)
    {
        ValidateInitialized();
        return CurrentMap.GetTileType(x, y);
    }

    public SurfaceType GetTileType(TileCoordinate coordinate)
    {
        ValidateInitialized();
        return CurrentMap.GetTileType(coordinate);
    }

    public void SetTileType(int x, int y, SurfaceType tileType)
    {
        ValidateInitialized();
        
        lock (_lock)
        {
            
            _map.SetTileType(x, y, tileType);
        }
    }

    public void SetTileType(TileCoordinate coordinate, SurfaceType tileType)
    {
        SetTileType(coordinate.X, coordinate.Y, tileType);
    }

    public bool CanPlaceObject(int x, int y)
    {
        ValidateInitialized();
        return CurrentMap.CanPlaceObject(x, y);
    }

    public bool CanPlaceObject(TileCoordinate coordinate)
    {
        return CanPlaceObject(coordinate.X, coordinate.Y);
    }

    public bool CanPlaceObjectInArea(Area area)
    {
        ValidateInitialized();
        return CurrentMap.CanPlaceObjectInArea(area);
    }

    public void FillArea(Area area, SurfaceType tileType)
    {
        ValidateInitialized();
        
        lock (_lock)
        {
            _map.FillArea(area, tileType);
            
        }
    }

    public bool IsValidCoordinate(int x, int y)
    {
        ValidateInitialized();
        return x >= 0 && x < Width && y >= 0 && y < Height;
    }

    public bool IsValidCoordinate(TileCoordinate coordinate)
    {
        return IsValidCoordinate(coordinate.X, coordinate.Y);
    }

    private void ValidateInitialized()
    {
        if (!_isInitialized)
        {
            throw new MapNotInitializedException("Map has not been initialized. Call InitializeMap() first.");
        }
    }
}