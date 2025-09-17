using Microsoft.Extensions.Logging;
using TileGameEngine.Domain.Entities;
using TileGameEngine.Domain.Helpers;
using TileGameEngine.Domain.Interfaces;

namespace TileGameEngine.Application.Services;

public class RegionService :  IRegionService
{
    private readonly ILogger<RegionService> _logger;
    private readonly ICacheService _cacheService;
    private readonly IRegionCache _regionCache;

    public RegionService(ILogger<RegionService> logger, ICacheService cacheService, IRegionCache regionCache)
    {
        _logger = logger;
        _cacheService = cacheService;
        _regionCache = regionCache;
    }

    public async Task<Region?> GetRegionIdByCoordinateAsync(TileCoordinate coordinate)
    {
        var region = await _regionCache.GetByCoordinateAsync(coordinate);
        return region;
    }

    public async Task<Region?> GetRegionIdByIdAsync(Guid id)
    {
        var regionKey = CacheKey.GenerateCacheKey(id, CacheKey.Region);
        var region = await _cacheService.GetAsync<Region>(regionKey);
        return region;
    }

    public async Task<bool> CheckTileInRegionAsync(Guid id, TileCoordinate coordinate)
    {
        var regionKey = CacheKey.GenerateCacheKey(id, CacheKey.Region);
        var region = await _cacheService.GetAsync<Region>(regionKey);
        if (region == null)
        {
            return false;
        }
        
        return region.Value.Area.Contains(coordinate.X, coordinate.Y);
    }

    public async Task<Region[]> GetAllRegionsInAreaAsync(Area area)
    {
        var regions = await _regionCache.GetAllObjectsInAreaAsync(area);
        return regions;
    }

    public Task GenerateRegionsAsync(string[] names)
    {
        //TODO Логика автогенераций
        throw new NotImplementedException();
    }
}