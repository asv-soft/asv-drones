using System;
using Asv.Avalonia;
using Asv.Drones.Api;
using Asv.IO;
using Asv.Mavlink;
using Microsoft.Extensions.Logging;
using R3;

namespace Asv.Drones;

public sealed class AltitudeTelemetryItemFactory(
    IUnitService unitService,
    ILoggerFactory loggerFactory
) : ITelemetryItemFactory
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

        var positionClientEx =
            device.GetMicroservice<IPositionClientEx>()
            ?? throw new ArgumentException(
                $"Unable to load {nameof(IPositionClientEx)} from {device.Id}"
            );

        var altitudeAgl = new ReactiveProperty<double>(double.NaN);
        var altitudeMsl = new ReactiveProperty<double>(double.NaN);

        var indicator = new AltitudeUavIndicatorViewModel(
            Id,
            loggerFactory,
            altitudeAgl,
            altitudeMsl,
            unitService.Units[AltitudeUnit.Id].CurrentUnitItem,
            DefaultStatusColor
        );

        return new TelemetryItemViewModel(
            Id,
            indicator,
            loggerFactory,
            altitudeAgl,
            altitudeMsl,
            positionClientEx
                .Base.GlobalPosition.ObserveOnUIThreadDispatcher()
                .Select(pld => Math.Truncate((pld?.RelativeAlt ?? double.NaN) / 1000d))
                .Subscribe(x => altitudeAgl.Value = x),
            positionClientEx
                .Base.GlobalPosition.ObserveOnUIThreadDispatcher()
                .Select(pld => Math.Truncate((pld?.Alt ?? double.NaN) / 1000d))
                .Subscribe(x => altitudeMsl.Value = x)
        );
    }

    public ITelemetryItem CreatePreview()
    {
        var altitudeAgl = new ReactiveProperty<double>(10);
        var altitudeMsl = new ReactiveProperty<double>(14);

        var indicator = new AltitudeUavIndicatorViewModel(
            Id,
            loggerFactory,
            altitudeAgl,
            altitudeMsl,
            unitService.Units[AltitudeUnit.Id].CurrentUnitItem,
            DefaultStatusColor
        );

        return new TelemetryItemViewModel(Id, indicator, loggerFactory, altitudeAgl, altitudeMsl);
    }
}
