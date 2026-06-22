using System;
using Asv.Avalonia;
using Asv.Common;
using Asv.Drones.Api;
using Asv.IO;
using Asv.Mavlink;
using Material.Icons;
using Microsoft.Extensions.Logging;
using R3;

namespace Asv.Drones;

public sealed class HomeAzimuthTelemetryItemFactory(
    IUnitService unitService,
    ILoggerFactory loggerFactory
) : ITelemetryItemFactory
{
    public const string Id = "home-azimuth";
    private const AsvColorKind DefaultStatusColor = AsvColorKind.Info5;

    public string ItemId => Id;

    public bool CanCreate(in IClientDevice device) =>
        device.GetMicroservice<IPositionClientEx>() is not null;

    public IRttBoxViewModel Create(in IClientDevice device)
    {
        ArgumentNullException.ThrowIfNull(device);

        var positionClientEx = device.GetRequiredMicroservice<IPositionClientEx>();
        var homeAzimuthObservable = positionClientEx
            .Current.Where(_ => positionClientEx.Home.CurrentValue.HasValue)
            .ThrottleLast(TimeSpan.FromMilliseconds(200))
            .Select(p => p.Azimuth(positionClientEx.Home.CurrentValue ?? GeoPoint.NaN))
            .Prepend(double.NaN);

        return InternalCreate(homeAzimuthObservable);
    }

    public IRttBoxViewModel CreatePreview()
    {
        var homeAzimuthObservable = Observable.Return(30d).Concat(Observable.Never<double>());

        return InternalCreate(homeAzimuthObservable);
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
            Header = RS.HomeAzimuthUavIndicatorViewModel_HomeAzimuth,
            ShortHeader = RS.HomeAzimuthUavIndicatorViewModel_HomeAzimuth_Short,
            Icon = MaterialIconKind.Home,
            Status = DefaultStatusColor,
            FormatString = "F0",
        };
    }
}
