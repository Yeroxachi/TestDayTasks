using MemoryPack;
using TileGameEngine.Domain.Enums;

namespace TileGameEngine.Domain.Entities;

[MemoryPackable]
public readonly partial struct Tile
{
    public readonly SurfaceType SurfaceType;
        
    public Tile(SurfaceType surfaceType)
    {
        SurfaceType = surfaceType;
    }

    // Жедательно создать отдельную сущность для SurfaceType где в будущем можно заранее прописовать там строить можно или нет 
    public bool CanPlaceObject => SurfaceType == SurfaceType.Plain;
}