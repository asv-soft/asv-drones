using System;
using Asv.Avalonia;
using Asv.Common;
using Asv.Drones.Api;
using Asv.IO;
using Asv.Mavlink;
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
    public string DisplayName => RS.HomeAzimuthUavIndicatorViewModel_HomeAzimuth;

    public bool CanCreate(in IClientDevice device) =>
        device.GetMicroservice<IPositionClientEx>() is not null;

    public ITelemetryItem Create(in IClientDevice device)
    {
        ArgumentNullException.ThrowIfNull(device);

        var positionClientEx = device.GetRequiredMicroservice<IPositionClientEx>();
        var homeAzimuthObservable = positionClientEx
            .Current.Where(_ => positionClientEx.Home.CurrentValue.HasValue)
            .ThrottleLast(TimeSpan.FromMilliseconds(200))
            .Select(p => p.Azimuth(positionClientEx.Home.CurrentValue ?? GeoPoint.NaN))
            .Prepend(double.NaN);

        return new HomeAzimuthTelemetryItemViewModel(
            Id,
            loggerFactory,
            unitService,
            homeAzimuthObservable,
            DefaultStatusColor
        );
    }

    public ITelemetryItem CreatePreview()
    {
        var homeAzimuthObservable = Observable.Return(30d).Concat(Observable.Never<double>());

        return new HomeAzimuthTelemetryItemViewModel(
            Id,
            loggerFactory,
            unitService,
            homeAzimuthObservable,
            DefaultStatusColor
        );
    }
}
