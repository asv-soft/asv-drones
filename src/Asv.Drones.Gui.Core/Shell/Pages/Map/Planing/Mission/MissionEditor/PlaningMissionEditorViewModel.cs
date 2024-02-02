using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Reactive;
using System.Windows.Input;
using Asv.Common;
using Asv.Mavlink.V2.Common;
using Avalonia.Controls;
using DynamicData;
using DynamicData.Binding;
using Material.Icons;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Asv.Drones.Gui.Core;

[Export]
[PartCreationPolicy(CreationPolicy.NonShared)]
public class PlaningMissionEditorViewModel : PlaningWidgetBase
{
    private readonly IPlaningMission _svc;
    private readonly ILocalizationService _loc;
    private readonly MissionPointFlyoutMenuItem _addItems;

    public PlaningMissionEditorViewModel() : base(new Uri("asv:shell.page.mission.mission-editor"))
    {
        if (Design.IsDesignMode)
        {
            
        }
    }
    
    [ImportingConstructor]
    public PlaningMissionEditorViewModel(IPlaningMission svc, ILocalizationService loc) : this()
    {
        _svc = svc;
        _loc = loc;
        
        _addItems = new MissionPointFlyoutMenuItem
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
        
        AddablePoints.Add(new[]
            {
                _addItems
            }
        );
        
        BeginEditName = ReactiveCommand.Create(() =>
        {
            IsInEditNameMode = true;
        }).DisposeItWith(Disposable);

        EndEditName = ReactiveCommand.Create(() =>
        {
            IsInEditNameMode = false;
            _svc.MissionStore.RenameFile(Context.Mission.MissionId, Context.Mission.Name);
        }).DisposeItWith(Disposable);
        
        Delete = ReactiveCommand.Create(() =>
        {
            _svc.MissionStore.DeleteFile(Context.Mission.MissionId);
            Context.Mission = null;
        }).DisposeItWith(Disposable);
    }
    
    protected override void InternalAfterMapInit(IMap context)
    {
        Context = (IPlaningMissionContext)context;
    }

    [Reactive]
    public bool IsInEditNameMode { get; set; }
    public ReactiveCommand<Unit, Unit> BeginEditName { get; }
    public ReactiveCommand<Unit, Unit> EndEditName { get; }
    public ReactiveCommand<Unit, Unit> Delete { get; }
    [Reactive]
    public IPlaningMissionContext Context { get; set; }
    [Reactive]
    public string TotalDistance { get; set; }
    public List<MissionPointFlyoutMenuItem> AddablePoints { get; } = new();
}

public class MissionPointFlyoutMenuItem : ReactiveObject
{
    [Reactive]
    public bool IsEnabled { get; set; } = true;
    public string Title { get; init; }
    public MaterialIconKind Icon { get; init; }
    public MavCmd Type { get; init; }
    public ReactiveCommand<Unit,Unit> Command { get; init; }
    public List<MissionPointFlyoutMenuItem> Items { get; } = new();
}