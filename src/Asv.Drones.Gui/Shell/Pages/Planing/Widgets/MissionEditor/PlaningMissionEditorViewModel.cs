using System;
using System.Collections.Generic;
using System.Composition;
using System.Reactive;
using System.Threading;
using System.Threading.Tasks;
using Asv.Cfg;
using Asv.Common;
using Asv.Drones.Gui.Api;
using Asv.Mavlink;
using Asv.Mavlink.V2.Common;
using DynamicData;
using FluentAvalonia.UI.Controls;
using Material.Icons;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Asv.Drones.Gui;

[Export(WellKnownUri.ShellPageMapPlaning, typeof(IMapWidget))]
public class PlaningMissionEditorViewModel : MapWidgetBase
{
    private readonly IPlaningMission _svc;
    private readonly ILocalizationService _loc;
    private readonly ILogService _log;
    private readonly IConfiguration _cfg;
    private readonly MissionPointFlyoutMenuItem _addableItems;

    public PlaningMissionEditorViewModel() : base(WellKnownUri.Undefined)
    {
        DesignTime.ThrowIfNotDesignMode();
    }

    [ImportingConstructor]
    public PlaningMissionEditorViewModel(IPlaningMission svc, ILocalizationService loc, ILogService log,
        IConfiguration cfg) : base(WellKnownUri.ShellPageMapPlaningWidgetEditor)
    {
        _svc = svc;
        _loc = loc;
        _cfg = cfg;
        _log = log;
        
        Order = 100;

        _addableItems = new MissionPointFlyoutMenuItem
        {
            Title = RS.PlaningMissionEditorViewModel_AddPointFlyoutMenuItem_Title,
            Icon = MaterialIconKind.Add,
            Items =
            {
                new MissionPointFlyoutMenuItem
                {
                    Title = RS.PlaningMissionEditorViewModel_TakeOffMenuItem_Title,
                    Type = MavCmd.MavCmdNavTakeoff,
                    Icon = MaterialIconKind.FlightTakeoff,
                    Command = ReactiveCommand.Create(() =>
                    {
                        Context.Mission.AddPointCmd.Execute(MavCmd.MavCmdNavTakeoff).Subscribe();
                    })
                },
                new MissionPointFlyoutMenuItem
                {
                    Title = RS.PlaningMissionEditorViewModel_DoLandMenuItem_Title,
                    Type = MavCmd.MavCmdNavLand,
                    Icon = MaterialIconKind.FlightLand,
                    Command = ReactiveCommand.Create(() =>
                    {
                        Context.Mission.AddPointCmd.Execute(MavCmd.MavCmdNavLand).Subscribe();
                    })
                },
                new MissionPointFlyoutMenuItem
                {
                    Title = RS.PlaningMissionEditorViewModel_WaypointMenuItem_Title,
                    Type = MavCmd.MavCmdNavWaypoint,
                    Icon = MaterialIconKind.Location,
                    Command = ReactiveCommand.Create(() =>
                    {
                        Context.Mission.AddPointCmd.Execute(MavCmd.MavCmdNavWaypoint).Subscribe();
                    })
                },
                new MissionPointFlyoutMenuItem
                {
                    Title = RS.PlaningMissionEditorViewModel_NavSplineWaypointMenuItem_Title,
                    Type = MavCmd.MavCmdNavSplineWaypoint,
                    Icon = MaterialIconKind.Location,
                    Command = ReactiveCommand.Create(() =>
                    {
                        Context.Mission.AddPointCmd.Execute(MavCmd.MavCmdNavSplineWaypoint).Subscribe();
                    })
                },
                new MissionPointFlyoutMenuItem
                {
                    Title = RS.PlaningMissionEditorViewModel_DoSetRoiMenuItem_Title,
                    Type = MavCmd.MavCmdDoSetRoi,
                    Icon = MaterialIconKind.ImageFilterCenterFocus,
                    Command = ReactiveCommand.Create(() =>
                    {
                        Context.Mission.AddPointCmd.Execute(MavCmd.MavCmdDoSetRoi).Subscribe();
                    })
                },
                new MissionPointFlyoutMenuItem
                {
                    Title = RS.PlaningMissionEditorViewModel_DoChangeSpeedMenuItem_Title,
                    Type = MavCmd.MavCmdDoChangeSpeed,
                    Icon = MaterialIconKind.Speedometer,
                    Command = ReactiveCommand.Create(() =>
                    {
                        Context.Mission.AddPointCmd.Execute(MavCmd.MavCmdDoChangeSpeed).Subscribe();
                    })
                }
            }
        };

        AddablePoints.Add(_addableItems);

        BeginEditMissionNameCommand =
            ReactiveCommand.Create(() => { IsInEditMissionNameMode = true; }).DisposeItWith(Disposable);

        EndEditMissionNameCommand = ReactiveCommand.Create(EndEditMissionName).DisposeItWith(Disposable);

        DeleteMissionCommand = ReactiveCommand.Create(DeleteMission).DisposeItWith(Disposable);

        DownloadMissionCommand = ReactiveCommand.CreateFromTask(DownloadMissionAsync).DisposeItWith(Disposable);

        UploadMissionCommand = ReactiveCommand.CreateFromTask(UploadMissionAsync).DisposeItWith(Disposable);
    }

