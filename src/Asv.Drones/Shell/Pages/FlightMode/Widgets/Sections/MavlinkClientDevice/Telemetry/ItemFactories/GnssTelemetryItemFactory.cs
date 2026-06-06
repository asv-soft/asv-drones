using System;
using Asv.Avalonia;
using Asv.Drones.Api;
using Asv.IO;
using Asv.Mavlink;
using Microsoft.Extensions.Logging;
using R3;

namespace Asv.Drones;

public sealed class GnssTelemetryItemFactory(ILoggerFactory loggerFactory) : ITelemetryItemFactory
{
    public const string Id = "gnss-uav";
    private const AsvColorKind DefaultStatusColor = AsvColorKind.Info5;

    public string ItemId => Id;
    public string DisplayName => RS.UavRttItem_GNSS;

    public bool CanCreate(in IClientDevice device) =>
        device.GetMicroservice<IGnssClientEx>() is not null;

    public ITelemetryItem Create(in IClientDevice device)
    {
        ArgumentNullException.ThrowIfNull(device);

        var gnssClient = device.GetRequiredMicroservice<IGnssClientEx>();
        var gnssObservable = gnssClient
            .Main.Info.Select(info => new GnssRttBoxData(
                info.SatellitesVisible,
                info.Hdop ?? double.NaN,
                info.Vdop ?? double.NaN,
                info.FixType
            ))
            .Prepend(
                new GnssRttBoxData(
                    0,
                    double.NaN,
                    double.NaN,
                    Mavlink.Common.GpsFixType.GpsFixTypeNoGps
                )
            );

        return new GnssUavIndicatorViewModel(Id, loggerFactory, gnssObservable, DefaultStatusColor);
    }

    public ITelemetryItem CreatePreview()
    {
        var gnssObservable = Observable
            .Return(new GnssRttBoxData(10, 2d, 4d, Mavlink.Common.GpsFixType.GpsFixTypeDgps))
            .Concat(Observable.Never<GnssRttBoxData>());

        return new GnssUavIndicatorViewModel(Id, loggerFactory, gnssObservable, DefaultStatusColor);
    }
}
