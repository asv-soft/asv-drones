using System.Reactive.Disposables;
using System.Reactive.Linq;
using Asv.Common;
using Asv.Drones.Gui.Core;
using Asv.Drones.Uav;
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
            // DONE: Localize
            Title = RS.StartAutoAnchorActionViewModel_Title;
            Icon = MaterialIconKind.RayStartArrow;
            Vehicle.Position.IsArmed.Select(_ => _).Subscribe(CanExecute).DisposeItWith(Disposable);
        }

        protected override async Task ExecuteImpl(CancellationToken cancel)
        {
            // DONE: Localize
            _log.Info(LogName,  string.Format(RS.StartAutoAnchorActionViewModel_ExecuteImpl_LogInfo, Vehicle.Name.Value));
            await Vehicle.SetAutoMode(cancel);
            await Vehicle.Missions.Base.MissionSetCurrent(0, cancel);
        }
    }
}