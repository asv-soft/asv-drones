using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Asv.Common;
using Asv.Drones.Gui.Api;
using Asv.Mavlink;
using Material.Icons;

namespace Asv.Drones.Gui
{
    public class UavActionStartAuto : UavActionBase
    {
        private readonly ILogService _log;

        public UavActionStartAuto(IVehicleClient vehicle, IMap map, ILogService log) : base(vehicle, map, log)
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
            _log.Info(LogName,
                string.Format(RS.StartAutoAnchorActionViewModel_ExecuteImpl_LogInfo, Vehicle.Name.Value));
            await Vehicle.SetAutoMode(cancel);
            await Vehicle.Missions.Base.MissionSetCurrent(0, cancel);
        }
    }
}