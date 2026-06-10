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
    public const string Id = "heading";
    private const AsvColorKind DefaultStatusColor = AsvColorKind.Info5;

    public string ItemId => Id;
    public string DisplayName => RS.HeadingUavIndicatorViewModel_Heading;

    public bool CanCreate(in IClientDevice device) =>
        device.GetMicroservice<IPositionClientEx>() is not null;

    public ITelemetryItem Create(in IClientDevice device)
    {
        ArgumentNullException.ThrowIfNull(device);

        var headingObservable = device
            .GetRequiredMicroservice<IPositionClientEx>()
            .Yaw.Select(Math.Truncate)
            .Prepend(double.NaN);

        return new HeadingTelemetryItemViewModel(
            Id,
            loggerFactory,
            unitService,
            headingObservable,
            DefaultStatusColor
        );
    }

    public ITelemetryItem CreatePreview()
    {
        var headingObservable = Observable.Return(29d).Concat(Observable.Never<double>());

        return new HeadingTelemetryItemViewModel(
            Id,
            loggerFactory,
            unitService,
            headingObservable,
            DefaultStatusColor
        );
    }
}
