using TileGameEngine.Domain.Entities;

namespace TileGameEngine.Tests.EntityTests;

public class AreaTests
{
    [Fact]
    public void Constructor_WithNormalCoordinates_CreatesAreaCorrectly()
    {
        // Arrange & Act
        var area = new Area(1, 2, 5, 8);

        // Assert
        Assert.Equal(1, area.X1);
        Assert.Equal(2, area.Y1);
        Assert.Equal(5, area.X2);
        Assert.Equal(8, area.Y2);
    }

    [Fact]
    public void Constructor_WithSwappedCoordinates_NormalizesCorrectly()
    {
        // Arrange & Act
        var area = new Area(5, 8, 1, 2);

        // Assert
        Assert.Equal(1, area.X1);
        Assert.Equal(2, area.Y1);
        Assert.Equal(5, area.X2);
        Assert.Equal(8, area.Y2);
    }

    [Fact]
    public void Constructor_WithMixedCoordinates_NormalizesCorrectly()
    {
        // Arrange & Act
        var area = new Area(5, 2, 1, 8);

        // Assert
        Assert.Equal(1, area.X1);
        Assert.Equal(2, area.Y1);
        Assert.Equal(5, area.X2);
        Assert.Equal(8, area.Y2);
    }

    [Fact]
    public void Constructor_WithSameCoordinates_CreatesSingleTileArea()
    {
        // Arrange & Act
        var area = new Area(3, 3, 3, 3);

        // Assert
        Assert.Equal(3, area.X1);
        Assert.Equal(3, area.Y1);
        Assert.Equal(3, area.X2);
        Assert.Equal(3, area.Y2);
    }

    [Theory]
    [InlineData(0, 0, 4, 4, 5)]
    [InlineData(1, 1, 1, 1, 1)]
    [InlineData(0, 0, 9, 0, 10)]
    [InlineData(0, 0, 0, 9, 1)]
    public void Width_CalculatesCorrectly(int x1, int y1, int x2, int y2, int expectedWidth)
    {
        // Arrange & Act
        var area = new Area(x1, y1, x2, y2);

        // Assert
        Assert.Equal(expectedWidth, area.Width);
    }

    [Theory]
    [InlineData(0, 0, 4, 4, 5)]
    [InlineData(1, 1, 1, 1, 1)]
    [InlineData(0, 0, 0, 9, 10)]
    [InlineData(0, 0, 9, 0, 1)]
    public void Height_CalculatesCorrectly(int x1, int y1, int x2, int y2, int expectedHeight)
    {
        // Arrange & Act
        var area = new Area(x1, y1, x2, y2);

        // Assert
        Assert.Equal(expectedHeight, area.Height);
    }

    [Theory]
    [InlineData(0, 0, 4, 4, 25)]
    [InlineData(1, 1, 1, 1, 1)]
    [InlineData(0, 0, 9, 9, 100)]
    [InlineData(2, 3, 5, 7, 20)]
    public void TotalArea_CalculatesCorrectly(int x1, int y1, int x2, int y2, int expectedArea)
    {
        // Arrange & Act
        var area = new Area(x1, y1, x2, y2);

        // Assert
        Assert.Equal(expectedArea, area.TotalArea);
    }

    [Theory]
    [InlineData(2, 2)]
    [InlineData(1, 1)]
    [InlineData(3, 3)]
    [InlineData(1, 3)]
    [InlineData(3, 1)]
    public void Contains_WithCoordinateInside_ReturnsTrue(int x, int y)
    {
        // Arrange
        var area = new Area(1, 1, 3, 3);
        var coord = new TileCoordinate(x, y);

        // Act
        var contains = area.Contains(coord);

        // Assert
        Assert.True(contains);
    }

    [Theory]
    [InlineData(0, 0)]
    [InlineData(4, 4)]
    [InlineData(1, 0)]
    [InlineData(0, 1)]
    [InlineData(-1, -1)]
    public void Contains_WithCoordinateOutside_ReturnsFalse(int x, int y)
    {
        // Arrange
        var area = new Area(1, 1, 3, 3);
        var coord = new TileCoordinate(x, y);

        // Act
        var contains = area.Contains(coord);

        // Assert
        Assert.False(contains);
    }

