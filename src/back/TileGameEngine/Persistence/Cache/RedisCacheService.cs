using System.Diagnostics;
using MemoryPack;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using TileGameEngine.Domain.Interfaces;

namespace Persistence.Cache;

public class RedisCacheService(IConnectionMultiplexer redis, ILogger<RedisCacheService> logger)
    : ICacheService
{
        private readonly IDatabase _database = redis.GetDatabase();

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
                logger.LogError(ex, "Error getting cached value for key: {Key}", key);
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
                logger.LogError(ex, "Error getting cached string for key: {Key}", key);
                return null;
            }
        }
        
        public async Task SetAsync<T>(string key, T value, TimeSpan expiry) where T : struct
        {
            try
            {
                var serialized = MemoryPackSerializer.Serialize(value);
                await _database.StringSetAsync(key, serialized, expiry);
                logger.LogDebug("Cached value for key: {Key} with expiry: {Expiry}", key, expiry);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error caching value for key: {Key}", key);
            }
        }
        
        public async Task SetStringAsync(string key, string value, TimeSpan expiry)
        {
            try
            {
                await _database.StringSetAsync(key, value, expiry);
                logger.LogDebug("Cached string for key: {Key} with expiry: {Expiry}", key, expiry);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error caching string for key: {Key}", key);
            }
        }
        
        public async Task RemoveAsync(string key)
        {
            try
            {
                await _database.KeyDeleteAsync(key);
                logger.LogDebug("Removed cached value for key: {Key}", key);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error removing cached value for key: {Key}", key);
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
                logger.LogDebug("Removed cached values for pattern: {Pattern}", pattern);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error removing cached values for pattern: {Pattern}", pattern);
            }
        }
    }