using System.Diagnostics.CodeAnalysis;
using Asv.Avalonia;
using Asv.Drones.Api;
using Material.Icons;
using Microsoft.Extensions.Logging;
using R3;

namespace Asv.Drones;

public class MissionDistanceTelemetryItemViewModel
    : KeyValueRttBoxViewModel<MissionDistanceRttBoxData>,
        ITelemetryItem
{
    [SetsRequiredMembers]
    public MissionDistanceTelemetryItemViewModel()
        : this(
            MissionDistanceTelemetryItemFactory.Id,
            DesignTime.LoggerFactory,
            DeviceTelemetryDesignPreview
                .UnitService.Units[DistanceUnit.Id]
                .CurrentUnitItem.Select(unit => new MissionDistanceRttBoxData(1100d, 1000d, unit)),
            DeviceTelemetryDesignPreview.DefaultStatusColor
        )
    {
        DesignTime.ThrowIfNotDesignMode();
    }

    [SetsRequiredMembers]
    public MissionDistanceTelemetryItemViewModel(
        string id,
        ILoggerFactory loggerFactory,
        Observable<MissionDistanceRttBoxData> distanceData,
        AsvColorKind defaultStatusColor,
        TimeSpan? networkErrorTimeout = null
    )
        : base(
            id,
            loggerFactory,
            distanceData.ObserveOnUIThreadDispatcher().ThrottleLast(TimeSpan.FromMilliseconds(200)),
            networkErrorTimeout
        )
    {
        ItemId = id;
        Header = RS.MissionDistanceTelemetryItemViewModel_Header;
        Icon = MaterialIconKind.LocationDistance;
        UpdateAction = (model, changes) =>
        {
            model[
                0,
                RS.MissionDistanceTelemetry_DisplayName,
                changes.DistanceUnit.Symbol
            ].ValueString = changes.DistanceUnit.PrintFromSi(changes.TotalDistance, "F2");
            model[
                1,
                RS.MissionDistanceTelemetry_MissionDistance,
                changes.DistanceUnit.Symbol
            ].ValueString = changes.DistanceUnit.PrintFromSi(changes.MissionDistance, "F2");
        };
        Status = defaultStatusColor;
    }

    public string ItemId { get; }
}

#pragma warning disable SA1313
public record MissionDistanceRttBoxData(
    double TotalDistance,
    double MissionDistance,
    IUnitItem DistanceUnit
);
#pragma warning restore SA1313
