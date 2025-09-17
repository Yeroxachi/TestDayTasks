using TileGameEngine.Domain.Entities;

namespace TileGameEngine.Domain.Interfaces;

public interface IRegionService
{
    Task<Region?> GetRegionIdByCoordinateAsync(TileCoordinate coordinate);
    Task<Region?> GetRegionIdByIdAsync(Guid id);
    Task<bool> CheckTileInRegionAsync(Guid id, TileCoordinate coordinate);
    Task<Region[]> GetAllRegionsInAreaAsync(Area area);
    Task GenerateRegionsAsync(string[] names);
}