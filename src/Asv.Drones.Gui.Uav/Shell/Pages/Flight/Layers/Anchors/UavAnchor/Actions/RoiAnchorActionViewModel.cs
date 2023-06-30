using System.Reactive.Disposables;
using System.Reactive.Linq;
using Asv.Common;
using Asv.Drones.Gui.Core;
using Asv.Drones.Uav;
using Asv.Mavlink;
using Material.Icons;

namespace Asv.Drones.Gui.Uav
{
    public class RoiAnchorActionViewModel : UavActionActionBase
    {
        private readonly ILogService _log;

        public RoiAnchorActionViewModel(IVehicleClient vehicle, IMap map,ILogService log) : base(vehicle, map,log)
        {
            _log = log;
            // TODO: Localize
            Title = "Set ROI";
            Icon = MaterialIconKind.ImageFilterCenterFocus;
            Vehicle.Position.IsArmed.Select(_ => _).Subscribe(CanExecute).DisposeItWith(Disposable);
        }

        protected override async Task ExecuteImpl(CancellationToken cancel)
        {
            // TODO: Localize
            var target = await Map.ShowTargetDialog("Select target for region of interests (ROI)", CancellationToken.None);
            var point = new GeoPoint(target.Latitude, target.Longitude, (double)Vehicle.Position.Current.Value.Altitude);
            _log.Info(LogName, $"User set ROI '{point}' for {Vehicle.Name.Value}");
            await Vehicle.Position.SetRoi(point, CancellationToken.None);
        }
    }
}