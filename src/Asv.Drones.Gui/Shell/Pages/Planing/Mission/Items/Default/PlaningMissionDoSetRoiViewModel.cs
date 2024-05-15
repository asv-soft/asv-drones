using System;
using Asv.Common;
using Asv.Drones.Gui.Api;
using Material.Icons;
using ReactiveUI;

namespace Asv.Drones.Gui;

public class PlaningMissionDoSetRoiViewModel : PlaningMissionPointViewModel
{
    public PlaningMissionDoSetRoiViewModel(PlaningMissionPointModel point, PlaningMissionViewModel mission,
        IPlaningMission svc, ILocalizationService loc) : base(point, mission)
    {
        MissionAnchor = new PlaningMissionDoSetRoiAnchor(point);

        Icon = MaterialIconKind.ImageFilterCenterFocus;

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

        Param1Title = RS.PlaningMissionDoSetRoiViewModel_Param1Title;
        Param2Title = RS.PlaningMissionDoSetRoiViewModel_Param2Title;
        Param3Title = RS.PlaningMissionDoSetRoiViewModel_Param3Title;
        Param4Title = RS.PlaningMissionDoSetRoiViewModel_Param4Title;
    }
}