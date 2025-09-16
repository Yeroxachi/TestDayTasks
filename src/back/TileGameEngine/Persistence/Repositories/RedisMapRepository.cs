using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using TileGameEngine.Domain.Entities;
using TileGameEngine.Domain.Enums;
using TileGameEngine.Domain.Interfaces;

namespace Persistence.Repositories;

public class RedisMapRepository(IConnectionMultiplexer redis, ILogger<RedisMapRepository> logger)
    : IMapRepository
{
    private readonly IDatabase _database = redis.GetDatabase();
    private const string MapTilesKey = "map:tiles";
    private const string MapInfoKey = "map:info";

    public async Task<SurfaceType> GetTileTypeAsync(int x, int y)
    {
        try
        {
            var field = $"{x}:{y}";
            var value = await _database.HashGetAsync(MapTilesKey, field);
                
            if (!value.HasValue)
            {
                logger.LogWarning("Tile ({X}, {Y}) not found in Redis, returning default", x, y);
                return SurfaceType.Plain; // Default value
            }
                
            return (SurfaceType)(int)value;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving tile ({X}, {Y}) from Redis", x, y);
            throw;
        }
    }

    public async Task SetTileTypeAsync(int x, int y, SurfaceType tileType)
    {
        try
        {
            var field = $"{x}:{y}";
            await _database.HashSetAsync(MapTilesKey, field, (int)tileType);
            logger.LogDebug("Set tile ({X}, {Y}) to {TileType} in Redis", x, y, tileType);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error setting tile ({X}, {Y}) in Redis", x, y);
            throw;
        }
    }

    public async Task SetAreaAsync(Area area, SurfaceType tileType)
    {
        try
        {
            var batch = _database.CreateBatch();
            var tasks = new List<Task>();

            foreach (var coord in area.GetCoordinates())
            {
                var field = $"{coord.X}:{coord.Y}";
                tasks.Add(batch.HashSetAsync(MapTilesKey, field, (int)tileType));
            }

            batch.Execute();
            await Task.WhenAll(tasks);

            logger.LogInformation("Set area {Area} to {TileType} in Redis with {TileCount} tiles",
                area, tileType, area.TotalArea);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error setting area {Area} in Redis", area);
            throw;
        }
    }

    public async Task<Map> LoadMapAsync(int width, int height)
    {
        try
        {
            var map = new Map(width, height);
            var allTiles = await _database.HashGetAllAsync(MapTilesKey);

            foreach (var tile in allTiles)
            {
                var coords = tile.Name.ToString().Split(':');
                if (coords.Length == 2 &&
                    int.TryParse(coords[0], out var x) &&
                    int.TryParse(coords[1], out var y) &&
                    x >= 0 && x < width && y >= 0 && y < height)
                {
                    var tileType = (SurfaceType)(int)tile.Value;
                    map.SetTileType(x, y, tileType);
                }
            }

            logger.LogInformation("Loaded map {Width}x{Height} with {TileCount} tiles from Redis",
                width, height, allTiles.Length);

            return map;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error loading map from Redis");
            throw;
        }
    }

    public async Task SaveMapAsync(Map map)
    {
        try
        {
            var batch = _database.CreateBatch();
            var tasks = new List<Task>
            {
                batch.HashSetAsync(MapInfoKey, "width", map.Width),
                batch.HashSetAsync(MapInfoKey, "height", map.Height)
            };

            for (var x = 0; x < map.Width; x++)
            {
                for (var y = 0; y < map.Height; y++)
                {
                    var field = $"{x}:{y}";
                    var tileType = map.GetTileType(x, y);
                    tasks.Add(batch.HashSetAsync(MapTilesKey, field, (int)tileType));
                }
            }

            batch.Execute();
            await Task.WhenAll(tasks);

            logger.LogInformation("Saved map {Width}x{Height} to Redis", map.Width, map.Height);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error saving map to Redis");
            throw;
        }
    }

    public async Task<bool> CanPlaceObjectAsync(int x, int y)
    {
        var tileType = await GetTileTypeAsync(x, y);
        return tileType == SurfaceType.Plain;
    }

    public async Task<bool> CanPlaceObjectInAreaAsync(Area area)
    {
        var tasks = area.GetCoordinates().Select(coord => CanPlaceObjectAsync(coord.X, coord.Y)).ToList();

        var results = await Task.WhenAll(tasks);
        return results.All(result => result);
    }
}