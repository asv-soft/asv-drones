using System.Reactive.Disposables;
using System.Reactive.Linq;
using Asv.Drones.Gui.Core;
using Asv.Drones.Uav;
using Asv.Mavlink;
using Material.Icons;
using ReactiveUI;

namespace Asv.Drones.Gui.Uav
{
    public class RtlAnchorActionViewModel : UavActionActionBase
    {
        private readonly ILogService _log;

        public RtlAnchorActionViewModel(IVehicle vehicle, IMap map,ILogService log) : base(vehicle, map,log)
        {
            _log = log;
            // TODO: Localize
            Title = "Return to launch (RTL)";
            Icon = MaterialIconKind.HomeCircleOutline;
            Vehicle.IsArmed.ObserveOn(RxApp.MainThreadScheduler).Select(_ => _).Subscribe(CanExecute).DisposeWith(Disposable);
        }

        protected override async Task ExecuteImpl(CancellationToken cancel)
        {
            // TODO: Localize
            _log.Info(LogName, $"User send Return To the Launch(RTL) for {Vehicle.Name.Value}");
            await Vehicle.DoRtl(cancel);
        }
    }
}