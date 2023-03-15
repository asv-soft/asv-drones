using System.Reactive.Linq;
using System.Windows.Input;
using Asv.Common;
using Asv.Drones.Gui.Core;
using Asv.Mavlink;
using Asv.Mavlink.V2.Common;
using Avalonia.Controls;
using Material.Icons;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Asv.Drones.Gui.Uav
{
    public class FlightUavViewModel:FlightVehicleWidgetBase
    {
        public FlightUavViewModel()
        {
            if (Design.IsDesignMode)
            {
                Icon = MaterialIconKind.Navigation;
                Title = "Hexacopter[45646]";
                Attitude = new AttitudeViewModel();
            }
        }
        public FlightUavViewModel(IVehicle vehicle, ILogService log,ILocalizationService loc):base(vehicle,"uav")
        {
            Vehicle.Name.Subscribe(_ => Title = _).DisposeItWith(Disposable);
            Vehicle.Class.Select(MavlinkHelper.GetIcon).Subscribe(_ => Icon = _).DisposeItWith(Disposable);
            Attitude = new AttitudeViewModel(vehicle, new Uri(Id, "/id"),loc);
            Vehicle.BatteryCharge.Subscribe(_ => BatteryLevel = _.Value).DisposeItWith(Disposable);
            Vehicle.GpsInfo.Subscribe(_ => FixType = _.FixType).DisposeItWith(Disposable);
            Vehicle.GpsInfo.Subscribe(_ => DopStatus = _.PdopStatus).DisposeItWith(Disposable);
        }

        protected override void InternalAfterMapInit(IMap map)
        {
            base.InternalAfterMapInit(map);
            LocateVehicleCommand = ReactiveCommand.Create(() =>
            {
                Map.Center = Vehicle.GlobalPosition.Value;
            } ).DisposeItWith(Disposable);
        }

        public AttitudeViewModel Attitude { get; }
        public ICommand LocateVehicleCommand { get; set; }
        [Reactive] 
        public double BatteryLevel { get; set; }
        [Reactive]
        public GpsFixType FixType { get; set; }
        [Reactive]
        public DopStatusEnum DopStatus { get; set; }
    }
}