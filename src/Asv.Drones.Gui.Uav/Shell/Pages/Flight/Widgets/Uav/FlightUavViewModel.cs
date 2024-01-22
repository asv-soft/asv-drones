using System.Collections.ObjectModel;
using System.Reactive.Linq;
using System.Windows.Input;
using Asv.Common;
using Asv.Drones.Gui.Core;
using Asv.Drones.Gui.Uav.MissionStatus;
using Asv.Mavlink;
using Avalonia.Controls;
using DynamicData;
using DynamicData.Binding;
using FluentAvalonia.UI.Controls;
using Material.Icons;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using MavlinkHelper = Asv.Drones.Gui.Core.MavlinkHelper;

namespace Asv.Drones.Gui.Uav
{
    public class UavAction
    {
        public string Title { get; set; }
        public ICommand Command { get; set; }
    }
    
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
                UavActions = new List<UavAction>
                {
                    new UavAction()
                    {
                        Title = RS.FlightUavView_FindButton_ToolTip
                    },
                    new UavAction()
                    {
                       Title = RS.FlightUavView_FollowButton_ToolTip
                    }
                };
            }
        }
        
        public FlightUavViewModel(IVehicleClient vehicle, ILogService log, ILocalizationService loc,
            IEnumerable<IUavRttItemProvider> rttItems):base(vehicle,GenerateUri(vehicle))
        {
            Vehicle.Name.Subscribe(_ => Title = _).DisposeItWith(Disposable);
            Icon = MavlinkHelper.GetIcon(vehicle.Class);
            Attitude = new AttitudeViewModel(vehicle, new Uri(Id, "/id"),loc);
            MissionStatus = new MissionStatusViewModel(vehicle, log, new Uri(Id, "/id"),loc);
            Vehicle.CurrentMode.Subscribe(_ => CurrentMode = new VehicleModeWithIcons(_)).DisposeItWith(Disposable);

            rttItems
                .SelectMany(_ => _.Create(Vehicle))
                .OrderBy(_=>_.Order)
                .AsObservableChangeSet()
                .AutoRefresh(_ => _.IsVisible)
                .Filter(_ => _.IsVisible)
                .Bind(out _rttItems)
                .DisposeMany()
                .Subscribe()
                .DisposeItWith(Disposable);

            Vehicle.Position.Current.Subscribe(p =>
            {
                LastKnownPosition = p;
            });

            Vehicle.Heartbeat.Link.DistinctUntilChanged().Subscribe(s =>
            {
                if (s == LinkState.Disconnected)
                {
                    log.Info(LogName, $"Last known position: {LastKnownPosition}");
                }
            });

            MinimizedRttItems = _rttItems.Where(_ => _.IsMinimizedVisible).ToList();

            ChangeStateCommand = ReactiveCommand.Create(() =>
            {
                IsMinimized = !IsMinimized;
            });

            SelectModeCommand = ReactiveCommand.Create(async () =>
            {
                var dialog = new ContentDialog()
                {
                    Title = RS.SelectModeAnchorActionViewModel_Title,
                    PrimaryButtonText = RS.SelectModeAnchorActionViewModel_DialogPrimaryButton,
                    IsSecondaryButtonEnabled = true,
                    SecondaryButtonText = RS.SelectModeAnchorActionViewModel_DialogSecondaryButton
                };
        
                using var viewModel = new SelectModeViewModel(Vehicle);
                dialog.Content = viewModel;

                var result = await dialog.ShowAsync();

                if (result == ContentDialogResult.Primary)
                {
                    log.Info(LogName, string.Format(RS.SelectModeAnchorActionViewModel_LogMessage, CurrentMode.Mode.Name, viewModel.SelectedMode.Mode.Name));
                    await Vehicle.SetVehicleMode(viewModel.SelectedMode.Mode, CancellationToken.None);
                }
            });

            vehicle.CurrentMode.Subscribe(_ => CurrentMode = new VehicleModeWithIcons(_)).DisposeItWith(Disposable);
        }

        protected override void InternalAfterMapInit(IMap context)
        {
            base.InternalAfterMapInit(context);
            
            LocateVehicleCommand = ReactiveCommand.Create(() =>
            {
                Map.Center = Vehicle.Position.Current.Value;
                var findUavVehicle = Map.Markers.Where(_ => _ is FlightUavAnchor).Cast<FlightUavAnchor>()
                    .FirstOrDefault(_ => _.Vehicle.FullId == Vehicle.FullId);
                if (findUavVehicle != null)
                {
                    Map.SelectedItem = findUavVehicle;
                }
            }).DisposeItWith(Disposable);

            FollowUavCommand = ReactiveCommand.Create(() =>
            {
                if (Map.ItemToFollow != null)
                {
                    Map.ItemToFollow = null;
                    IsFollowed = false;
                    return;
                }
                
                if (Map is MapPageViewModel vm)
                {
                    var uavWidgets = vm.LeftWidgets.OfType<FlightUavViewModel>();
                    foreach (var uavWidget in uavWidgets)
                    {
                        uavWidget.IsFollowed = false;
                    }
                }
                
                var findUavVehicle = Map.Markers.Where(_ => _ is FlightUavAnchor).Cast<FlightUavAnchor>()
                    .FirstOrDefault(_ => _.Vehicle.FullId == Vehicle.FullId);
                
                if (findUavVehicle != null)
                {
                    IsFollowed = true;
                    Map.ItemToFollow = findUavVehicle;
                }
            }).DisposeItWith(Disposable);

            UavActions = new List<UavAction>
            {
                new UavAction()
                {
                    Title = RS.FlightUavView_FindButton_ToolTip,
                    Command = LocateVehicleCommand
                },
                new UavAction()
                {
                    Title = RS.FlightUavView_FollowButton_ToolTip,
                    Command = FollowUavCommand
                }
            };

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
        public ICommand FollowUavCommand { get; set; }
        public ICommand SelectModeCommand { get; set; }
        public List<UavAction> UavActions { get; set; }
        public AttitudeViewModel Attitude { get; }
        public MissionStatusViewModel MissionStatus { get; }
        public ReadOnlyObservableCollection<IUavRttItem> RttItems => _rttItems;
        public IEnumerable<IUavRttItem> MinimizedRttItems { get; set; }

        public GeoPoint LastKnownPosition { get; set; }
        public string LogName => "Map." + Title;

        [Reactive] 
        public bool IsMinimized { get; set; } = false;
        [Reactive]
        public bool IsFollowed { get; set; }
        [Reactive]
        public VehicleModeWithIcons CurrentMode { get; set; }
    }
}