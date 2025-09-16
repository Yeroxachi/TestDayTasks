using TileGameEngine.Domain.Entities;
using TileGameEngine.Domain.Enums;
using TileGameEngine.Domain.Exceptions;

namespace TileGameEngine.Tests.EntityTests;

public class MapTests
{
    [Fact]
    public void Constructor_WithValidDimensions_CreatesMapWithCorrectSize()
    {
        // Arrange & Act
        var map = new Map(10, 20);

        // Assert
        Assert.Equal(10, map.Width);
        Assert.Equal(20, map.Height);
        Assert.Equal(200, map.MemoryUsageBytes);
    }

    [Theory]
    [InlineData(0, 10)]
    [InlineData(10, 0)]
    [InlineData(-1, 10)]
    [InlineData(10, -1)]
    [InlineData(0, 0)]
    public void Constructor_WithInvalidDimensions_ThrowsArgumentException(int width, int height)
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentException>(() => new Map(width, height));
    }

    [Fact]
    public void Constructor_InitializesAllTilesAsPlain()
    {
        // Arrange
        var map = new Map(5, 5);

        // Act & Assert
        for (int x = 0; x < 5; x++)
        {
            for (int y = 0; y < 5; y++)
            {
                Assert.Equal(SurfaceType.Plain, map.GetTileType(x, y));
            }
        }
    }

    [Fact]
    public void GetTileType_WithValidCoordinates_ReturnsTileType()
    {
        // Arrange
        var map = new Map(10, 10);
        map.SetTileType(5, 5, SurfaceType.Mountains);

        // Act
        var tileType = map.GetTileType(5, 5);

        // Assert
        Assert.Equal(SurfaceType.Mountains, tileType);
    }

    [Fact]
    public void GetTileType_WithTileCoordinate_ReturnsTileType()
    {
        // Arrange
        var map = new Map(10, 10);
        var coord = new TileCoordinate(3, 4);
        map.SetTileType(coord, SurfaceType.Mountains);

        // Act
        var tileType = map.GetTileType(coord);

        // Assert
        Assert.Equal(SurfaceType.Mountains, tileType);
    }

    [Theory]
    [InlineData(-1, 0)]
    [InlineData(0, -1)]
    [InlineData(10, 0)]
    [InlineData(0, 10)]
    [InlineData(10, 10)]
    public void GetTileType_WithInvalidCoordinates_ThrowsMapBoundaryException(int x, int y)
    {
        // Arrange
        var map = new Map(10, 10);

        // Act & Assert
        Assert.Throws<MapBoundaryException>(() => map.GetTileType(x, y));
    }

    [Fact]
    public void SetTileType_WithValidCoordinates_UpdatesTile()
    {
        // Arrange
        var map = new Map(10, 10);

        // Act
        map.SetTileType(5, 5, SurfaceType.Mountains);

        // Assert
        Assert.Equal(SurfaceType.Mountains, map.GetTileType(5, 5));
        Assert.Equal(SurfaceType.Plain, map.GetTileType(5, 4));
    }

    [Fact]
    public void SetTileType_WithTileCoordinate_UpdatesTile()
    {
        // Arrange
        var map = new Map(10, 10);
        var coord = new TileCoordinate(7, 3);

        // Act
        map.SetTileType(coord, SurfaceType.Mountains);

        // Assert
        Assert.Equal(SurfaceType.Mountains, map.GetTileType(coord));
    }

    [Theory]
    [InlineData(-1, 0)]
    [InlineData(0, -1)]
    [InlineData(10, 0)]
    [InlineData(0, 10)]
    public void SetTileType_WithInvalidCoordinates_ThrowsMapBoundaryException(int x, int y)
    {
        // Arrange
        var map = new Map(10, 10);

        // Act & Assert
        Assert.Throws<MapBoundaryException>(() => map.SetTileType(x, y, SurfaceType.Mountains));
    }

    [Fact]
    public void CanPlaceObject_OnPlainTile_ReturnsTrue()
    {
        // Arrange
        var map = new Map(10, 10);

        // Act
        var canPlace = map.CanPlaceObject(5, 5);

        // Assert
        Assert.True(canPlace);
    }

    [Fact]
    public void CanPlaceObject_OnMountainTile_ReturnsFalse()
    {
        // Arrange
        var map = new Map(10, 10);
        map.SetTileType(5, 5, SurfaceType.Mountains);

        // Act
        var canPlace = map.CanPlaceObject(5, 5);

        // Assert
        Assert.False(canPlace);
    }

    [Fact]
    public void CanPlaceObjectInArea_AllPlainTiles_ReturnsTrue()
    {
        // Arrange
        var map = new Map(10, 10);
        var area = new Area(2, 2, 4, 4);

        // Act
        var canPlace = map.CanPlaceObjectInArea(area);

        // Assert
        Assert.True(canPlace);
    }

    [Fact]
    public void CanPlaceObjectInArea_WithMountainTile_ReturnsFalse()
    {
        // Arrange
        var map = new Map(10, 10);
        var area = new Area(2, 2, 4, 4);
        map.SetTileType(3, 3, SurfaceType.Mountains);

        // Act
        var canPlace = map.CanPlaceObjectInArea(area);

        // Assert
        Assert.False(canPlace);
    }

    [Fact]
    public void CanPlaceObjectInArea_EmptyArea_ReturnsTrue()
    {
        // Arrange
        var map = new Map(10, 10);
        var area = new Area(5, 5, 5, 5);

        // Act
        var canPlace = map.CanPlaceObjectInArea(area);

        // Assert
        Assert.True(canPlace);
    }

    [Fact]
    public void FillArea_UpdatesAllTilesInArea()
    {
        // Arrange
        var map = new Map(10, 10);
        var area = new Area(2, 2, 4, 4);

        // Act
        map.FillArea(area, SurfaceType.Mountains);

        // Assert
        for (int x = 2; x <= 4; x++)
        {
            for (int y = 2; y <= 4; y++)
            {
                Assert.Equal(SurfaceType.Mountains, map.GetTileType(x, y));
            }
        }

        // Check tiles outside area remain unchanged
        Assert.Equal(SurfaceType.Plain, map.GetTileType(1, 1));
        Assert.Equal(SurfaceType.Plain, map.GetTileType(5, 5));
    }

    [Fact]
    public void FillArea_WithPartiallyOutOfBoundsArea_OnlyFillsValidTiles()
    {
        // Arrange
        var map = new Map(10, 10);
        var area = new Area(8, 8, 12, 12);

        // Act
        map.FillArea(area, SurfaceType.Mountains);

        // Assert
        Assert.Equal(SurfaceType.Mountains, map.GetTileType(8, 8));
        Assert.Equal(SurfaceType.Mountains, map.GetTileType(9, 9));
        Assert.Throws<MapBoundaryException>(() => map.GetTileType(10, 10));
    }

    [Fact]
    public void FromTileArray_CreatesMapWithCorrectTiles()
    {
        // Arrange
        var tiles = new SurfaceType[3, 3]
        {
            { SurfaceType.Plain, SurfaceType.Mountains, SurfaceType.Plain },
            { SurfaceType.Mountains, SurfaceType.Plain, SurfaceType.Mountains },
            { SurfaceType.Plain, SurfaceType.Mountains, SurfaceType.Plain }
        };

        // Act
        var map = Map.FromTileArray(tiles);

        // Assert
        Assert.Equal(3, map.Width);
        Assert.Equal(3, map.Height);
        Assert.Equal(SurfaceType.Plain, map.GetTileType(0, 0));
        Assert.Equal(SurfaceType.Mountains, map.GetTileType(1, 0));
        Assert.Equal(SurfaceType.Plain, map.GetTileType(2, 0));
        Assert.Equal(SurfaceType.Mountains, map.GetTileType(0, 1));
        Assert.Equal(SurfaceType.Plain, map.GetTileType(1, 1));
        Assert.Equal(SurfaceType.Mountains, map.GetTileType(2, 1));
    }

    [Fact]
    public void FromTileList_CreatesMapWithSpecifiedTiles()
    {
        // Arrange
        var tiles = new List<(int x, int y, SurfaceType type)>
        {
            (0, 0, SurfaceType.Mountains),
            (2, 2, SurfaceType.Mountains),
            (4, 4, SurfaceType.Mountains)
        };

        // Act
        var map = Map.FromTileList(tiles, 5, 5);

        // Assert
        Assert.Equal(5, map.Width);
        Assert.Equal(5, map.Height);
        Assert.Equal(SurfaceType.Mountains, map.GetTileType(0, 0));
        Assert.Equal(SurfaceType.Mountains, map.GetTileType(2, 2));
        Assert.Equal(SurfaceType.Mountains, map.GetTileType(4, 4));
        Assert.Equal(SurfaceType.Plain, map.GetTileType(1, 1)); 
        Assert.Equal(SurfaceType.Plain, map.GetTileType(3, 3));
    }

    [Fact]
    public void FromTileList_WithEmptyList_CreatesMapWithAllPlainTiles()
    {
        // Arrange
        var tiles = new List<(int x, int y, SurfaceType type)>();

        // Act
        var map = Map.FromTileList(tiles, 3, 3);

        // Assert
        for (int x = 0; x < 3; x++)
        {
            for (int y = 0; y < 3; y++)
            {
                Assert.Equal(SurfaceType.Plain, map.GetTileType(x, y));
            }
        }
    }

    [Fact]
    public void Map_ThreadSafety_ConcurrentReadWrite()
    {
        // Arrange
        var map = new Map(100, 100);
        var tasks = new List<Task>();
        var random = new Random(42);

        // Act
        // Multiple writers
        for (var i = 0; i < 10; i++)
        {
            var taskId = i;
            tasks.Add(Task.Run(() =>
            {
                for (var j = 0; j < 100; j++)
                {
                    var x = random.Next(0, 100);
                    var y = random.Next(0, 100);
                    map.SetTileType(x, y, taskId % 2 == 0 ? SurfaceType.Mountains : SurfaceType.Plain);
                }
            }));
        }

        // Multiple readers
        for (var i = 0; i < 10; i++)
        {
            tasks.Add(Task.Run(() =>
            {
                for (var j = 0; j < 100; j++)
                {
                    var x = random.Next(0, 100);
                    var y = random.Next(0, 100);
                    var _ = map.GetTileType(x, y);
                }
            }));
        }

        // Assert
        Assert.True(Task.WaitAll(tasks.ToArray(), TimeSpan.FromSeconds(5)));
    }

    [Fact]
    public void MemoryUsageBytes_ReturnsCorrectSize()
    {
        // Arrange & Act
        var map1 = new Map(10, 10);
        var map2 = new Map(20, 20);
        var map3 = new Map(100, 100);

        // Assert
        Assert.Equal(100, map1.MemoryUsageBytes); // 10 * 10 * 1 byte
        Assert.Equal(400, map2.MemoryUsageBytes); // 20 * 20 * 1 byte
        Assert.Equal(10000, map3.MemoryUsageBytes); // 100 * 100 * 1 byte
    }
}