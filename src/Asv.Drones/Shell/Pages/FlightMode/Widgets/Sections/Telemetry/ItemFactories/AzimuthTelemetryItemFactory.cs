using System;
using Asv.Avalonia;
using Asv.Drones.Api;
using Asv.IO;
using Asv.Mavlink;
using Microsoft.Extensions.Logging;
using R3;

namespace Asv.Drones;

public sealed class AzimuthTelemetryItemFactory(
    IUnitService unitService,
    ILoggerFactory loggerFactory
) : ITelemetryItemFactory
{
    public const string Id = "azimuth";
    private const AsvColorKind DefaultStatusColor = AsvColorKind.Info5;

    public string ItemId => Id;
    public string DisplayName => RS.UavRttItem_Azimuth;

    public bool CanCreate(in IClientDevice device) =>
        device.GetMicroservice<IPositionClientEx>() is not null;

    public ITelemetryItem Create(in IClientDevice device)
    {
        ArgumentNullException.ThrowIfNull(device);

        var azimuthObservable = device
            .GetRequiredMicroservice<IPositionClientEx>()
            .Yaw.Select(value => Math.Round(value, 2))
            .Prepend(double.NaN);

        return new AzimuthTelemetryItemViewModel(
            Id,
            loggerFactory,
            unitService,
            azimuthObservable,
            DefaultStatusColor
        );
    }

    public ITelemetryItem CreatePreview()
    {
        var azimuthObservable = Observable.Return(39d).Concat(Observable.Never<double>());

        return new AzimuthTelemetryItemViewModel(
            Id,
            loggerFactory,
            unitService,
            azimuthObservable,
            DefaultStatusColor
        );
    }
}
