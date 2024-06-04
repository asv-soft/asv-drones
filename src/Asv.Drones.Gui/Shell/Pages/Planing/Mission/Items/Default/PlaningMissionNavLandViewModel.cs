using System;
using Asv.Common;
using Asv.Drones.Gui.Api;
using Material.Icons;
using ReactiveUI;

namespace Asv.Drones.Gui;

public class PlaningMissionNavLandViewModel : PlaningMissionPointViewModel
{
    public PlaningMissionNavLandViewModel(PlaningMissionPointModel point, PlaningMissionViewModel mission,
        IPlaningMission svc, ILocalizationService loc) : base(point, mission)
    {
        MissionAnchor = new PlaningMissionNavLandAnchor(point);

        Icon = MaterialIconKind.FlightLand;

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

        Param1Title = RS.PlaningMissionNavLandViewModel_Param1Title;
        Param2Title = RS.PlaningMissionNavLandViewModel_Param2Title;
        Param3Title = RS.PlaningMissionNavLandViewModel_Param3Title;
        Param4Title = RS.PlaningMissionNavLandViewModel_Param4Title;
    }
}