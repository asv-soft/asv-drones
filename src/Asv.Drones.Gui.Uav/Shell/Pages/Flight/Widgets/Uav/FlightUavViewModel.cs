using System.Reactive.Linq;
using System.Windows.Input;
using Asv.Common;
using Asv.Drones.Gui.Core;
using Asv.Mavlink;
using Avalonia.Controls;
using Material.Icons;
using ReactiveUI;

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
        public FlightUavViewModel(IVehicle vehicle, ILogService log):base(vehicle,"uav")
        {
            Vehicle.Name.Subscribe(_ => Title = _).DisposeItWith(Disposable);
            Vehicle.Class.Select(MavlinkHelper.GetIcon).Subscribe(_ => Icon = _).DisposeItWith(Disposable);
            Attitude = new AttitudeViewModel(vehicle, new Uri(Id, "/id"));
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
    }
}