using MemoryPack;

namespace TileGameEngine.Domain.Entities;

[MemoryPackable]
public partial struct Region
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public Area Area { get; set; }

    public Region(string name, Area area)
    {
        Id = Guid.NewGuid();
        Name = name;
        Area = area;
    }
}