    [Fact]
    public void Contains_WithXYOverload_WorksCorrectly()
    {
        // Arrange
        var area = new Area(5, 5, 10, 10);

        // Act & Assert
        Assert.True(area.Contains(7, 7));
        Assert.True(area.Contains(5, 5));
        Assert.True(area.Contains(10, 10));
        Assert.False(area.Contains(4, 7));
        Assert.False(area.Contains(11, 7));
    }

    [Fact]
    public void GetCoordinates_ReturnsAllCoordinatesInArea()
    {
        // Arrange
        var area = new Area(1, 1, 2, 2);

        // Act
        var coordinates = area.GetCoordinates().ToList();

        // Assert
        Assert.Equal(4, coordinates.Count);
        Assert.Contains(new TileCoordinate(1, 1), coordinates);
        Assert.Contains(new TileCoordinate(1, 2), coordinates);
        Assert.Contains(new TileCoordinate(2, 1), coordinates);
        Assert.Contains(new TileCoordinate(2, 2), coordinates);
    }

    [Fact]
    public void GetCoordinates_ForSingleTileArea_ReturnsSingleCoordinate()
    {
        // Arrange
        var area = new Area(5, 5, 5, 5);

        // Act
        var coordinates = area.GetCoordinates().ToList();

        // Assert
        Assert.Single(coordinates);
        Assert.Equal(new TileCoordinate(5, 5), coordinates[0]);
    }

    [Fact]
    public void GetCoordinates_ForLargeArea_ReturnsCorrectCount()
    {
        // Arrange
        var area = new Area(0, 0, 9, 9);

        // Act
        var coordinatesCount = area.GetCoordinates().Count();

        // Assert
        Assert.Equal(100, coordinatesCount);
    }

    [Fact]
    public void IntersectsWith_OverlappingAreas_ReturnsTrue()
    {
        // Arrange
        var area1 = new Area(1, 1, 5, 5);
        var area2 = new Area(3, 3, 7, 7);

        // Act & Assert
        Assert.True(area1.IntersectsWith(area2));
        Assert.True(area2.IntersectsWith(area1));
    }

    [Fact]
    public void IntersectsWith_NonOverlappingAreas_ReturnsFalse()
    {
        // Arrange
        var area1 = new Area(1, 1, 3, 3);
        var area2 = new Area(5, 5, 7, 7);

        // Act & Assert
        Assert.False(area1.IntersectsWith(area2));
        Assert.False(area2.IntersectsWith(area1));
    }

    [Fact]
    public void IntersectsWith_TouchingEdges_ReturnsTrue()
    {
        // Arrange
        var area1 = new Area(1, 1, 3, 3);
        var area2 = new Area(3, 3, 5, 5);

        // Act & Assert
        Assert.True(area1.IntersectsWith(area2));
        Assert.True(area2.IntersectsWith(area1));
    }

    [Fact]
    public void IntersectsWith_AdjacentAreas_ReturnsFalse()
    {
        // Arrange
        var area1 = new Area(1, 1, 3, 3);
        var area2 = new Area(4, 1, 6, 3);

        // Act & Assert
        Assert.False(area1.IntersectsWith(area2));
        Assert.False(area2.IntersectsWith(area1));
    }

    [Fact]
    public void IntersectsWith_OneAreaInsideAnother_ReturnsTrue()
    {
        // Arrange
        var area1 = new Area(1, 1, 10, 10);
        var area2 = new Area(3, 3, 5, 5);

        // Act & Assert
        Assert.True(area1.IntersectsWith(area2));
        Assert.True(area2.IntersectsWith(area1));
    }

    [Fact]
    public void IntersectsWith_SameArea_ReturnsTrue()
    {
        // Arrange
        var area1 = new Area(1, 1, 5, 5);
        var area2 = new Area(1, 1, 5, 5);

        // Act & Assert
        Assert.True(area1.IntersectsWith(area2));
    }
    
    
    //Не особо полезный тест
    [Fact]
    public void Area_WithNegativeCoordinates_WorksCorrectly()
    {
        // Arrange & Act
        var area = new Area(-5, -5, 5, 5);

        // Assert
        Assert.Equal(-5, area.X1);
        Assert.Equal(-5, area.Y1);
        Assert.Equal(5, area.X2);
        Assert.Equal(5, area.Y2);
        Assert.Equal(11, area.Width);
        Assert.Equal(11, area.Height);
        Assert.Equal(121, area.TotalArea);
    }
}