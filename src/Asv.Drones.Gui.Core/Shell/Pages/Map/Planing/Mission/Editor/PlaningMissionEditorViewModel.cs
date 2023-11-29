using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Reactive;
using System.Windows.Input;
using Asv.Common;
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
    private readonly MissionPointFlyoutMenuItem _replaceItems;
    private IDisposable _selectedPointDisposable;

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
                    Type = PlaningMissionPointType.TakeOff,
                    Icon = MaterialIconKind.FlightTakeoff,
                    Command = ReactiveCommand.Create(() =>
                    {
                        Context.Mission.AddPointCmd.Execute(PlaningMissionPointType.TakeOff).Subscribe();
                    })
                },
                new MissionPointFlyoutMenuItem
                {
                    Title = RS.PlaningMissionEditorViewModel_DoLandMenuItem_Title,
                    Type = PlaningMissionPointType.DoLand,
                    Icon = MaterialIconKind.FlightLand,
                    Command = ReactiveCommand.Create(() =>
                    {
                        Context.Mission.AddPointCmd.Execute(PlaningMissionPointType.DoLand).Subscribe();
                    })
                },
                new MissionPointFlyoutMenuItem
                {
                    Title = RS.PlaningMissionEditorViewModel_WaypointMenuItem_Title,
                    Type = PlaningMissionPointType.Waypoint,
                    Icon = MaterialIconKind.Location,
                    Command = ReactiveCommand.Create(() =>
                    {
                        Context.Mission.AddPointCmd.Execute(PlaningMissionPointType.Waypoint).Subscribe();
                    })
                },
                new MissionPointFlyoutMenuItem
                {
                    Title = RS.PlaningMissionEditorViewModel_RoiMenuItem_Title,
                    Type = PlaningMissionPointType.Roi,
                    Icon = MaterialIconKind.ImageFilterCenterFocus,
                    Command = ReactiveCommand.Create(() =>
                    {
                        Context.Mission.AddPointCmd.Execute(PlaningMissionPointType.Roi).Subscribe();
                    })
                }
            }
        };
        
        _replaceItems = new MissionPointFlyoutMenuItem
        {
            Title = RS.PlaningMissionEditorViewModel_ReplacePointFlyoutMenuItem,
            Icon = MaterialIconKind.FindReplace,
            Items =
            {
                new MissionPointFlyoutMenuItem
                {
                    Title = RS.PlaningMissionEditorViewModel_TakeOffMenuItem_Title,
                    Type = PlaningMissionPointType.TakeOff,
                    Icon = MaterialIconKind.FlightTakeoff,
                    Command = ReactiveCommand.Create(() =>
                    {
                        Context.Mission.ReplacePointCmd.Execute(PlaningMissionPointType.TakeOff).Subscribe();
                    })
                },
                new MissionPointFlyoutMenuItem
                {
                    Title = RS.PlaningMissionEditorViewModel_DoLandMenuItem_Title,
                    Type = PlaningMissionPointType.DoLand,
                    Icon = MaterialIconKind.FlightLand,
                    Command = ReactiveCommand.Create(() =>
                    {
                        Context.Mission.ReplacePointCmd.Execute(PlaningMissionPointType.DoLand).Subscribe();
                    })
                },
                new MissionPointFlyoutMenuItem
                {
                    Title = RS.PlaningMissionEditorViewModel_WaypointMenuItem_Title,
                    Type = PlaningMissionPointType.Waypoint,
                    Icon = MaterialIconKind.Location,
                    Command = ReactiveCommand.Create(() =>
                    {
                        Context.Mission.ReplacePointCmd.Execute(PlaningMissionPointType.Waypoint).Subscribe();
                    })
                },
                new MissionPointFlyoutMenuItem
                {
                    Title = RS.PlaningMissionEditorViewModel_RoiMenuItem_Title,
                    Type = PlaningMissionPointType.Roi,
                    Icon = MaterialIconKind.ImageFilterCenterFocus,
                    Command = ReactiveCommand.Create(() =>
                    {
                        Context.Mission.ReplacePointCmd.Execute(PlaningMissionPointType.Roi).Subscribe();
                    })
                }
            }
        };
        
        AddablePoints.Add(new[]
            {
                _addItems,
                _replaceItems
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
        
        Context.WhenAnyValue(_ => _.Mission)
            .WhereNotNull()
            .Subscribe(_ =>
            {
                if (_selectedPointDisposable != null)
                {
                    _selectedPointDisposable.Dispose();
                    _selectedPointDisposable = null;
                }
                
                _selectedPointDisposable = _.WhenAnyValue(_ => _.SelectedPoint)
                    .Subscribe(_ =>
                    {
                        _replaceItems.IsEnabled = _ != null;
                    });
            }).DisposeItWith(Disposable);
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
    public PlaningMissionPointType Type { get; init; }
    public ReactiveCommand<Unit,Unit> Command { get; init; }
    public List<MissionPointFlyoutMenuItem> Items { get; } = new();
}