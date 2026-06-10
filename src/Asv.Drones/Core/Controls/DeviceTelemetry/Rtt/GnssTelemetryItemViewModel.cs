using System;
using System.Diagnostics.CodeAnalysis;
using Asv.Avalonia;
using Asv.Drones.Api;
using Asv.Mavlink;
using Material.Icons;
using Microsoft.Extensions.Logging;
using R3;

namespace Asv.Drones;

#pragma warning disable SA1313
public record GnssRttBoxData(
    int Satellites,
    double HdopCount,
    double VdopCount,
    Mavlink.Common.GpsFixType Mode
);
#pragma warning restore SA1313

public class GnssTelemetryItemViewModel : KeyValueRttBoxViewModel<GnssRttBoxData>, ITelemetryItem
{
    private const int DangerSatelliteCount = 10;
    private static readonly Range WarningSatelliteAmount = 15..20;

    private readonly AsvColorKind _defaultStatusColor;

    [SetsRequiredMembers]
    public GnssTelemetryItemViewModel()
        : this(
            nameof(GnssTelemetryItemViewModel),
            DesignTime.LoggerFactory,
            Observable
                .Return(
                    new GnssRttBoxData(18, 0.9d, 1.2d, Mavlink.Common.GpsFixType.GpsFixTypeRtkFixed)
                )
                .Concat(Observable.Never<GnssRttBoxData>()),
            DeviceTelemetryDesignPreview.DefaultStatusColor
        )
    {
        DesignTime.ThrowIfNotDesignMode();
    }

    [SetsRequiredMembers]
    public GnssTelemetryItemViewModel(
        string id,
        ILoggerFactory loggerFactory,
        Observable<GnssRttBoxData> gnssData,
        AsvColorKind defaultStatusColor,
        TimeSpan? networkErrorTimeout = null
    )
        : base(
            id,
            loggerFactory,
            gnssData.ObserveOnUIThreadDispatcher().ThrottleLast(TimeSpan.FromMilliseconds(200)),
            networkErrorTimeout
        )
    {
        ItemId = id;
        _defaultStatusColor = defaultStatusColor;

        Header = RS.UavRttItem_GNSS;
        Icon = MaterialIconKind.GpsFixed;
        UpdateAction = (model, changes) =>
        {
            model[0, RS.UavWidgetViewModel_GnssRttBox_SatellitesCount_Header, null].ValueString =
                changes.Satellites.ToString();
            model[1, RS.UavWidgetViewModel_GnssRttBox_Hdop_Header, null].ValueString =
                changes.HdopCount.ToString("F2");
            model[2, RS.UavWidgetViewModel_GnssRttBox_Vdop_Header, null].ValueString =
                changes.VdopCount.ToString("F2");
            model[3, RS.UavWidgetViewModel_GnssRttBox_Mode_Header, null].ValueString =
                GpsFixTypeToString(changes.Mode);

            ChangeGnssStatus(changes.Satellites, changes.Mode);
        };
        Status = defaultStatusColor;
    }

    public string ItemId { get; }

    private void ChangeGnssStatus(int satellitesCount, Mavlink.Common.GpsFixType mode)
    {
        if (
            mode == Mavlink.Common.GpsFixType.GpsFixTypeRtkFloat
            || satellitesCount > WarningSatelliteAmount.Start.Value
            || satellitesCount < WarningSatelliteAmount.End.Value
        )
        {
            Status = AsvColorKind.Warning;
            return;
        }

        if (
            mode != Mavlink.Common.GpsFixType.GpsFixTypeRtkFixed
            || satellitesCount < DangerSatelliteCount
        )
        {
            Status = AsvColorKind.Error;
            return;
        }

        Status = _defaultStatusColor;
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
