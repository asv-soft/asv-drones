using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using System.Windows.Input;
using Asv.Cfg;
using Asv.Common;
using Asv.Drones.Gui.Api;
using Asv.Mavlink;
using DynamicData;
using DynamicData.Binding;
using FluentAvalonia.UI.Controls;
using Material.Icons;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using MavlinkHelper = Asv.Drones.Gui.Api.MavlinkHelper;

namespace Asv.Drones.Gui;

public class FlightUavViewModel : MapWidgetBase
{
    private readonly IVehicleClient _vehicle;
    private readonly IConfiguration _cfg;
    private readonly ILocalizationService _loc;
    private readonly ILogService _log;
    private readonly ReadOnlyObservableCollection<IUavRttItem> _rttItems;

    public FlightUavViewModel() : base(WellKnownUri.UndefinedUri)
    {
        Order = 100;
        DesignTime.ThrowIfNotDesignMode();
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

    public FlightUavViewModel(IVehicleClient vehicle, ILogService log, ILocalizationService loc,
        IConfiguration cfg, IEnumerable<IUavRttItemProvider> rttItems) : base(
        $"{WellKnownUri.ShellPageMapFlightWidgetUav}#{vehicle.FullId}")
    {
        _vehicle = vehicle;
        _cfg = cfg;
        _loc = loc;
        _log = log;
        vehicle.Name.Subscribe(x => Title = x)
            .DisposeItWith(Disposable);
        Icon = MavlinkHelper.GetIcon(vehicle.Class);

        Attitude = new AttitudeViewModel(vehicle, new Uri(Id, "/attitude"), loc)
            .DisposeItWith(Disposable);
        MissionStatus = new MissionStatusViewModel(vehicle, log, new Uri(Id, "/mission"), loc)
            .DisposeItWith(Disposable);

        rttItems
            .SelectMany(_ => _.Create(vehicle))
            .OrderBy(_ => _.Order)
            .AsObservableChangeSet()
            .AutoRefresh(_ => _.IsVisible)
            .Filter(_ => _.IsVisible)
            .Bind(out _rttItems)
            .DisposeMany()
            .Subscribe()
            .DisposeItWith(Disposable);

        vehicle.Position.Current.Subscribe(p => { LastKnownPosition = p; }).DisposeItWith(Disposable);
        vehicle.Heartbeat.Link.DistinctUntilChanged().Subscribe(s =>
        {
            if (s == LinkState.Disconnected)
            {
                log.Info(Title, $"Last known position: {LastKnownPosition}");
            }
        }).DisposeItWith(Disposable);
        MinimizedRttItems = _rttItems.Where(_ => _.IsMinimizedVisible).ToList();
        ChangeStateCommand = ReactiveCommand.Create(() => { IsMinimized = !IsMinimized; }).DisposeItWith(Disposable);
        SelectModeCommand = ReactiveCommand.Create(async () =>
        {
            var dialog = new ContentDialog()
            {
                Title = RS.SelectModeAnchorActionViewModel_Title,
                PrimaryButtonText = RS.SelectModeAnchorActionViewModel_DialogPrimaryButton,
                IsSecondaryButtonEnabled = true,
                SecondaryButtonText = RS.SelectModeAnchorActionViewModel_DialogSecondaryButton
            };

            using var viewModel = new SelectModeViewModel(vehicle);
            dialog.Content = viewModel;

            var result = await dialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                log.Info(Title,
                    string.Format(RS.SelectModeAnchorActionViewModel_LogMessage, CurrentMode.Mode.Name,
                        viewModel.SelectedMode.Mode.Name));
                await vehicle.SetVehicleMode(viewModel.SelectedMode.Mode, CancellationToken.None);
            }
        }).DisposeItWith(Disposable);

        vehicle.CurrentMode.Subscribe(_ => CurrentMode = new VehicleModeWithIcons(_)).DisposeItWith(Disposable);
    }

