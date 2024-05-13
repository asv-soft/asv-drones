using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using Asv.Common;
using Asv.Drones.Gui.Api;
using Asv.Mavlink;
using Avalonia.Controls;
using DynamicData;
using FluentAvalonia.UI.Controls;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using ReactiveUI.Validation.Extensions;

namespace Asv.Drones.Gui;

public class SelectDownloadVehicleViewModel : ViewModelBaseWithValidation
{
    private readonly ILogService _log;
    private readonly PlaningMissionViewModel _mission;

    private CancellationTokenSource _cts;
    private ReadOnlyObservableCollection<IVehicleClient> _vehicleItems;

    public SelectDownloadVehicleViewModel() : base(WellKnownUri.ShellPageMapPlaningWidgetEditorDownloadMissionDialog)
    {
        if (Design.IsDesignMode)
        {
        }
    }

    public SelectDownloadVehicleViewModel(IMavlinkDevicesService mavlink, ILogService log,
        PlaningMissionViewModel mission) : base(WellKnownUri.ShellPageMapPlaningWidgetEditorDownloadMissionDialog)
    {
        _log = log;
        _mission = mission;

        mavlink.Vehicles
            .Bind(out _vehicleItems)
            .Subscribe()
            .DisposeItWith(Disposable);

        mavlink.Vehicles.CountChanged()
            .Subscribe(_ =>
            {
                if (VehicleItems != null) SelectedVehicle = VehicleItems.FirstOrDefault();
            })
            .DisposeItWith(Disposable);

        this.ValidationRule(vm => vm.SelectedVehicle,
                vehicle => vehicle != null, RS.SelectDownloadVehicleViewModel_SelectVehicle_ValidationMessage)
            .DisposeItWith(Disposable);
    }

    public void ApplyDialog(ContentDialog dialog)
    {
        dialog.Closing += OnDialogOnClosing;

        this.IsValid()
            .Subscribe(isValid => dialog.IsPrimaryButtonEnabled = isValid)
            .DisposeItWith(Disposable);
    }

    private async void OnDialogOnClosing(ContentDialog sender, ContentDialogClosingEventArgs args)
    {
        if (args.Result is not ContentDialogResult.Primary)
        {
            if (_cts != null) await _cts.CancelAsync();
            return;
        }

        args.Cancel = true;

        sender.IsPrimaryButtonEnabled = false;

        _cts = new CancellationTokenSource();

        MissionItem[]? vehicleMission = null;

        try
        {
            vehicleMission =
                await SelectedVehicle.Missions.Download(_cts.Token, progress => DownloadProgress = progress);
            _log.Info("Planing", RS.PlanningPageViewModel_MissionDownloaded);
        }
        catch (Exception ex)
        {
            _log.Error("Planing", ex.Message);
        }

        if (vehicleMission != null) _mission.ClearAllPoints();

        foreach (var missionItem in vehicleMission)
        {
            if (_cts.IsCancellationRequested) return;
            if (missionItem.Index == 0) continue;
            _mission?.AddOrUpdatePoint(missionItem.TransformMissionItemToPointModel());
        }

        sender.IsPrimaryButtonEnabled = true;

        sender.Hide();
    }

    [Reactive] public double DownloadProgress { get; set; }
    [Reactive] public IVehicleClient SelectedVehicle { get; set; }
    public ReadOnlyObservableCollection<IVehicleClient> VehicleItems => _vehicleItems;
}