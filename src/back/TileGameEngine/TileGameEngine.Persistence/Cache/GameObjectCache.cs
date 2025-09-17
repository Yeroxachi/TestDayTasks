using MemoryPack;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using TileGameEngine.Domain.Entities;
using TileGameEngine.Domain.Helpers;
using TileGameEngine.Domain.Interfaces;

namespace TileGameEngine.Persistence.Cache;

//TODO Реализовать Параллельную обработку получения данных
public class GameObjectCache : IGameObjectCache
{
    private readonly IDatabase _database;
    private readonly ILogger<RedisCacheService> _logger;

    public GameObjectCache(IConnectionMultiplexer redis, ILogger<RedisCacheService> logger)
    {
        _database = redis.GetDatabase();
        _logger = logger;
    }

    public async Task<GameObject?> GetByCoordinateAsync(TileCoordinate coordinate)
    {
        var (lon, lat) = GeoHelper.TileToLonLat(coordinate.X, coordinate.Y);

        var searchRadiusMeters = Math.Max(
            GeoHelper.MinSearch,
            Math.Max(Math.Abs(lon), Math.Abs(lat)) * GeoHelper.DegreeInMeters
        );

        var members = await _database.GeoRadiusAsync(
            CacheKey.GameObject,
            lon,
            lat,
            searchRadiusMeters
        ).ConfigureAwait(false);

        if (members.Length == 0)
            return null;

        foreach (var member in members)
        {
            var key = member.Member.ToString();
            if (string.IsNullOrEmpty(key))
            {
                continue;
            }
            try
            {
                var value = await _database.StringGetAsync(key);
                byte[] bytes = value;
                var obj = MemoryPackSerializer.Deserialize<GameObject>(bytes);

                var objArea = obj.GetBoundingArea();
                if (objArea.Contains(coordinate))
                    return obj;
            }
            catch (Exception e)
            {
                continue;
            }
        }

        return null;
    }

    public async Task<GameObject[]> GetAllObjectsInAreaAsync(Area area)
    {
        var centerX = (area.X1 + area.X2) / 2;
        var centerY = (area.Y1 + area.Y2) / 2;
        var (lon, lat) = GeoHelper.TileToLonLat(centerX, centerY);
        
        var searchRadiusMeters = Math.Max(
            GeoHelper.MinSearch,
            Math.Max(Math.Abs(lon), Math.Abs(lat)) * GeoHelper.DegreeInMeters
        );

        var members = await _database.GeoRadiusAsync(CacheKey.GameObject, lon, lat, searchRadiusMeters).ConfigureAwait(false);

        if (members.Length == 0)
            return new List<GameObject>().ToArray();

        var result = new List<GameObject>();

        foreach (var member in members)
        {
            var key = member.Member.ToString();
            if (string.IsNullOrEmpty(key))
            {
                continue;
            }

            try
            {
                var value = await _database.StringGetAsync(key);
                byte[] bytes = value;
                var obj = MemoryPackSerializer.Deserialize<GameObject>(bytes);
            
                if (obj.IntersectsWith(area))
                    result.Add(obj);
            }
            catch (Exception e)
            {
                continue;
            }
        }

        return result.ToArray();
    }
}