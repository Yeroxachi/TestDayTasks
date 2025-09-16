using TileGameEngine.Domain.Entities;

namespace TileGameEngine.Domain.Interfaces;

public interface IGameObjectRepository : IDisposable
{
    Task AddOrUpdateAsync(GameObject obj);
    Task<GameObject?> GetByIdAsync(uint id);
    Task<bool> RemoveAsync(uint id);
    Task<GameObject?> GetByCoordinateAsync(int x, int y);
    Task<IEnumerable<GameObject>> GetInAreaAsync(Area area);
    Task MoveObjectAsync(uint id, Domain.Entities.TileCoordinate newPosition);
}