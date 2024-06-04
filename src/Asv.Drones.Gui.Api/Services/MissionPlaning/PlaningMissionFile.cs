using Asv.Cfg;
using Asv.Cfg.Json;
using Asv.Common;

namespace Asv.Drones.Gui.Api;

public class PlaningMissionFile : ZipJsonVersionedFile
{
    public static readonly SemVersion Version1_0_0 = new(1, 0, 0);
    public static readonly SemVersion LastVersion = Version1_0_0;
    private const string FileType = "AsvDronesMission";


    public PlaningMissionFile(Stream stream, Guid id, string name) : base(stream, LastVersion, FileType, true)
    {
    }

    public PlaningMissionFile(Stream stream) : base(stream, LastVersion, FileType, false)
    {
    }

    public PlaningMissionModel Load()
    {
        return this.Get<PlaningMissionModel>();
    }

    public void Save(PlaningMissionModel model)
    {
        this.Set(model);
    }
}