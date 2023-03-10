using System.ComponentModel.Composition;
using Asv.Drones.Gui.Core;
using Asv.Drones.Uav;
using Asv.Mavlink;

namespace Asv.Drones.Gui.Uav
{
    [Export(typeof(IFlightUavActionProvider))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class DefaultFlightUavActionProvider:IFlightUavActionProvider
    {
        private readonly ILogService _log;

        [ImportingConstructor]
        public DefaultFlightUavActionProvider(ILogService log)
        {
            _log = log;
        }
        
        public IEnumerable<UavActionActionBase> CreateActions(IVehicle vehicle, IMap map)
        {
            yield return new GoToMapAnchorActionViewModel(vehicle,map, _log);
            yield return new TakeOffAnchorActionViewModel(vehicle, map, _log);
            yield return new RtlAnchorActionViewModel(vehicle, map, _log);
            yield return new RoiAnchorActionViewModel(vehicle, map, _log);
            yield return new LandAnchorActionViewModel(vehicle, map, _log);
            yield return new StartAutoAnchorActionViewModel(vehicle, map, _log);
        }
    }
}