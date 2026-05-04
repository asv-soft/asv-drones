using System;
using System.Diagnostics.CodeAnalysis;
using Asv.Avalonia;
using Asv.Drones.Api;
using Material.Icons;
using Microsoft.Extensions.Logging;
using R3;

namespace Asv.Drones;

#pragma warning disable SA1313
public record AngleRttBoxData(double Pitch, double Roll, IUnitItem AngleUnit);
#pragma warning restore SA1313

public class AngleUavRttIndicatorViewModel
    : TwoColumnRttBoxViewModel<AngleRttBoxData>,
        ITelemetryItem
{
    [SetsRequiredMembers]
    public AngleUavRttIndicatorViewModel()
        : this(
            nameof(AngleUavRttIndicator),
            DeviceTelemetryDesignPreview
                .UnitService.Units[AngleUnit.Id]
                .CurrentUnitItem.Select(unit => new AngleRttBoxData(30d, 10d, unit)),
            DeviceTelemetryDesignPreview.DefaultStatusColor
        )
    {
        DesignTime.ThrowIfNotDesignMode();
    }

    [SetsRequiredMembers]
    public AngleUavRttIndicatorViewModel(
        string id,
        Observable<AngleRttBoxData> angleData,
        AsvColorKind defaultStatusColor,
        TimeSpan? networkErrorTimeout = null
    )
        : base(
            id,
            angleData.ObserveOnUIThreadDispatcher().ThrottleLast(TimeSpan.FromMilliseconds(200)),
            networkErrorTimeout
        )
    {
        ItemId = id;
        Header = RS.AngleUavRttIndicatorViewModel_Angle;
        Icon = MaterialIconKind.Altimeter;
        UpdateAction = (model, changes) =>
        {
            model.Left.ValueString = changes.AngleUnit.PrintFromSi(changes.Pitch, "F2");
            model.Right.ValueString = changes.AngleUnit.PrintFromSi(changes.Roll, "F2");

            model.Left.UnitSymbol = changes.AngleUnit.Symbol;
            model.Right.UnitSymbol = changes.AngleUnit.Symbol;
        };
        Status = defaultStatusColor;

        Left.Header = RS.AngleUavRttIndicatorViewModel_Pitch;
        Right.Header = RS.AngleUavRttIndicatorViewModel_Roll;
    }

    public string ItemId { get; }
}
