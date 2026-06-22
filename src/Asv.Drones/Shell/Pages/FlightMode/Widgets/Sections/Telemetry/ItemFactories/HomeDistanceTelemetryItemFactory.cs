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
    private const AsvColorKind DefaultStatusColor = AsvColorKind.Info5;

    public bool CanCreate(in IClientDevice device) =>
        device.GetMicroservice<IPositionClientEx>() is not null;

    public IRttBoxViewModel Create(in IClientDevice device)
    {
        ArgumentNullException.ThrowIfNull(device);

        var positionClient = device.GetRequiredMicroservice<IPositionClientEx>();

        return CreateItem(
            positionClient.HomeDistance.Skip(1).DistinctUntilChanged().Prepend(double.NaN)
        );
    }

    public IRttBoxViewModel CreatePreview()
    {
        return CreateItem(Observable.Return(100d).Concat(Observable.Never<double>()));
    }

    private IRttBoxViewModel CreateItem(Observable<double> distance)
    {
        return new SplitDigitRttBoxViewModel(
            Id,
            loggerFactory,
            unitService,
            DistanceUnit.Id,
            distance,
            null
        )
        {
            Icon = MaterialIconKind.Home,
            Header = RS.HomeDistanceTelemetry_DisplayName,
            Status = DefaultStatusColor,
            FormatString = "F2",
        };
    }
}
