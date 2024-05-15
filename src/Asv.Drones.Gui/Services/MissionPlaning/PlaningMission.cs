using System;
using System.Composition;
using System.IO;
using Asv.Common;
using Asv.Drones.Gui.Api;
using Asv.Mavlink;

namespace Asv.Drones.Gui;

[Export(typeof(IPlaningMission))]
[Shared]
public class PlaningMission : DisposableOnceWithCancel, IPlaningMission
{
    public const string MissionDataFolder = "missions";

    [ImportingConstructor]
    public PlaningMission(IApplicationHost appService)
    {
        var missionPath = Path.Combine(appService.Paths.AppDataFolder, MissionDataFolder);
        if (Directory.Exists(missionPath) == false)
        {
            Directory.CreateDirectory(missionPath);
        }

        MissionStore = new FileSystemHierarchicalStore<Guid, PlaningMissionFile>(missionPath,
            PlaningMissionStoreFormat.Instance, TimeSpan.Zero);
    }

    public IHierarchicalStore<Guid, PlaningMissionFile> MissionStore { get; }
}