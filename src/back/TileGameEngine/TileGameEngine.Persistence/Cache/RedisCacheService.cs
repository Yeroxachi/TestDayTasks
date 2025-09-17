using MemoryPack;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using TileGameEngine.Domain.Entities;
using TileGameEngine.Domain.Helpers;
using TileGameEngine.Domain.Interfaces;

namespace TileGameEngine.Persistence.Cache;

public class RedisCacheService : ICacheService
{
    private readonly IDatabase _database;
    private readonly ILogger<RedisCacheService> _logger;
    
    public RedisCacheService(IConnectionMultiplexer redis, ILogger<RedisCacheService> logger)
    {
        _database = redis.GetDatabase();
        _logger = logger;
    }

    public async Task<T?> GetAsync<T>(string key) where T : struct
    {
        try
        {
            var value = await _database.StringGetAsync(key);
            byte[] bytes = value;
            return MemoryPackSerializer.Deserialize<T>(bytes);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting cached value for key: {Key}", key);
            return null;
        }
    }

    public async Task<string?> GetStringAsync(string key)
    {
        try
        {
            return await _database.StringGetAsync(key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting cached string for key: {Key}", key);
            return null;
        }
    }

    public async Task SetAsync<T>(string key, T value) where T : struct
    {
        try
        {
            var serialized = MemoryPackSerializer.Serialize(value);
            await _database.StringSetAsync(key, serialized);
            _logger.LogDebug("Cached value for key: {Key}", key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error caching value for key: {Key}", key);
        }
    }

    public async Task SetStringAsync(string key, string value)
    {
        try
        {
            await _database.StringSetAsync(key, value);
            _logger.LogDebug("Cached string for key: {Key}", key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error caching string for key: {Key}", key);
        }
    }

    public async Task RemoveAsync(string key)
    {
        try
        {
            await _database.KeyDeleteAsync(key);
            _logger.LogDebug("Removed cached value for key: {Key}", key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing cached value for key: {Key}", key);
        }
    }

    public async Task RemoveByPatternAsync(string pattern)
    {
        try
        {
            var server = _database.Multiplexer.GetServer(_database.Multiplexer.GetEndPoints().First());
            var keys = server.Keys(pattern: pattern);

            var tasks = new List<Task>();
            foreach (var key in keys)
            {
                tasks.Add(_database.KeyDeleteAsync(key));
            }

            await Task.WhenAll(tasks);
            _logger.LogDebug("Removed cached values for pattern: {Pattern}", pattern);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing cached values for pattern: {Pattern}", pattern);
        }
    }
    
    public async Task<T?> GetByCoordinateAsync<T>(string geoKey, int x, int y, Func<uint, string> keySelector, Func<T, Area> areaSelector) where T : struct
    {
        try
        {
            var (lon, lat) = GeoHelper.TileToLonLat(x, y);

            var searchRadiusMeters = Math.Max(GeoHelper.MinSearch, Math.Max(Math.Abs(lon), Math.Abs(lat)) * GeoHelper.DegreeInMeters);

            var members = await _database.GeoRadiusAsync(geoKey, lon, lat, searchRadiusMeters)
                .ConfigureAwait(false);

            if (members.Length == 0)
                return null;

            foreach (var member in members)
            {
                if (!uint.TryParse(member.Member, out var id))
                    continue;

                var objKey = keySelector(id);
                var obj = await GetAsync<T>(objKey).ConfigureAwait(false);

                if (obj == null)
                    continue;

                var area = areaSelector((T)obj);
                if (area.IntersectsWith(new Area(x, y, x, y)))
                    return obj;
            }

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while performing GetByCoordinateAsync at ({X},{Y})", x, y);
            return null;
        }
    }

    public async Task GeoAddAsync(string geoKey, double lon, double lat, string key)
    {
        await _database.GeoAddAsync(geoKey, lon, lat, key);
    }

    public async Task<GeoPosition?> GeoGetPositionAsync(string geoKey, string key)
    {
        return await _database.GeoPositionAsync(geoKey, key);
    }
}