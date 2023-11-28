using System.Reactive.Disposables;
using System.Reactive.Linq;
using Asv.Common;
using Asv.Drones.Gui.Core;
using Asv.Mavlink;
using Material.Icons;

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
            Vehicle.Position.IsArmed.Select(_ => _).Subscribe(CanExecute).DisposeItWith(Disposable);
        }

        protected override async Task ExecuteImpl(CancellationToken cancel)
        {
            // TODO: Localize
            _log.Info(LogName, $"User send Start mission for {Vehicle.Name.Value}");
            await Vehicle.SetAutoMode(cancel);
            await Vehicle.Missions.Base.MissionSetCurrent(0, cancel);
        }
    }
}