using System.Collections.ObjectModel;
using System.Reactive.Linq;
using System.Windows.Input;
using Asv.Common;
using Asv.Drones.Gui.Core;
using Asv.Drones.Gui.Uav.MissionStatus;
using Asv.Drones.Gui.Uav.Uav;
using Asv.Mavlink;
using Avalonia.Controls;
using DynamicData;
using DynamicData.Binding;
using Material.Icons;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using MavlinkHelper = Asv.Drones.Gui.Core.MavlinkHelper;

namespace Asv.Drones.Gui.Uav
{
    public class FlightUavViewModel:FlightVehicleWidgetBase
    {
        private readonly ReadOnlyObservableCollection<IUavRttItem> _rttItems;

        public static Uri GenerateUri(IVehicleClient vehicle) => FlightVehicleWidgetBase.GenerateUri(vehicle,"uav");
        
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
        
        public FlightUavViewModel(IVehicleClient vehicle, ILogService log, ILocalizationService loc,
            IEnumerable<IUavRttItemProvider> rttItems):base(vehicle,GenerateUri(vehicle))
        {
            Vehicle.Name.Subscribe(_ => Title = _).DisposeItWith(Disposable);
            Icon = MavlinkHelper.GetIcon(vehicle.Class);
            Attitude = new AttitudeViewModel(vehicle, new Uri(Id, "/id"),loc);
            MissionStatus = new MissionStatusViewModel(vehicle, log, new Uri(Id, "/id"),loc);
            
            rttItems
                .SelectMany(_ => _.Create(Vehicle))
                .OrderBy(_=>_.Order)
                .AsObservableChangeSet()
                //TODO: check this rtt item behavior
                .AutoRefreshOnObservable(_ => this.WhenAnyValue(__ => __.IsMinimized), TimeSpan.FromMilliseconds(100))
                .Filter(_ =>
                {
                    if (!IsMinimized)
                    {
                        return _.IsVisible;
                    }
                    
                    return _.IsVisible & _.IsMinimizedVisible;
                })
                .Bind(out _rttItems)
                .DisposeMany()
                .Subscribe()
                .DisposeItWith(Disposable);

            ChangeStateCommand = ReactiveCommand.Create(() =>
            {
                IsMinimized = !IsMinimized;
            });
        }

        protected override void InternalAfterMapInit(IMap map)
        {
            base.InternalAfterMapInit(map);
            
            LocateVehicleCommand = ReactiveCommand.Create(() =>
            {
                Map.Center = Vehicle.Position.Current.Value;
                var findUavVehicle = Map.Markers.Where(_ => _ is UavAnchor).Cast<UavAnchor>()
                    .FirstOrDefault(_ => _.Vehicle.FullId == Vehicle.FullId);
                if (findUavVehicle != null)
                {
                    Map.SelectedItem = findUavVehicle;
                }
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
                if (anchor is UavFlightMissionPathPolygon polygon && polygon.Vehicle.FullId == Vehicle.FullId)
                    polygon.IsVisible = needTo;
            }
        }
        
        private void ChangeAnchorsVisibility(bool needTo)
        {
            foreach (var anchor in Map.Markers)
            {
                if (anchor is UavFlightMissionAnchor missionAnchor && missionAnchor.Vehicle.FullId == Vehicle.FullId)
                    missionAnchor.IsVisible = needTo;
            }
        }
        
        public ICommand LocateVehicleCommand { get; set; }
        public ICommand ChangeStateCommand { get; set; }
        public AttitudeViewModel Attitude { get; }
        public MissionStatusViewModel MissionStatus { get; }
        public ReadOnlyObservableCollection<IUavRttItem> RttItems => _rttItems;

        [Reactive] 
        public bool IsMinimized { get; set; } = false;
    }
}