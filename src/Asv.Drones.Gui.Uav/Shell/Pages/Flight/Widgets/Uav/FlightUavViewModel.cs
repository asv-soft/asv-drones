using System.Collections.ObjectModel;
using System.Reactive.Linq;
using System.Windows.Input;
using Asv.Common;
using Asv.Drones.Gui.Core;
using Asv.Drones.Gui.Uav.Uav;
using Asv.Mavlink;
using Avalonia.Controls;
using DynamicData;
using DynamicData.Binding;
using Material.Icons;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Asv.Drones.Gui.Uav
{
    public class FlightUavViewModel:FlightVehicleWidgetBase
    {
        private readonly ReadOnlyObservableCollection<IUavRttItem> _rttItems;
        
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
        
        public FlightUavViewModel(IVehicle vehicle, ILogService log, ILocalizationService loc,
            IEnumerable<IUavRttItemProvider> rttItems):base(vehicle,GenerateUri(vehicle))
        {
            Vehicle.Name.Subscribe(_ => Title = _).DisposeItWith(Disposable);
            Vehicle.Class.Select(MavlinkHelper.GetIcon).Subscribe(_ => Icon = _).DisposeItWith(Disposable);
            Attitude = new AttitudeViewModel(vehicle, new Uri(Id, "/id"),loc);
            MissionStatus = new MissionStatusViewModel(vehicle, log, new Uri(Id, "/id"),loc);
            
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
            }).DisposeItWith(Disposable);
            
            this.WhenValueChanged(_ => _.MissionStatus.EnableAnchors, false)
                .Subscribe(ChangeAnchorsVisibility)
                .DisposeItWith(Disposable);
            
            this.WhenValueChanged(_ => _.MissionStatus.EnablePolygon, false)
                .Subscribe(ChangePolygonVisibility)
                .DisposeItWith(Disposable);

            Map.Markers.WhenValueChanged(_ => _.Count)
                .Subscribe(_ =>
                {
                    ChangeAnchorsVisibility(MissionStatus.EnableAnchors);
                    ChangePolygonVisibility(MissionStatus.EnablePolygon);
                }).DisposeItWith(Disposable);
            
        }
        
        private void ChangePolygonVisibility(bool needTo)
        {
            foreach (var anchor in Map.Markers)
            {
                if (anchor is UavFlightMissionPathPolygon polygon)
                {
                    polygon.PathOpacity = needTo ? 1 : 0;
                }
            }
        }
        
        private void ChangeAnchorsVisibility(bool needTo)
        {
            foreach (var anchor in Map.Markers)
            {
                if (anchor is UavFlightMissionAnchor missionAnchor)
                    missionAnchor.IsVisible = needTo;
            }
        }
        
        public ICommand LocateVehicleCommand { get; set; }
        public AttitudeViewModel Attitude { get; }
        public MissionStatusViewModel MissionStatus { get; }
        public ReadOnlyObservableCollection<IUavRttItem> RttItems => _rttItems;
    }
}