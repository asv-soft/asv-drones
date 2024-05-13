using System;
using Asv.Common;
using Asv.Drones.Gui.Api;
using Material.Icons;
using ReactiveUI;

namespace Asv.Drones.Gui;

public class PlaningMissionNavWaypointViewModel : PlaningMissionPointViewModel
{
    public PlaningMissionNavWaypointViewModel(PlaningMissionPointModel point, PlaningMissionViewModel mission,
        IPlaningMission svc, ILocalizationService loc) : base(point, mission)
    {
        MissionAnchor = new PlaningMissionNavWaypointAnchor(point);

        Icon = MaterialIconKind.Location;

        this.WhenAnyValue(_ => _.Index)
            .Subscribe(_ =>
            {
                MissionAnchor.Index = _;
                IsChanged = true;
            }).DisposeItWith(Disposable);

        MissionAnchor.WhenAnyValue(_ => _.Location)
            .Subscribe(_ =>
            {
                Point.Location = _;
                IsChanged = true;
            }).DisposeItWith(Disposable);

        Param1Title = RS.PlaningMissionNavWaypointViewModel_Param1Title;
        Param2Title = RS.PlaningMissionNavWaypointViewModel_Param2Title;
        Param3Title = RS.PlaningMissionNavWaypointViewModel_Param3Title;
        Param4Title = RS.PlaningMissionNavWaypointViewModel_Param4Title;
    }
}