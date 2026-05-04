using System;
using Asv.Avalonia;
using Asv.Drones.Api;
using Asv.IO;
using Asv.Mavlink;
using R3;

namespace Asv.Drones;

public sealed class AltitudeTelemetryItemFactory(IUnitService unitService) : ITelemetryItemFactory
{
    public const string Id = "altitude-uav";
    private const AsvColorKind DefaultStatusColor = AsvColorKind.Info5;

    public string ItemId => Id;
    public string DisplayName => RS.UavRttItem_Altitude;

    public bool CanCreate(in IClientDevice device) =>
        device.GetMicroservice<IPositionClientEx>() is not null;

    public ITelemetryItem Create(in IClientDevice device)
    {
        ArgumentNullException.ThrowIfNull(device);

        var positionClientEx = device.GetRequiredMicroservice<IPositionClientEx>();

        var altitudeObservable = positionClientEx
            .Base.GlobalPosition.Select(pld =>
                (
                    AltitudeAgl: Math.Truncate((pld?.RelativeAlt ?? double.NaN) / 1000d),
                    AltitudeMsl: Math.Truncate((pld?.Alt ?? double.NaN) / 1000d)
                )
            )
            .Prepend((AltitudeAgl: double.NaN, AltitudeMsl: double.NaN))
            .CombineLatest(
                unitService.Units[AltitudeUnit.Id].CurrentUnitItem,
                (value, unit) => new AltitudeRttBoxData(value.AltitudeAgl, value.AltitudeMsl, unit)
            );

        return new AltitudeUavIndicatorViewModel(Id, altitudeObservable, DefaultStatusColor);
    }

    public ITelemetryItem CreatePreview()
    {
        var altitudeObservable = unitService
            .Units[AltitudeUnit.Id]
            .CurrentUnitItem.Select(unit => new AltitudeRttBoxData(10d, 14d, unit));

        return new AltitudeUavIndicatorViewModel(Id, altitudeObservable, DefaultStatusColor);
    }
}
