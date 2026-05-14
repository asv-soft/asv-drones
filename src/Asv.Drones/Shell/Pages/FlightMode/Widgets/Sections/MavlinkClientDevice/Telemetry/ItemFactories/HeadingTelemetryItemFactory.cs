using System;
using Asv.Avalonia;
using Asv.Drones.Api;
using Asv.IO;
using Asv.Mavlink;
using Microsoft.Extensions.Logging;
using R3;

namespace Asv.Drones;

public sealed class HeadingTelemetryItemFactory(
    IUnitService unitService,
    ILoggerFactory loggerFactory
) : ITelemetryItemFactory
{
    public const string Id = "heading-uav";
    private const AsvColorKind DefaultStatusColor = AsvColorKind.Info5;

    public string ItemId => Id;
    public string DisplayName => RS.HeadingUavIndicatorViewModel_Heading;

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

        var heading = new ReactiveProperty<double>(double.NaN);

        var indicator = new HeadingUavIndicatorViewModel(
            Id,
            loggerFactory,
            unitService,
            heading,
            DefaultStatusColor
        );

        return new TelemetryItemViewModel(
            Id,
            indicator,
            loggerFactory,
            heading,
            positionClientEx
                .Yaw.ObserveOnUIThreadDispatcher()
                .Select(Math.Truncate)
                .Subscribe(x => heading.Value = x)
        );
    }

    public ITelemetryItem CreatePreview()
    {
        var heading = new ReactiveProperty<double>(29);

        var indicator = new HeadingUavIndicatorViewModel(
            Id,
            loggerFactory,
            unitService,
            heading,
            DefaultStatusColor
        );

        return new TelemetryItemViewModel(Id, indicator, loggerFactory, heading);
    }
}
