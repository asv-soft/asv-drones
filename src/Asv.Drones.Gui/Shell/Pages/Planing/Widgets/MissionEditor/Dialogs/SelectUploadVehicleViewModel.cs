using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using Asv.Cfg;
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

public class SelectUploadVehicleViewModelConfig
{
    public bool CloseOnFinish { get; set; }
}

public class SelectUploadVehicleViewModel : ViewModelBaseWithValidation
{
    private readonly ILogService _log;
    private readonly PlaningMissionViewModel _mission;

    private CancellationTokenSource _cts;
    private ReadOnlyObservableCollection<IVehicleClient> _vehicleItems;

    public SelectUploadVehicleViewModel() : base(WellKnownUri.ShellPageMapPlaningWidgetEditorUploadMissionDialog)
    {
        if (Design.IsDesignMode)
        {
        }
    }

    public SelectUploadVehicleViewModel(IMavlinkDevicesService mavlink, IConfiguration cfg, ILogService log,
        PlaningMissionViewModel mission) : base(WellKnownUri.ShellPageMapPlaningWidgetEditorUploadMissionDialog)
    {
        _log = log;
        _mission = mission;

        var config = cfg.Get<SelectUploadVehicleViewModelConfig>();

        CloseOnFinish = config.CloseOnFinish;

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
                vehicle => vehicle != null, RS.SelectUploadVehicleViewModel_SelectVehicle_ValidationMessage)
            .DisposeItWith(Disposable);

        this.WhenAnyValue(vm => vm.CloseOnFinish)
            .Subscribe(closeOnFinish =>
            {
                config.CloseOnFinish = closeOnFinish;
                cfg.Set(config);
            })
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
            await _cts.CancelAsync();
            return;
        }

        args.Cancel = true;

        sender.IsPrimaryButtonEnabled = false;

        _cts = new CancellationTokenSource();

        await SelectedVehicle.Missions.ClearRemote(_cts.Token);

        SelectedVehicle.Missions.ClearLocal();

        SelectedVehicle.Missions.AddNavMissionItem(SelectedVehicle.Position.Home.Value ?? GeoPoint.Zero);

        _mission.Points.ForEach(_ =>
        {
            if (_cts.IsCancellationRequested) return;
            _.CreateVehicleItems(SelectedVehicle, null);
        });

        try
        {
            await SelectedVehicle.Missions.Upload(_cts.Token, progress => UploadProgress = progress);
            _log.Info("Planing", RS.PlanningPageViewModel_MissionUploaded);
        }
        catch (Exception ex)
        {
            _log.Error("Planing", ex.Message);
        }

        sender.IsPrimaryButtonEnabled = true;

        if (CloseOnFinish) sender.Hide();
    }

    [Reactive] public bool CloseOnFinish { get; set; }
    [Reactive] public double UploadProgress { get; set; }
    [Reactive] public IVehicleClient SelectedVehicle { get; set; }
    public ReadOnlyObservableCollection<IVehicleClient> VehicleItems => _vehicleItems;
}