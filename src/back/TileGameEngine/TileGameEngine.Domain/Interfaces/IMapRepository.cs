using TileGameEngine.Domain.Entities;
using TileGameEngine.Domain.Enums;

namespace TileGameEngine.Domain.Interfaces;

public interface IMapRepository
{
    Task<SurfaceType> GetTileTypeAsync(int x, int y);
    Task SetTileTypeAsync(int x, int y, SurfaceType tileType);
    Task SetAreaAsync(Area area, SurfaceType tileType);
    Task<Map> LoadMapAsync(int width, int height);
    Task SaveMapAsync(Map map);
    Task<bool> CanPlaceObjectAsync(int x, int y);
    Task<bool> CanPlaceObjectInAreaAsync(Area area);
}