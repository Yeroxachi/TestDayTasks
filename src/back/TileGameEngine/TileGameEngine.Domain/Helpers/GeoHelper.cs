namespace TileGameEngine.Domain.Helpers;

public static class GeoHelper
{
    private const double OriginLon = 0.0;
    private const double OriginLat = 0.0;
    private const double LonPerTile = 0.00001;
    private const double LatPerTile = 0.00001;
    public const double MinSearch = 10;
    public const double DegreeInMeters = 111320;
    public static (double lon, double lat) TileToLonLat(int x, int y)
    {
        var lon = OriginLon + x * LonPerTile;
        var lat = OriginLat + y * LatPerTile;
        return (lon, lat);
    }
}