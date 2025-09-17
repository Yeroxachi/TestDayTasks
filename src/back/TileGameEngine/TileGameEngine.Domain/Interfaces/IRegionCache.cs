using TileGameEngine.Domain.Entities;

namespace TileGameEngine.Domain.Interfaces;

public interface IRegionCache
{
    Task<Region?> GetByCoordinateAsync(TileCoordinate coordinate);
    Task<Region[]> GetAllObjectsInAreaAsync(Area area);
}