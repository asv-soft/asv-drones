using System;
using Asv.Avalonia;
using Asv.Drones.Api;
using Asv.IO;
using Asv.Mavlink;
using Microsoft.Extensions.Logging;
using R3;

namespace Asv.Drones;

public sealed class AngleTelemetryItemFactory(IUnitService unitService) : ITelemetryItemFactory
{
    public const string Id = "angle";
    private const AsvColorKind DefaultStatusColor = AsvColorKind.Info5;

    public string ItemId => Id;
    public string DisplayName => RS.AngleUavRttIndicatorViewModel_Angle;

    public bool CanCreate(in IClientDevice device) =>
        device.GetMicroservice<IPositionClientEx>() is not null;

    public ITelemetryItem Create(in IClientDevice device)
    {
        ArgumentNullException.ThrowIfNull(device);

        var positionClientEx = device.GetRequiredMicroservice<IPositionClientEx>();

        var angleObservable = positionClientEx
            .Pitch.Prepend(double.NaN)
            .CombineLatest(
                positionClientEx.Roll.Prepend(double.NaN),
                unitService.Units[AngleUnit.Id].CurrentUnitItem,
                (pitch, roll, unit) => new AngleRttBoxData(pitch, roll, unit)
            );

        return new AngleTelemetryItemViewModel(Id, angleObservable, DefaultStatusColor);
    }

    public ITelemetryItem CreatePreview()
    {
        var angleObservable = unitService
            .Units[AngleUnit.Id]
            .CurrentUnitItem.Select(unit => new AngleRttBoxData(30d, 10d, unit));

        return new AngleTelemetryItemViewModel(Id, angleObservable, DefaultStatusColor);
    }
}
