using System;
using Asv.Avalonia;
using Asv.Drones.Api;
using Asv.IO;
using Asv.Mavlink;
using Material.Icons;
using Microsoft.Extensions.Logging;
using R3;

namespace Asv.Drones;

public sealed class HomeDistanceTelemetryItemFactory(
    IUnitService unitService,
    ILoggerFactory loggerFactory
) : ITelemetryItemFactory
{
    public const string Id = "home-distance";

    public string ItemId => Id;
    public string DisplayName => RS.HomeDistanceTelemetry_DisplayName;
    private const AsvColorKind DefaultStatusColor = AsvColorKind.Info5;

    public bool CanCreate(in IClientDevice device) =>
        device.GetMicroservice<IPositionClientEx>() is not null;

    public ITelemetryItem Create(in IClientDevice device)
    {
        ArgumentNullException.ThrowIfNull(device);

        var positionClient = device.GetRequiredMicroservice<IPositionClientEx>();

        return CreateItem(positionClient.HomeDistance.Prepend(double.NaN));
    }

    public ITelemetryItem CreatePreview()
    {
        return CreateItem(Observable.Return(100d).Concat(Observable.Never<double>()));
    }

    private DistanceTelemetryItemViewModel CreateItem(Observable<double> distance)
    {
        return new DistanceTelemetryItemViewModel(
            Id,
            loggerFactory,
            unitService,
            distance,
            MaterialIconKind.Home,
            DisplayName,
            DefaultStatusColor
        );
    }
}