    protected override void InternalAfterMapInit(IMap context)
    {
        LocateVehicleCommand = ReactiveCommand.Create(() =>
        {
            Map.Center = _vehicle.Position.Current.Value;
            var findUavVehicle = Map.Markers.Where(_ => _ is FlightUavAnchor).Cast<FlightUavAnchor>()
                .FirstOrDefault(_ => _.Vehicle.FullId == _vehicle.FullId);
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
                .FirstOrDefault(_ => _.Vehicle.FullId == _vehicle.FullId);

            if (findUavVehicle != null)
            {
                IsFollowed = true;
                Map.ItemToFollow = findUavVehicle;
            }
        }).DisposeItWith(Disposable);
        TakeOffCommand = ReactiveCommand.CreateFromTask(async () =>
        {
            var dialog = new ContentDialog()
            {
                Title = RS.TakeOffAnchorActionViewModel_Title,
                PrimaryButtonText = RS.TakeOffAnchorActionViewModel_DialogPrimaryButton,
                IsSecondaryButtonEnabled = true,
                SecondaryButtonText = RS.TakeOffAnchorActionViewModel_DialogSecondaryButton
            };

            using var viewModel = new TakeOffViewModel(_cfg, _loc);
            viewModel.ApplyDialog(dialog);
            dialog.Content = viewModel;

            var result = await dialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                var altInMeters = _loc.Altitude.ConvertToSi(viewModel.Altitude);
                _log.Info(LogName,
                          string.Format(RS.TakeOffAnchorActionViewModel_LogMessage,
                                        _loc.Altitude.FromSiToStringWithUnits(altInMeters), _vehicle.Name.Value));
                await _vehicle.TakeOff(altInMeters);
            }
        }).DisposeItWith(Disposable);
        SetRoiCommand = ReactiveCommand.CreateFromTask(async () =>
        {
            var target = await Map.ShowTargetDialog(RS.RoiAnchorActionViewModel_ExecuteImpl_ShowTargetDialog_Value,
                                                    CancellationToken.None);
            if (!target.Equals(GeoPoint.NaN))
            {
                var point = new GeoPoint(target.Latitude, target.Longitude,
                                         (double)_vehicle.Position.Current.Value.Altitude);
                _log.Info(LogName,
                          string.Format(RS.RoiAnchorActionViewModel_ExecuteImpl_LogInfo, point, _vehicle.Name.Value));
                await _vehicle.Position.SetRoi(point, CancellationToken.None);
            }
        }).DisposeItWith(Disposable);
        RtlCommand = ReactiveCommand.CreateFromTask(async () =>
        {
            _log.Info(LogName, string.Format(RS.RtlAnchorActionViewModel_ExecuteImpl_LogInfo, _vehicle.Name.Value));
            await _vehicle.DoRtl();
        }).DisposeItWith(Disposable);
        StartMissionCommand = ReactiveCommand.CreateFromTask(async () =>
        {
            _log.Info(LogName,
                      string.Format(RS.StartAutoAnchorActionViewModel_ExecuteImpl_LogInfo, _vehicle.Name.Value));
            await _vehicle.SetAutoMode();
            await _vehicle.Missions.Base.MissionSetCurrent(0);
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
            },
            new UavAction()
            {
                Title = RS.FlightUavViewModel_TakeOffCommand_Name,
                Command = TakeOffCommand
            },
            new UavAction()
            {
                Title = RS.FlightUavViewModel_RoiCommand_Name,
                Command = SetRoiCommand
            },
            new UavAction()
            {
                Title = RS.FlightUavViewModel_StartMissionCommand_Name,
                Command = StartMissionCommand
            },
            new UavAction()
            {
                Title = RS.FlightUavViewModel_RtlCommand_Name,
                Command = RtlCommand
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
            if (anchor is UavFlightMissionPathPolygon polygon && polygon.Vehicle.FullId == _vehicle.FullId)
            {
                polygon.IsVisible = needTo;
            }
        }
    }

    private void ChangeAnchorsVisibility(bool needTo)
    {
        foreach (var anchor in Map.Markers)
        {
            if (anchor is UavFlightMissionAnchor missionAnchor && missionAnchor.Vehicle.FullId == _vehicle.FullId)
            {
                missionAnchor.IsVisible = needTo;
            }

            if (anchor.Id.ToString().Contains(AnchorConstants.AirportAnchorsTagTypeName))
            {
                anchor.IsVisible = needTo;
            }
        }
    }

    public ICommand LocateVehicleCommand { get; set; }
    public ICommand ChangeStateCommand { get; set; }
    public ICommand FollowUavCommand { get; set; }
    public ICommand TakeOffCommand { get; set; }
    public ICommand SetRoiCommand { get; set; }
    public ICommand StartMissionCommand { get; set; }
    public ICommand RtlCommand { get; set; }
    public ICommand SelectModeCommand { get; set; }
    public List<UavAction> UavActions { get; set; }
    public AttitudeViewModel Attitude { get; }
    public MissionStatusViewModel MissionStatus { get; }
    public ReadOnlyObservableCollection<IUavRttItem> RttItems => _rttItems;
    public IEnumerable<IUavRttItem> MinimizedRttItems { get; set; }

    public GeoPoint LastKnownPosition { get; set; }
    public string LogName => "Map." + Title;

    [Reactive] public bool IsMinimized { get; set; } = false;
    [Reactive] public bool IsFollowed { get; set; }
    [Reactive] public VehicleModeWithIcons CurrentMode { get; set; }
}

public class UavAction
{
    public string Title { get; set; }
    public ICommand Command { get; set; }
}