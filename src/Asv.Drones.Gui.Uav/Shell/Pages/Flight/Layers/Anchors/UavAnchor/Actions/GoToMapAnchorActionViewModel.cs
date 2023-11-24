using System.Reactive.Disposables;
using System.Reactive.Linq;
using Asv.Common;
using Asv.Drones.Gui.Core;
using Asv.Drones.Uav;
using Asv.Mavlink;
using Material.Icons;

namespace Asv.Drones.Gui.Uav
{
    public class GoToMapAnchorActionViewModel: UavActionActionBase
    {
        private readonly ILogService _log;

        public GoToMapAnchorActionViewModel(IVehicleClient vehicle, IMap map,ILogService log):base(vehicle,map,log)
        {
            _log = log;
            // DONE: Localize
            Title = RS.GoToMapAnchorActionViewModel_Title;
            Icon = MaterialIconKind.Target;
            Vehicle.Position.IsArmed.Select(_ => _).Subscribe(CanExecute).DisposeItWith(Disposable);
        }

        protected override async Task ExecuteImpl(CancellationToken cancel)
        {
            // DONE: Localize
             var target = await Map.ShowTargetDialog(RS.GoToMapAnchorActionViewModel_ExecuteImpl_ShowTargetDialog_Value, CancellationToken.None);
            var point = new GeoPoint(target.Latitude, target.Longitude, (double)Vehicle.Position.Current.Value.Altitude);
            _log.Info(LogName,string.Format(RS.GoToMapAnchorActionViewModel_ExecuteImpl_LogInfo_Value, 
                                                            point,
                                                            Vehicle.Name.Value));
            
            await Vehicle.GoTo(point, CancellationToken.None);
        }
    }
}