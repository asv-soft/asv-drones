using System;
using Asv.Avalonia;
using Asv.Drones.Api;
using Asv.IO;
using Asv.Mavlink;
using Material.Icons;
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

    public bool CanCreate(in IClientDevice device) =>
        device.GetMicroservice<IPositionClientEx>() is not null;

    public IRttBoxViewModel Create(in IClientDevice device)
    {
        ArgumentNullException.ThrowIfNull(device);

        var headingObservable = device
            .GetRequiredMicroservice<IPositionClientEx>()
            .Yaw.Select(Math.Truncate)
            .Prepend(double.NaN);

        return InternalCreate(headingObservable);
    }

    public IRttBoxViewModel CreatePreview()
    {
        var headingObservable = Observable.Return(29d).Concat(Observable.Never<double>());

        return InternalCreate(headingObservable);
    }

    private IRttBoxViewModel InternalCreate(Observable<double> observable)
    {
        return new SplitDigitRttBoxViewModel(
            Id,
            loggerFactory,
            unitService,
            AngleUnit.Id,
            observable,
            null
        )
        {
            Header = RS.HeadingUavIndicatorViewModel_Heading,
            ShortHeader = RS.HeadingUavIndicatorViewModel_Heading_Short,
            Icon = MaterialIconKind.SunAzimuth,
            Status = DefaultStatusColor,
            FormatString = "F0",
        };
    }
}
