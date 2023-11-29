using System.ComponentModel.Composition;
using Asv.Common;
using Asv.Mavlink;

namespace Asv.Drones.Gui.Core;

[Export(typeof(IPlaningMission))]
[PartCreationPolicy(CreationPolicy.Shared)]
public class PlaningMission : DisposableOnceWithCancel, IPlaningMission
{
    public const string MissionDataFolder = "missions";
  
    [ImportingConstructor]
    public PlaningMission(IAppService appService)
    {
        var missionPath = Path.Combine(appService.Paths.DataFolder, MissionDataFolder);
        if (Directory.Exists(missionPath) == false)
        {
            Directory.CreateDirectory(missionPath);
        }
        MissionStore = new FileSystemHierarchicalStore<Guid, PlaningMissionFile>(missionPath, 
            PlaningMissionStoreFormat.Instance, TimeSpan.Zero);
    }

    public IHierarchicalStore<Guid, PlaningMissionFile> MissionStore { get; }
}