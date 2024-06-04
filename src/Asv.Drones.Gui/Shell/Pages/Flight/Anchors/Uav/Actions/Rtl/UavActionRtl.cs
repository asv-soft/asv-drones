using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Asv.Common;
using Asv.Drones.Gui.Api;
using Asv.Mavlink;
using Material.Icons;

namespace Asv.Drones.Gui
{
    public class UavActionRtl : UavActionBase
    {
        private readonly ILogService _log;

        public UavActionRtl(IVehicleClient vehicle, IMap map, ILogService log) : base(vehicle, map, log)
        {
            _log = log;
            // DONE: Localize
            Title = RS.RtlAnchorActionViewModel_RTL;
            Icon = MaterialIconKind.HomeCircleOutline;
            Vehicle.Position.IsArmed.Select(_ => _).Subscribe(CanExecute).DisposeItWith(Disposable);
        }

        protected override async Task ExecuteImpl(CancellationToken cancel)
        {
            // DONE: Localize
            _log.Info(LogName, string.Format(RS.RtlAnchorActionViewModel_ExecuteImpl_LogInfo, Vehicle.Name.Value));
            await Vehicle.DoRtl(cancel);
        }
    }
}