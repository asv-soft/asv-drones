using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Asv.Common;
using Asv.Drones.Gui.Api;
using Asv.Mavlink;
using Material.Icons;

namespace Asv.Drones.Gui
{
    public class UavActionRoi : UavActionBase
    {
        private readonly ILogService _log;

        public UavActionRoi(IVehicleClient vehicle, IMap map, ILogService log) : base(vehicle, map, log)
        {
            _log = log;
            // DONE: Localize
            Title = RS.RoiAnchorActionViewModel_Title;
            Icon = MaterialIconKind.ImageFilterCenterFocus;
            Vehicle.Position.IsArmed.Select(_ => _).Subscribe(CanExecute).DisposeItWith(Disposable);
        }

        protected override async Task ExecuteImpl(CancellationToken cancel)
        {
            // DONE: Localize
            var target = await Map.ShowTargetDialog(RS.RoiAnchorActionViewModel_ExecuteImpl_ShowTargetDialog_Value,
                CancellationToken.None);
            if (!target.Equals(GeoPoint.NaN))
            {
                var point = new GeoPoint(target.Latitude, target.Longitude,
                    (double)Vehicle.Position.Current.Value.Altitude);
                _log.Info(LogName,
                    string.Format(RS.RoiAnchorActionViewModel_ExecuteImpl_LogInfo, point, Vehicle.Name.Value));
                await Vehicle.Position.SetRoi(point, CancellationToken.None);
            }
        }
    }
}