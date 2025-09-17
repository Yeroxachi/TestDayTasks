using Microsoft.Extensions.Logging;
using TileGameEngine.Domain.Entities;
using TileGameEngine.Domain.Helpers;
using TileGameEngine.Domain.Interfaces;

namespace TileGameEngine.Application.Services;

public class ObjectService :  IObjectService
{
    private readonly Map _map;
    private readonly ILogger<ObjectService> _logger;
    private readonly ICacheService _cacheService;
    private readonly IGameObjectCache _gameObjectCache;

    public ObjectService(ref Map map, ILogger<ObjectService> logger, ICacheService cacheService,  IGameObjectCache gameObjectCache)
    {
        _map = map;
        _logger = logger;
        _cacheService = cacheService;
        _gameObjectCache = gameObjectCache;
    }

    public async Task AddObjectAsync(GameObject obj)
    {
        var objKey = CacheKey.GenerateCacheKey(obj.Id, CacheKey.GameObject);
        var objArea = obj.GetBoundingArea();
        if (_map.CanPlaceObjectInArea(objArea))
        {
            var objects = await GetAllGameObjectsInAreaAsync(objArea);
            if (objects.Length != 0)
            {
                throw new InvalidOperationException("Object already exists in this Area");
            }

            await _cacheService.SetAsync(objKey, obj).ConfigureAwait(false);;
            var centroidX = obj.Position.X + (obj.Width - 1) / 2; 
            var centroidY = obj.Position.Y + (obj.Height - 1) / 2; 
            var (lon, lat) = GeoHelper.TileToLonLat(centroidX, centroidY); 
            await _cacheService.GeoAddAsync(CacheKey.GameObject, lon, lat, obj.Id.ToString()).ConfigureAwait(false);
        }
    }

    public async Task<GameObject?> GetGameObjectAsync(Guid id)
    {
        var objKey = CacheKey.GenerateCacheKey(id, CacheKey.GameObject);
        var gameObj = await _cacheService.GetAsync<GameObject>(objKey);
        return gameObj;
    }

    public async Task DeleteGameObjectAsync(Guid id)
    {
        var objKey = CacheKey.GenerateCacheKey(id, CacheKey.GameObject);
        await _cacheService.RemoveAsync(objKey);
    }

    public async Task<GameObject?> GetGameObjectByCoordinateAsync(TileCoordinate coordinate)
    {
        var gameObj = await _gameObjectCache.GetByCoordinateAsync(coordinate);
        return gameObj;
    }

    public async Task<bool> CheckGameObjectInAreaAsync(Guid id, Area area)
    {
        var objKey = CacheKey.GenerateCacheKey(id, CacheKey.GameObject);
        var gameObj = await _cacheService.GetAsync<GameObject>(objKey);
        if (gameObj.HasValue)
        {
            return area.IntersectsWith(gameObj.Value.GetBoundingArea());
        }
        return false;
    }

    public async Task<GameObject[]> GetAllGameObjectsInAreaAsync(Area area)
    {
        var gameObjects = await _gameObjectCache.GetAllObjectsInAreaAsync(area);
        return gameObjects;
    }
}