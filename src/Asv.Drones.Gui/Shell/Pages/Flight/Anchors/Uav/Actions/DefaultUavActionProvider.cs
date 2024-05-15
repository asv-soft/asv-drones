using System.Collections.Generic;
using System.Composition;
using Asv.Cfg;
using Asv.Drones.Gui.Api;
using Asv.Mavlink;

namespace Asv.Drones.Gui
{
    [Export(typeof(IUavActionProvider))]
    [method: ImportingConstructor]
    public class DefaultUavActionProvider(ILogService log, IConfiguration cfg, ILocalizationService loc)
        : IUavActionProvider
    {
        public IEnumerable<UavActionBase> CreateActions(IVehicleClient vehicle, IMap map)
        {
            yield return new UavActionGoTo(vehicle, map, log);
            yield return new UavActionTakeOff(vehicle, map, log, cfg, loc);
            yield return new UavActionRtl(vehicle, map, log);
            yield return new UavActionRoi(vehicle, map, log);
            yield return new UavActionLand(vehicle, map, log);
            yield return new UavActionStartAuto(vehicle, map, log);
            yield return new UavActionRebootAutopilot(vehicle, map, log);
            yield return new UavActionSelectMode(vehicle, map, log);
        }
    }
}