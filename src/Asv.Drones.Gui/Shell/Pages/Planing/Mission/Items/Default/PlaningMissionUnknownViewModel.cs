using System;
using Asv.Common;
using Asv.Drones.Gui.Api;
using Asv.Mavlink;
using Material.Icons;
using ReactiveUI;

namespace Asv.Drones.Gui;

public class PlaningMissionUnknownViewModel : PlaningMissionPointViewModel
{
    public PlaningMissionUnknownViewModel(PlaningMissionPointModel point, PlaningMissionViewModel mission,
        IPlaningMission svc, ILocalizationService loc) : base(point, mission)
    {
        MissionAnchor = new PlaningMissionUnknownAnchor(point);

        Icon = MaterialIconKind.QuestionMark;

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
    }

    public override void CreateVehicleItems(IVehicleClient vehicle, ISdrClientDevice? sdr)
    {
    }
}