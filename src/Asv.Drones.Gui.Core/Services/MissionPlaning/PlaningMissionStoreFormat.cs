using Asv.Mavlink;

namespace Asv.Drones.Gui.Core;

public class PlaningMissionStoreFormat : GuidHierarchicalStoreFormat<PlaningMissionFile>
{
    public static IHierarchicalStoreFormat<Guid, PlaningMissionFile> Instance { get; } = new PlaningMissionStoreFormat();

    private PlaningMissionStoreFormat() : base(".asvm")
    {
        
    }

    public override PlaningMissionFile OpenFile(Stream stream)
    {
        return new PlaningMissionFile(stream);
    }

    public override PlaningMissionFile CreateFile(Stream stream, Guid id, string name)
    {
        return new PlaningMissionFile(stream, id, name);
    }

    public override void Dispose()
    {
        
    }
}