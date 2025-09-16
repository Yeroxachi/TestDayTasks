namespace TileGameEngine.Domain.Interfaces;

public interface ICacheService
{
    Task<T?> GetAsync<T>(string key) where T : struct;
    Task<string?> GetStringAsync(string key);
    Task SetAsync<T>(string key, T value, TimeSpan expiry) where T : struct;
    Task SetStringAsync(string key, string value, TimeSpan expiry);
    Task RemoveAsync(string key);
    Task RemoveByPatternAsync(string pattern);
}