using System.Reactive.Disposables;
using System.Reactive.Linq;
using Asv.Drones.Gui.Core;
using Asv.Drones.Uav;
using Asv.Mavlink;
using Asv.Mavlink.V2.Ardupilotmega;
using Material.Icons;
using ReactiveUI;

namespace Asv.Drones.Gui.Uav
{
    public class StartAutoAnchorActionViewModel : UavActionActionBase
    {
        private readonly ILogService _log;

        public StartAutoAnchorActionViewModel(IVehicleClient vehicle, IMap map, ILogService log) : base(vehicle, map, log)
        {
            _log = log;
            // TODO: Localize
            Title = "Start mission";
            Icon = MaterialIconKind.RayStartArrow;
            Vehicle.Position.IsArmed.ObserveOn(RxApp.MainThreadScheduler).Select(_ => _).Subscribe(CanExecute).DisposeWith(Disposable);
        }

        protected override async Task ExecuteImpl(CancellationToken cancel)
        {
            // TODO: Localize
            _log.Info(LogName, $"User send Start mission for {Vehicle.Name.Value}");
            await Vehicle.Commands.DoSetMode(1, (uint)CopterMode.CopterModeAuto, 0, cancel);
            await Vehicle.Missions.Base.MissionSetCurrent(0, cancel);
        }
    }
}