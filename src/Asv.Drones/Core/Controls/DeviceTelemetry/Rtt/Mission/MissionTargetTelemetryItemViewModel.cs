using System;
using System.Diagnostics.CodeAnalysis;
using Asv.Avalonia;
using Asv.Drones.Api;
using Material.Icons;
using Microsoft.Extensions.Logging;
using R3;

namespace Asv.Drones;

public class MissionTargetTelemetryItemViewModel
    : KeyValueRttBoxViewModel<TargetDistanceRttBoxData>,
        ITelemetryItem
{
    [SetsRequiredMembers]
    public MissionTargetTelemetryItemViewModel()
        : this(
            MissionTargetTelemetryItemFactory.Id,
            DesignTime.LoggerFactory,
            DeviceTelemetryDesignPreview
                .UnitService.Units[DistanceUnit.Id]
                .CurrentUnitItem.Select(unit => new TargetDistanceRttBoxData(
                    100d,
                    3,
                    10d,
                    unit,
                    new TimeSpanHourMinuteSecondUnitItem()
                )),
            DeviceTelemetryDesignPreview.DefaultStatusColor
        )
    {
        DesignTime.ThrowIfNotDesignMode();
    }

    [SetsRequiredMembers]
    public MissionTargetTelemetryItemViewModel(
        string id,
        ILoggerFactory loggerFactory,
        Observable<TargetDistanceRttBoxData> targetData,
        AsvColorKind defaultStatusColor,
        TimeSpan? networkErrorTimeout = null
    )
        : base(
            id,
            loggerFactory,
            targetData.ObserveOnUIThreadDispatcher().ThrottleLast(TimeSpan.FromMilliseconds(200)),
            networkErrorTimeout
        )
    {
        ItemId = id;
        Header = RS.MissionTargetTelemetry_DispayName;
        Icon = MaterialIconKind.Target;
        UpdateAction = (model, changes) =>
        {
            model[
                0,
                RS.MissionTargetTelemetry_TargetDistance,
                changes.DistanceUnit.Symbol
            ].ValueString = changes.DistanceUnit.PrintFromSi(changes.TargetDistance, "F2");
            model[1, RS.MissionProgressView_TargetIndexRTT, null].ValueString =
                changes.TargetIndex.ToString();
            model[2, RS.MissionTelemetry_RemainingTime, changes.TimeSpanUnit.Symbol].ValueString =
                double.IsNaN(changes.RemainingTime) || double.IsInfinity(changes.RemainingTime)
                    ? "-"
                    : changes.TimeSpanUnit.PrintFromSi(changes.RemainingTime, "F0");
        };
        Status = defaultStatusColor;
    }

    public string ItemId { get; }
}

#pragma warning disable SA1313
public record TargetDistanceRttBoxData(
    double TargetDistance,
    ushort TargetIndex,
    double RemainingTime,
    IUnitItem DistanceUnit,
    IUnitItem TimeSpanUnit
);
#pragma warning restore SA1313
