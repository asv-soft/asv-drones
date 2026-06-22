using System;
using Asv.Avalonia;
using Asv.Drones.Api;
using Asv.IO;
using Asv.Mavlink;
using Material.Icons;
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

    public bool CanCreate(in IClientDevice device) =>
        device.GetMicroservice<IPositionClientEx>() is not null;

    public IRttBoxViewModel Create(in IClientDevice device)
    {
        ArgumentNullException.ThrowIfNull(device);

        var azimuthObservable = device
            .GetRequiredMicroservice<IPositionClientEx>()
            .Yaw.Select(value => Math.Round(value, 2))
            .Prepend(double.NaN);

        return InternalCreate(azimuthObservable);
    }

    public IRttBoxViewModel CreatePreview()
    {
        var azimuthObservable = Observable.Return(39d).Concat(Observable.Never<double>());

        return InternalCreate(azimuthObservable);
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
            Header = RS.UavRttItem_Azimuth,
            Icon = MaterialIconKind.SunAzimuth,
            Status = DefaultStatusColor,
            FormatString = "F2",
        };
    }
}
