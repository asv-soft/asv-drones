using System;
using Asv.Avalonia;
using Asv.Drones.Api;
using Asv.IO;
using Asv.Mavlink;
using Microsoft.Extensions.Logging;
using R3;

namespace Asv.Drones;

public sealed class AngleTelemetryItemFactory(
    IUnitService unitService,
    ILoggerFactory loggerFactory
) : ITelemetryItemFactory
{
    public const string Id = "angle-uav";
    private const AsvColorKind DefaultStatusColor = AsvColorKind.Info5;

    public string ItemId => Id;
    public string DisplayName => RS.AngleUavRttIndicatorViewModel_Angle;

    public bool CanCreate(in IClientDevice device) =>
        device.GetMicroservice<IPositionClientEx>() is not null;

    public ITelemetryItem Create(in IClientDevice device)
    {
        ArgumentNullException.ThrowIfNull(device);

        var positionClientEx =
            device.GetMicroservice<IPositionClientEx>()
            ?? throw new ArgumentException(
                $"Unable to load {nameof(IPositionClientEx)} from {device.Id}"
            );

        var roll = new ReactiveProperty<double>(double.NaN);
        var pitch = new ReactiveProperty<double>(double.NaN);

        var indicator = new AngleUavRttIndicatorViewModel(
            Id,
            loggerFactory,
            pitch,
            roll,
            unitService.Units[AngleUnit.Id].CurrentUnitItem,
            DefaultStatusColor
        );

        return new TelemetryItemViewModel(
            Id,
            indicator,
            loggerFactory,
            roll,
            pitch,
            positionClientEx.Roll.Subscribe(x => roll.Value = x),
            positionClientEx.Pitch.Subscribe(x => pitch.Value = x)
        );
    }

    public ITelemetryItem CreatePreview()
    {
        var pitch = new ReactiveProperty<double>(30);
        var roll = new ReactiveProperty<double>(10);

        var indicator = new AngleUavRttIndicatorViewModel(
            Id,
            loggerFactory,
            pitch,
            roll,
            unitService.Units[AngleUnit.Id].CurrentUnitItem,
            DefaultStatusColor
        );

        return new TelemetryItemViewModel(Id, indicator, loggerFactory, pitch, roll);
    }
}
