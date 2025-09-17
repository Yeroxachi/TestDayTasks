using StackExchange.Redis;
using TileGameEngine.Domain.Entities;

namespace TileGameEngine.Domain.Interfaces;

public interface ICacheService
{
    Task<T?> GetAsync<T>(string key) where T : struct;
    Task<string?> GetStringAsync(string key);
    Task SetAsync<T>(string key, T value) where T : struct;
    Task SetStringAsync(string key, string value);
    Task RemoveAsync(string key);
    Task RemoveByPatternAsync(string pattern);
    Task GeoAddAsync(string geoKey, double lon, double lat, string member);
    Task<GeoPosition?> GeoGetPositionAsync(string geoKey, string member);
    Task<T?> GetByCoordinateAsync<T>(string geoKey, int x, int y, Func<uint, string> keySelector,
        Func<T, Area> areaSelector) where T : struct;
}