    private void EndEditMissionName()
    {
        IsInEditMissionNameMode = false;
        _svc.MissionStore.RenameFile(Context.Mission.MissionId, Context.Mission.Name);
    }

    private void DeleteMission()
    {
        _svc.MissionStore.DeleteFile(Context.Mission.MissionId);
        Context.Mission = null;
    }

    private async Task UploadMissionAsync()
    {
        if (Context.Mission == null) return;

        var dialog = new ContentDialog
        {
            Title = RS.PlaningMissionEditorViewModel_UploadDialog_Title,
            PrimaryButtonText = RS.PlaningMissionEditorViewModel_UploadDialog_PrimaryButtonText,
            IsSecondaryButtonEnabled = true,
            SecondaryButtonText = RS.PlaningMissionEditorViewModel_UploadDialog_SecondaryButtonText
        };

        using var viewModel = new SelectUploadVehicleViewModel(Context.Devices, _cfg, _log, Context.Mission);
        viewModel.ApplyDialog(dialog);
        dialog.Content = viewModel;
        await dialog.ShowAsync();
    }

    private async Task DownloadMissionAsync()
    {
        if (Context.Mission == null) return;

        var dialog = new ContentDialog
        {
            Title = RS.PlaningMissionEditorViewModel_DownloadDialog_Title,
            PrimaryButtonText = RS.PlaningMissionEditorViewModel_DownloadDialog_PrimaryButtonText,
            IsSecondaryButtonEnabled = true,
            SecondaryButtonText = RS.PlaningMissionEditorViewModel_DownloadDialog_SecondaryButtonText
        };

        using var viewModel = new SelectDownloadVehicleViewModel(Context.Devices, _log, Context.Mission);
        viewModel.ApplyDialog(dialog);
        dialog.Content = viewModel;
        await dialog.ShowAsync();
    }

    protected override void InternalAfterMapInit(IMap context)
    {
        Context = (IPlaningMissionContext)context;
    }

    [Reactive] public bool IsInEditMissionNameMode { get; set; }
    public ReactiveCommand<Unit, Unit> BeginEditMissionNameCommand { get; }
    public ReactiveCommand<Unit, Unit> EndEditMissionNameCommand { get; }
    public ReactiveCommand<Unit, Unit> DeleteMissionCommand { get; }
    public ReactiveCommand<Unit, Unit> UploadMissionCommand { get; }
    public ReactiveCommand<Unit, Unit> DownloadMissionCommand { get; }

    [Reactive] public IPlaningMissionContext Context { get; set; }
    [Reactive] public string TotalDistance { get; set; }
    public List<MissionPointFlyoutMenuItem> AddablePoints { get; } = [];
}

public class MissionPointFlyoutMenuItem : ReactiveObject
{
    [Reactive] public bool IsEnabled { get; set; } = true;
    public string Title { get; init; }
    public MaterialIconKind Icon { get; init; }
    public MavCmd Type { get; init; }
    public ReactiveCommand<Unit, Unit> Command { get; init; }
    public List<MissionPointFlyoutMenuItem> Items { get; } = new();
}