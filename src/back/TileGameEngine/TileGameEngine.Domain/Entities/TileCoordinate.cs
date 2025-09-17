using MemoryPack;

namespace TileGameEngine.Domain.Entities;

[MemoryPackable]
public readonly partial struct TileCoordinate(int x, int y) : IEquatable<TileCoordinate>
{
    public readonly int X = x;
    public readonly int Y = y;
    
    public bool Equals(TileCoordinate other)
    {
        return X == other.X && Y == other.Y;
    }

    public override bool Equals(object? obj)
    {
        return obj is TileCoordinate other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(X, Y);
    }

    public static bool operator ==(TileCoordinate left, TileCoordinate right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(TileCoordinate left, TileCoordinate right)
    {
        return !left.Equals(right);
    }

    public override string ToString()
    {
        return $"({X}, {Y})";
    }
}