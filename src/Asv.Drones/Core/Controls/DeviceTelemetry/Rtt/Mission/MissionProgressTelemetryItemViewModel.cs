using System.Diagnostics.CodeAnalysis;
using Asv.Avalonia;
using Asv.Drones.Api;
using Material.Icons;
using Microsoft.Extensions.Logging;
using R3;

namespace Asv.Drones;

public class MissionProgressTelemetryItemViewModel
    : KeyValueRttBoxViewModel<MissionProgressRttBoxData>,
        ITelemetryItem
{
    [SetsRequiredMembers]
    public MissionProgressTelemetryItemViewModel()
        : this(
            MissionProgressTelemetryItemFactory.Id,
            DesignTime.LoggerFactory,
            DeviceTelemetryDesignPreview
                .UnitService.Units[DistanceUnit.Id]
                .CurrentUnitItem.Select(unit => new MissionProgressRttBoxData(
                    900d,
                    0.7d,
                    900d,
                    unit,
                    DesignTime.UnitService.Units[TimeSpanUnit.Id].CurrentUnitItem.Value
                )),
            DeviceTelemetryDesignPreview.DefaultStatusColor
        )
    {
        DesignTime.ThrowIfNotDesignMode();
    }

    [SetsRequiredMembers]
    public MissionProgressTelemetryItemViewModel(
        string id,
        ILoggerFactory loggerFactory,
        Observable<MissionProgressRttBoxData> progressData,
        AsvColorKind defaultStatusColor,
        TimeSpan? networkErrorTimeout = null
    )
        : base(
            id,
            loggerFactory,
            progressData.ObserveOnUIThreadDispatcher().ThrottleLast(TimeSpan.FromMilliseconds(200)),
            networkErrorTimeout
        )
    {
        ItemId = id;
        Header = RS.MissionProgressTelemetry_DisplayName;
        Icon = MaterialIconKind.MapMarkerDistance;
        UpdateAction = (model, changes) =>
        {
            model[
                0,
                RS.MissionProgressTelemetry_RemainingDistance,
                changes.DistanceUnit.Symbol
            ].ValueString = changes.DistanceUnit.PrintFromSi(changes.RemainingDistance, "F2");
            model[1, RS.MissionTelemetry_RemainingTime, changes.TimeSpanUnit.Symbol].ValueString =
                double.IsNaN(changes.RemainingTime) || double.IsInfinity(changes.RemainingTime)
                    ? "-"
                    : changes.TimeSpanUnit.PrintFromSi(changes.RemainingTime, "F0");
            if (double.IsNaN(changes.Progress) || double.IsInfinity(changes.Progress))
            {
                Progress = 0;
                ProgressStatus = null;
                return;
            }

            Progress = changes.Progress;
            ProgressStatus = defaultStatusColor;
        };
        Status = defaultStatusColor;
    }

    public string ItemId { get; }
}

#pragma warning disable SA1313
public record MissionProgressRttBoxData(
    double RemainingDistance,
    double Progress,
    double RemainingTime,
    IUnitItem DistanceUnit,
    IUnitItem TimeSpanUnit
);
#pragma warning restore SA1313
