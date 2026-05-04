using System;
using System.Diagnostics.CodeAnalysis;
using Asv.Avalonia;
using Material.Icons;
using Microsoft.Extensions.Logging;
using R3;

namespace Asv.Drones;

#pragma warning disable SA1313
public record AngleRttBoxData(double Pitch, double Roll, IUnitItem AngleUnit);
#pragma warning restore SA1313

public class AngleUavRttIndicatorViewModel : TwoColumnRttBoxViewModel<AngleRttBoxData>
{
    [SetsRequiredMembers]
    public AngleUavRttIndicatorViewModel()
        : this(
            nameof(AngleUavRttIndicator),
            DesignTime.LoggerFactory,
            new ReactiveProperty<double>(30),
            new ReactiveProperty<double>(10),
            DeviceTelemetryDesignPreview.Unit(AngleUnit.Id).CurrentUnitItem,
            DeviceTelemetryDesignPreview.DefaultStatusColor
        )
    {
        DesignTime.ThrowIfNotDesignMode();
    }

    [SetsRequiredMembers]
    public AngleUavRttIndicatorViewModel(
        NavigationId id,
        ILoggerFactory loggerFactory,
        ReactiveProperty<double> pitchAngle,
        ReactiveProperty<double> rollAngle,
        SynchronizedReactiveProperty<IUnitItem> currentAngleUnitItem,
        AsvColorKind defaultStatusColor,
        TimeSpan? networkErrorTimeout = null
    )
        : base(
            id,
            loggerFactory,
            pitchAngle
                .CombineLatest(
                    rollAngle,
                    currentAngleUnitItem,
                    (agl, msl, unit) => new AngleRttBoxData(agl, msl, unit)
                )
                .ObserveOnUIThreadDispatcher()
                .ThrottleLast(TimeSpan.FromMilliseconds(200)),
            networkErrorTimeout
        )
    {
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
}
