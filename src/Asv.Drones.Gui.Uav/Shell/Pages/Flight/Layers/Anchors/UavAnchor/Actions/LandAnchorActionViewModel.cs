using System.Reactive.Disposables;
using System.Reactive.Linq;
using Asv.Drones.Gui.Core;
using Asv.Drones.Uav;
using Asv.Mavlink;
using Material.Icons;
using ReactiveUI;

namespace Asv.Drones.Gui.Uav
{
    public class LandAnchorActionViewModel : UavActionActionBase
    {
        private readonly ILogService _log;

        public LandAnchorActionViewModel(IVehicle vehicle, IMap map,ILogService log) : base(vehicle, map,log)
        {
            _log = log;
            Title = "Immediately land";
            Icon = MaterialIconKind.ArrowDownBoldHexagonOutline;
            Vehicle.IsArmed.ObserveOn(RxApp.MainThreadScheduler).Select(_ => _).Subscribe(CanExecute).DisposeWith(Disposable);
        }

        protected override async Task ExecuteImpl(CancellationToken cancel)
        {
            _log.Info(LogName, $"User send DoLand for {Vehicle.Name.Value}");
            await Vehicle.DoLand(cancel);
        }
    }
}