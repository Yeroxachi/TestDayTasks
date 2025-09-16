namespace TileGameEngine.Domain.Helpers;

public static class CacheKey
{
    public const string GameObject = $"object";
    public static string GenerateGameObjCacheKey(Guid id)
    {
        return $"{GameObject}:{id}";
    }
}