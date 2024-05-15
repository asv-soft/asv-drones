using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Asv.Common;
using Asv.Drones.Gui.Api;
using Asv.Mavlink;
using Material.Icons;

namespace Asv.Drones.Gui
{
    public class UavActionGoTo : UavActionBase
    {
        private readonly ILogService _log;

        public UavActionGoTo(IVehicleClient vehicle, IMap map, ILogService log) : base(vehicle, map, log)
        {
            _log = log;
            Title = RS.GoToMapAnchorActionViewModel_Title;
            Icon = MaterialIconKind.Target;
            Vehicle.Position.IsArmed.Select(_ => _).Subscribe(CanExecute).DisposeItWith(Disposable);
        }

        protected override async Task ExecuteImpl(CancellationToken cancel)
        {
            var target = await Map.ShowTargetDialog(RS.GoToMapAnchorActionViewModel_ExecuteImpl_ShowTargetDialog_Value,
                CancellationToken.None);
            if (!target.Equals(GeoPoint.NaN))
            {
                var point = new GeoPoint(target.Latitude, target.Longitude,
                    (double)Vehicle.Position.Current.Value.Altitude);
                _log.Info(LogName, string.Format(RS.GoToMapAnchorActionViewModel_ExecuteImpl_LogInfo_Value,
                    point,
                    Vehicle.Name.Value));

                await Vehicle.GoTo(point, CancellationToken.None);
            }
        }
    }
}