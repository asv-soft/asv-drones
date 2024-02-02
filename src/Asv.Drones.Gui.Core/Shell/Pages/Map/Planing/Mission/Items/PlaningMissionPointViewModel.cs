using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Reactive;
using Asv.Avalonia.Map;
using Asv.Common;
using Asv.Mavlink;
using Asv.Mavlink.V2.Common;
using Avalonia.Media;
using DynamicData;
using DynamicData.Binding;
using Material.Icons;
using Newtonsoft.Json.Linq;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using ReactiveUI.Validation.Extensions;

namespace Asv.Drones.Gui.Core;

public class PlaningMissionPointViewModel : ViewModelBaseWithValidation
{
    private const string UriString = "asv:shell.page.planing-mission.mission.point";
    
    private readonly PlaningMissionViewModel _parent;
    private readonly PlaningMissionPointModel _point;
    
    private PlaningMissionPointViewModel(string uri) : base(uri)
    {
        this.ValidationRule(x=>x.Name, name => !string.IsNullOrWhiteSpace(name), RS.PlaningMissionViewModel_NameMustBeNotEmpty);
    }

    protected PlaningMissionPointViewModel(PlaningMissionPointModel point, PlaningMissionViewModel parent) 
        : this($"{UriString}.{point.Index}.{point.Type}")
    {
        _parent = parent;
        _point = point;
        
        Index = point.Index;
        Type = point.Type;
        
        Delete = ReactiveCommand.Create(() =>
        {
            parent.RemovePoint(this);
        }).DisposeItWith(Disposable);
        
        this.WhenValueChanged(_ => _.Name)
            .WhereNotNull()
            .Subscribe(_ =>
            {
                IsChanged = true;
                if(MissionAnchor != null)
                    MissionAnchor.Title = _;
            })
            .DisposeItWith(Disposable);

        this.WhenValueChanged(_ => _.Index)
            .Subscribe(_ =>
            {
                IsChanged = true;
                _point.Index = _;
                if(MissionAnchor != null)
                    MissionAnchor.Index = _;
                Name = $"{Type.GetPlaningMissionPointName()} {_}";
            }).DisposeItWith(Disposable);

        this.WhenValueChanged(_ => _.Type)
            .Subscribe(_ =>
            {
                IsChanged = true;
                _point.Type = _;
                Name = $"{_.GetPlaningMissionPointName()} {Index}";
            }).DisposeItWith(Disposable);
        
    }
    public PlaningMissionPointModel Point => _point;
    [Reactive]
    public int Index { get; set; }
    [Reactive]
    public string Name { get; set; }
    [Reactive]
    public MavCmd Type { get; set; }
    [Reactive]
    public bool IsChanged { get; set; }
    [Reactive]
    public PlaningMissionAnchor MissionAnchor { get; set; }
    [Reactive]
    public MaterialIconKind Icon { get; set; }
    [Reactive]
    public string Param1Title { get; set; } = RS.PlaningMissionPointViewModel_Param1Title;
    [Reactive]
    public string Param2Title { get; set; } = RS.PlaningMissionPointViewModel_Param2Title;
    [Reactive]
    public string Param3Title { get; set; } = RS.PlaningMissionPointViewModel_Param3Title;
    [Reactive]
    public string Param4Title { get; set; } = RS.PlaningMissionPointViewModel_Param4Title;
    
    public ReactiveCommand<Unit, Unit> Delete { get; }
    public PlaningMissionPointModel SaveToJson(SemVersion fileVersion)
    {
        return new PlaningMissionPointModel
        {
            Index = Index,
            Type = Type,
            Location = MissionAnchor?.Location ?? GeoPoint.Zero,
            Param1 = Point.Param1,
            Param2 = Point.Param2,
            Param3 = Point.Param3,
            Param4 = Point.Param4
        };
    }
    
    public virtual void CreateVehicleItems(IVehicleClient vehicle, ISdrClientDevice? sdr)
    {
        MissionItem missionItem = vehicle.Missions.Create();
        missionItem.Location.OnNext(Point.Location);
        missionItem.AutoContinue.OnNext(true);
        missionItem.Command.OnNext(Type);
        missionItem.Current.OnNext(false);
        missionItem.Frame.OnNext(MavFrame.MavFrameGlobalInt);
        missionItem.MissionType.OnNext(MavMissionType.MavMissionTypeMission);
        missionItem.Param1.OnNext(Point.Param1);
        missionItem.Param2.OnNext(Point.Param2);
        missionItem.Param3.OnNext(Point.Param3);
        missionItem.Param4.OnNext(Point.Param4);
    }
}