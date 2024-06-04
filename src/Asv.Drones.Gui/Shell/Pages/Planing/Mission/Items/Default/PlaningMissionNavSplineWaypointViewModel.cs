using System;
using Asv.Common;
using Asv.Drones.Gui.Api;
using Material.Icons;
using ReactiveUI;

namespace Asv.Drones.Gui;

public class PlaningMissionNavSplineWaypointViewModel : PlaningMissionPointViewModel
{
    public PlaningMissionNavSplineWaypointViewModel(PlaningMissionPointModel point, PlaningMissionViewModel mission,
        IPlaningMission svc, ILocalizationService loc) : base(point, mission)
    {
        MissionAnchor = new PlaningMissionNavSplineWaypointAnchor(point);

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

        Param1Title = RS.PlaningMissionNavSplineWaypointViewModel_Param1Title;
        Param2Title = RS.PlaningMissionNavSplineWaypointViewModel_Param2Title;
        Param3Title = RS.PlaningMissionNavSplineWaypointViewModel_Param3Title;
        Param4Title = RS.PlaningMissionNavSplineWaypointViewModel_Param4Title;
    }
}