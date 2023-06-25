using System.ComponentModel.Composition;
using Asv.Cfg;
using Asv.Drones.Gui.Core;
using Asv.Drones.Uav;
using Asv.Mavlink;

namespace Asv.Drones.Gui.Uav
{
    [Export(typeof(IFlightUavActionProvider))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class DefaultFlightUavActionProvider : IFlightUavActionProvider
    {
        private readonly ILogService _log;
        private readonly IConfiguration _cfg;
        private readonly ILocalizationService _loc;
        [ImportingConstructor]
        public DefaultFlightUavActionProvider(ILogService log, IConfiguration cfg, ILocalizationService loc)
        {
            _log = log;
            _cfg = cfg;
            _loc = loc;
        }
        
        public IEnumerable<UavActionActionBase> CreateActions(IVehicleClient vehicle, IMap map)
        {
            yield return new GoToMapAnchorActionViewModel(vehicle,map, _log);
            yield return new TakeOffAnchorActionViewModel(vehicle, map, _log, _cfg, _loc);
            yield return new RtlAnchorActionViewModel(vehicle, map, _log);
            yield return new RoiAnchorActionViewModel(vehicle, map, _log);
            yield return new LandAnchorActionViewModel(vehicle, map, _log);
            yield return new StartAutoAnchorActionViewModel(vehicle, map, _log);
            yield return new SelectModeAnchorActionViewModel(vehicle, map, _log);
        }
    }
}