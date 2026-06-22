using System;
using Asv.Avalonia;
using Asv.Drones.Api;
using Asv.IO;
using Asv.Mavlink;
using Material.Icons;
using Microsoft.Extensions.Logging;
using R3;

namespace Asv.Drones;

public sealed class GnssTelemetryItemFactory(ILoggerFactory loggerFactory) : ITelemetryItemFactory
{
    public const string Id = "gnss";
    private const AsvColorKind DefaultStatusColor = AsvColorKind.Info5;
    private const int DangerSatelliteCount = 10;
    private static readonly Range WarningSatelliteAmount = 15..20;

    public string ItemId => Id;

    public bool CanCreate(in IClientDevice device) =>
        device.GetMicroservice<IGnssClientEx>() is not null;

    public IRttBoxViewModel Create(in IClientDevice device)
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

        return InternalCreate(gnssObservable);
    }

    public IRttBoxViewModel CreatePreview()
    {
        var gnssObservable = Observable
            .Return(new GnssRttBoxData(10, 2d, 4d, Mavlink.Common.GpsFixType.GpsFixTypeDgps))
            .Concat(Observable.Never<GnssRttBoxData>());

        return InternalCreate(gnssObservable);
    }

    private IRttBoxViewModel InternalCreate(Observable<GnssRttBoxData> observable)
    {
        return new KeyValueRttBoxViewModel<GnssRttBoxData>(Id, loggerFactory, observable, null)
        {
            Header = RS.UavRttItem_GNSS,
            Icon = MaterialIconKind.GpsFixed,
            UpdateAction = (model, changes) =>
            {
                model[
                    0,
                    RS.UavWidgetViewModel_GnssRttBox_SatellitesCount_Header,
                    null
                ].ValueString = changes.Satellites.ToString();
                model[1, RS.UavWidgetViewModel_GnssRttBox_Hdop_Header, null].ValueString =
                    changes.HdopCount.ToString("F2");
                model[2, RS.UavWidgetViewModel_GnssRttBox_Vdop_Header, null].ValueString =
                    changes.VdopCount.ToString("F2");
                model[3, RS.UavWidgetViewModel_GnssRttBox_Mode_Header, null].ValueString =
                    GpsFixTypeToString(changes.Mode);

                ChangeGnssStatus(model, changes.Satellites, changes.Mode);
            },
            Status = DefaultStatusColor,
        };
    }

    private static void ChangeGnssStatus(
        KeyValueRttBoxViewModel<GnssRttBoxData> rtt,
        int satellitesCount,
        Mavlink.Common.GpsFixType mode
    )
    {
        if (
            mode == Mavlink.Common.GpsFixType.GpsFixTypeRtkFloat
            || satellitesCount > WarningSatelliteAmount.Start.Value
            || satellitesCount < WarningSatelliteAmount.End.Value
        )
        {
            rtt.Status = AsvColorKind.Warning;
            return;
        }

        if (
            mode != Mavlink.Common.GpsFixType.GpsFixTypeRtkFixed
            || satellitesCount < DangerSatelliteCount
        )
        {
            rtt.Status = AsvColorKind.Error;
            return;
        }

        rtt.Status = DefaultStatusColor;
    }

    private static string GpsFixTypeToString(Mavlink.Common.GpsFixType type)
    {
        return type switch
        {
            Mavlink.Common.GpsFixType.GpsFixType2dFix => RS.GpsFixType_GpsFixType2dFix,
            Mavlink.Common.GpsFixType.GpsFixTypeRtkFloat => RS.GpsFixType_GpsFixTypeRtkFloat,
            Mavlink.Common.GpsFixType.GpsFixTypeRtkFixed => RS.GpsFixType_GpsFixTypeRtkFixed,
            Mavlink.Common.GpsFixType.GpsFixTypeDgps => RS.GpsFixType_GpsFixTypeDgps,
            Mavlink.Common.GpsFixType.GpsFixTypePpp => RS.GpsFixType_GpsFixTypePpp,
            Mavlink.Common.GpsFixType.GpsFixType3dFix => RS.GpsFixType_GpsFixType3dFix,
            Mavlink.Common.GpsFixType.GpsFixTypeStatic => RS.GpsFixType_GpsFixTypeStatic,
            Mavlink.Common.GpsFixType.GpsFixTypeNoGps => RS.GpsFixType_GpsFixTypeNoGps,
            _ => string.Empty,
        };
    }
}

#pragma warning disable SA1313
public record GnssRttBoxData(
    int Satellites,
    double HdopCount,
    double VdopCount,
    Mavlink.Common.GpsFixType Mode
);
#pragma warning restore SA1313
