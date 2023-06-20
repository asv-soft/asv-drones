using System.Reactive.Disposables;
using System.Reactive.Linq;
using Asv.Common;
using Asv.Drones.Gui.Core;
using Asv.Drones.Uav;
using Asv.Mavlink;
using Material.Icons;
using ReactiveUI;

namespace Asv.Drones.Gui.Uav
{
    public class GoToMapAnchorActionViewModel: UavActionActionBase
    {
        private readonly ILogService _log;

        public GoToMapAnchorActionViewModel(IVehicleClient vehicle, IMap map,ILogService log):base(vehicle,map,log)
        {
            _log = log;
            // TODO: Localize
            Title = "GoTo";
            Icon = MaterialIconKind.Target;
            Vehicle.Position.IsArmed.Select(_ => _).Subscribe(CanExecute).DisposeWith(Disposable);
        }

        protected override async Task ExecuteImpl(CancellationToken cancel)
        {
            // TODO: Localize
            var target = await Map.ShowTargetDialog("Select target for GoTo action", CancellationToken.None);
            var point = new GeoPoint(target.Latitude, target.Longitude, (double)Vehicle.Position.Current.Value.Altitude);
            _log.Info(LogName,$"User send GoTo '{point}' for {Vehicle.Name.Value}");
            await Vehicle.GoTo(point, CancellationToken.None);
        }
    }
}