using TileGameEngine.Domain.Entities;
using TileGameEngine.Domain.Enums;

namespace TileGameEngine.Domain.Interfaces;

public interface IMapService
{
    int Width { get; }
    int Height { get; }
    long MemoryUsageBytes { get; }
    
    SurfaceType GetTileType(int x, int y);
    SurfaceType GetTileType(TileCoordinate coordinate);
    void SetTileType(int x, int y, SurfaceType tileType);
    void SetTileType(TileCoordinate coordinate, SurfaceType tileType);
    bool CanPlaceObject(int x, int y);
    bool CanPlaceObject(TileCoordinate coordinate);
    bool CanPlaceObjectInArea(Area area);
    void FillArea(Area area, SurfaceType tileType);
    bool IsValidCoordinate(int x, int y);
    bool IsValidCoordinate(TileCoordinate coordinate);
}