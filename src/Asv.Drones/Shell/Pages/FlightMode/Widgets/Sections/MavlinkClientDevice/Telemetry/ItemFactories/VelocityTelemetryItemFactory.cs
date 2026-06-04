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

        var velocityObservable = device
            .GetRequiredMicroservice<IGnssClientEx>()
            .Main.GroundVelocity.Select(Math.Truncate)
            .Prepend(double.NaN);

        return new VelocityUavIndicatorViewModel(
            Id,
            loggerFactory,
            unitService,
            velocityObservable,
            DefaultStatusColor
        );
    }

    public ITelemetryItem CreatePreview()
    {
        var velocityObservable = Observable.Return(19.9d).Concat(Observable.Never<double>());

        return new VelocityUavIndicatorViewModel(
            Id,
            loggerFactory,
            unitService,
            velocityObservable,
            DefaultStatusColor
        );
    }
}
