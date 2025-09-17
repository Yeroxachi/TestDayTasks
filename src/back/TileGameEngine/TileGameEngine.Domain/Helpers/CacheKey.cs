namespace TileGameEngine.Domain.Helpers;

public static class CacheKey
{
    public const string GameObject = "object";
    public const string Tile = "tile";
    public const string Map = "map";
    public const string Region = "region";
    public static string GenerateCacheKey(Guid id, string keyName)
    {
        return $"{keyName}:{id}";
    }
}