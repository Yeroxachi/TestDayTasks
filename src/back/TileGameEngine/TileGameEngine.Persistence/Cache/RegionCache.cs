using MemoryPack;
using StackExchange.Redis;
using TileGameEngine.Domain.Entities;
using TileGameEngine.Domain.Helpers;
using TileGameEngine.Domain.Interfaces;

namespace TileGameEngine.Persistence.Cache;

//TODO Реализовать Параллельную обработку получения данных
public class RegionCache :  IRegionCache
{
    private readonly IDatabase _database;

    public RegionCache(IConnectionMultiplexer redis)
    {
        _database = redis.GetDatabase();
    }
    public async Task<Region?> GetByCoordinateAsync(TileCoordinate coordinate)
    {
        var (lon, lat) = GeoHelper.TileToLonLat(coordinate.X, coordinate.Y);

        var searchRadiusMeters = Math.Max(
            GeoHelper.MinSearch,
            Math.Max(Math.Abs(lon), Math.Abs(lat)) * GeoHelper.DegreeInMeters
        );

        var members = await _database.GeoRadiusAsync(
            CacheKey.Region,
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
                var region = MemoryPackSerializer.Deserialize<Region>(bytes);
                
                if (region.Area.Contains(coordinate))
                    return region;
            }
            catch (Exception e)
            {
                continue;
            }
        }

        return null;
    }

    public async Task<Region[]> GetAllObjectsInAreaAsync(Area area)
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
            return new List<Region>().ToArray();

        var result = new List<Region>();

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
                var region = MemoryPackSerializer.Deserialize<Region>(bytes);
            
                if (region.Area.IntersectsWith(area))
                    result.Add(region);
            }
            catch (Exception e)
            {
                continue;
            }
        }

        return result.ToArray();
    }
}