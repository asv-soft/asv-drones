using System.Collections.ObjectModel;
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
            CurrentMode = new VehicleModeWithIcons(Vehicle.CurrentMode.Value);
            
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
        
                var viewModel = new SelectModeViewModel(Vehicle);
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

            FollowUavCommand = ReactiveCommand.Create(() =>
            {
                var findUavVehicle = Map.Markers.Where(_ => _ is UavAnchor).Cast<UavAnchor>()
                    .FirstOrDefault(_ => _.Vehicle.FullId == Vehicle.FullId);
                if (findUavVehicle != null)
                {
                    Map.ItemToFollow = IsFollowed ? findUavVehicle : null;
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
        public ICommand FollowUavCommand { get; set; }
        public ICommand SelectModeCommand { get; set; }
        public AttitudeViewModel Attitude { get; }
        public MissionStatusViewModel MissionStatus { get; }
        public ReadOnlyObservableCollection<IUavRttItem> RttItems => _rttItems;
        public IEnumerable<IUavRttItem> MinimizedRttItems { get; set; }
        
        public string LogName => "Map." + Title;

        [Reactive] 
        public bool IsMinimized { get; set; } = false;
        [Reactive]
        public bool IsFollowed { get; set; }
        [Reactive]
        public VehicleModeWithIcons CurrentMode { get; set; }
    }
}