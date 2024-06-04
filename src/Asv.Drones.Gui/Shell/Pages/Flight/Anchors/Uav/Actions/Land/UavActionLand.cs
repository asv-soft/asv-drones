using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Asv.Common;
using Asv.Drones.Gui.Api;
using Asv.Mavlink;
using Material.Icons;

namespace Asv.Drones.Gui
{
    public class UavActionLand : UavActionBase
    {
        private readonly ILogService _log;

        public UavActionLand(IVehicleClient vehicle, IMap map, ILogService log) : base(vehicle, map, log)
        {
            _log = log;

            Title = RS.LandAnchorActionViewModel_Title;
            Icon = MaterialIconKind.ArrowDownBoldHexagonOutline;
            Vehicle.Position.IsArmed.Select(_ => _).Subscribe(CanExecute).DisposeItWith(Disposable);
        }

        protected override async Task ExecuteImpl(CancellationToken cancel)
        {
            _log.Info(LogName, string.Format(RS.LandAnchorActionViewModel_ExecuteImpl_LogInfo, Vehicle.Name.Value));
            await Vehicle.DoLand(cancel);
        }
    }
}