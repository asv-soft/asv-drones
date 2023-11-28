using System.Reactive.Disposables;
using System.Reactive.Linq;
using Asv.Common;
using Asv.Drones.Gui.Core;
using Asv.Mavlink;
using Material.Icons;

namespace Asv.Drones.Gui.Uav
{
    public class LandAnchorActionViewModel : UavActionActionBase
    {
        private readonly ILogService _log;

        public LandAnchorActionViewModel(IVehicleClient vehicle, IMap map,ILogService log) : base(vehicle, map,log)
        {
            _log = log;
            
            Title = RS.LandAnchorActionViewModel_Title;
            Icon = MaterialIconKind.ArrowDownBoldHexagonOutline;
            Vehicle.Position.IsArmed.Select(_ => _).Subscribe(CanExecute).DisposeItWith(Disposable);
        }

        protected override async Task ExecuteImpl(CancellationToken cancel)
        {
            
            _log.Info(LogName,  string.Format(RS.LandAnchorActionViewModel_ExecuteImpl_LogInfo, Vehicle.Name.Value));
            await Vehicle.DoLand(cancel);
        }
    }
}