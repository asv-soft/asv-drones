using System.Reactive.Disposables;
using System.Reactive.Linq;
using Asv.Drones.Gui.Core;
using Asv.Drones.Uav;
using Asv.Mavlink;
using Material.Icons;
using ReactiveUI;

namespace Asv.Drones.Gui.Uav
{
    public class TakeOffAnchorActionViewModel : UavActionActionBase
    {
        private readonly ILogService _log;

        public TakeOffAnchorActionViewModel(IVehicle vehicle, IMap map, ILogService log) : base(vehicle, map, log)
        {
            _log = log;
            Title = "Take off";
            Icon = MaterialIconKind.ArrowUpBoldHexagonOutline;
            Vehicle.IsArmed.ObserveOn(RxApp.MainThreadScheduler).Select(_ => !_).Subscribe(CanExecute).DisposeWith(Disposable);
        }

        protected override async Task ExecuteImpl(CancellationToken cancel)
        {
            _log.Info(LogName, $"User send TakeOff for {Vehicle.Name.Value}");
            await Vehicle.TakeOff(200, cancel);
        }
    }
}