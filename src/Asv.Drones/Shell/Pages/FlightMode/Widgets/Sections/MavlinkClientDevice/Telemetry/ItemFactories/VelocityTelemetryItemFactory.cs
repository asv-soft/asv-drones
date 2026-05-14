using System;
using Asv.Avalonia;
using Asv.Drones.Api;
using Asv.IO;
using Asv.Mavlink;
using Microsoft.Extensions.Logging;
using R3;

namespace Asv.Drones;

public sealed class VelocityTelemetryItemFactory(
    IUnitService unitService,
    ILoggerFactory loggerFactory
) : ITelemetryItemFactory
{
    public const string Id = "velocity-uav";
    private const AsvColorKind DefaultStatusColor = AsvColorKind.Info5;

    public string ItemId => Id;
    public string DisplayName => RS.UavRttItem_Velocity;

    public bool CanCreate(in IClientDevice device) =>
        device.GetMicroservice<IGnssClientEx>() is not null;

    public ITelemetryItem Create(in IClientDevice device)
    {
        ArgumentNullException.ThrowIfNull(device);

        var gnssClient =
            device.GetMicroservice<IGnssClientEx>()
            ?? throw new ArgumentException(
                $"Unable to load {nameof(IGnssClientEx)} from {device.Id}"
            );

        var velocity = new ReactiveProperty<double>(double.NaN);

        var indicator = new VelocityUavIndicatorViewModel(
            Id,
            loggerFactory,
            unitService,
            velocity,
            DefaultStatusColor
        );

        return new TelemetryItemViewModel(
            Id,
            indicator,
            loggerFactory,
            velocity,
            gnssClient
                .Main.GroundVelocity.ObserveOnUIThreadDispatcher()
                .Select(Math.Truncate)
                .Subscribe(x => velocity.Value = x)
        );
    }

    public ITelemetryItem CreatePreview()
    {
        var velocity = new ReactiveProperty<double>(19.9);

        var indicator = new VelocityUavIndicatorViewModel(
            Id,
            loggerFactory,
            unitService,
            velocity,
            DefaultStatusColor
        );

        return new TelemetryItemViewModel(Id, indicator, loggerFactory, velocity);
    }
}
