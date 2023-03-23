using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Reactive.Linq;
using System.Windows.Input;
using Asv.Common;
using Asv.Drones.Gui.Core;
using Asv.Drones.Gui.Uav.Uav;
using Asv.Mavlink;
using Avalonia.Controls;
using DynamicData;
using Material.Icons;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Asv.Drones.Gui.Uav
{
    public class FlightUavViewModel:FlightVehicleWidgetBase
    {
        private ReadOnlyObservableCollection<IUavRttItem> _rttItems;
        public static Uri GenerateUri(IVehicle vehicle) => FlightVehicleWidgetBase.GenerateUri(vehicle,"uav");

        
        
        

        public FlightUavViewModel()
        {
            if (Design.IsDesignMode)
            {
                Icon = MaterialIconKind.Navigation;
                Title = "Hexacopter[45646]";
                Attitude = new AttitudeViewModel();
                MissionStatus = new MissionStatusViewModel();
                _rttItems = new ReadOnlyObservableCollection<IUavRttItem>(new ObservableCollection<IUavRttItem>(new[]
                {
                    new BatteryUavRttViewModel()
                }));
            }
        }
        public FlightUavViewModel(IVehicle vehicle, ILogService log,ILocalizationService loc,
            IEnumerable<IUavRttItemProvider> rttItems):base(vehicle,GenerateUri(vehicle))
        {
            Vehicle.Name.Subscribe(_ => Title = _).DisposeItWith(Disposable);
            Vehicle.Class.Select(MavlinkHelper.GetIcon).Subscribe(_ => Icon = _).DisposeItWith(Disposable);
            Attitude = new AttitudeViewModel(vehicle, new Uri(Id, "/id"),loc);
            MissionStatus = new MissionStatusViewModel(Vehicle, log, new Uri(Id, "/id"),loc);
            
            rttItems
                .SelectMany(_ => _.Create(Vehicle))
                .OrderBy(_=>_.Order)
                .AsObservableChangeSet()
                .AutoRefresh(_=>_.IsVisible)
                .Filter(_=>_.IsVisible)
                .Bind(out _rttItems)
                .DisposeMany()
                .Subscribe()
                .DisposeItWith(Disposable);
        }

        protected override void InternalAfterMapInit(IMap map)
        {
            base.InternalAfterMapInit(map);
            LocateVehicleCommand = ReactiveCommand.Create(() =>
            {
                Map.Center = Vehicle.GlobalPosition.Value;
            } ).DisposeItWith(Disposable);
        }

        
        public ICommand LocateVehicleCommand { get; set; }
        public AttitudeViewModel Attitude { get; }
        public MissionStatusViewModel MissionStatus { get; }
        public ReadOnlyObservableCollection<IUavRttItem> RttItems => _rttItems;
    }
}