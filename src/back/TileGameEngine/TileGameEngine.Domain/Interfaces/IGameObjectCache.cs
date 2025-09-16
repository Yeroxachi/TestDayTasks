using TileGameEngine.Domain.Entities;

namespace TileGameEngine.Domain.Interfaces;

public interface IGameObjectCache
{
    Task<GameObject?> GetByCoordinateAsync(TileCoordinate coordinate);
    Task<GameObject[]> GetAllObjectsInAreaAsync(Area area);
}