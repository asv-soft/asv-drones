using System.Reactive.Disposables;
using System.Reactive.Linq;
using Asv.Common;
using Asv.Drones.Gui.Core;
using Asv.Mavlink;
using Material.Icons;

namespace Asv.Drones.Gui.Uav
{
    public class RtlAnchorActionViewModel : UavActionActionBase
    {
        private readonly ILogService _log;

        public RtlAnchorActionViewModel(IVehicleClient vehicle, IMap map,ILogService log) : base(vehicle, map,log)
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