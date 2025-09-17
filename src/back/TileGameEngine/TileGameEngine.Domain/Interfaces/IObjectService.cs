using TileGameEngine.Domain.Entities;

namespace TileGameEngine.Domain.Interfaces;

public interface IObjectService
{
    Task AddObjectAsync(GameObject obj);
    Task<GameObject?> GetGameObjectAsync(Guid id);
    Task DeleteGameObjectAsync(Guid id);
    Task<GameObject?> GetGameObjectByCoordinateAsync(TileCoordinate coordinate);
    Task<bool> CheckGameObjectInAreaAsync(Guid id, Area area);
    Task<GameObject[]> GetAllGameObjectsInAreaAsync(Area area);
}