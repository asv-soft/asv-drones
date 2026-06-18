using System;
using Asv.Avalonia;
using Asv.Drones.Api;
using Asv.IO;
using Asv.Mavlink;
using Microsoft.Extensions.Logging;
using R3;

namespace Asv.Drones;

public sealed class CurrentFlightModeTelemetryItemFactory(
    IUnitService unitService,
    ILoggerFactory loggerFactory
) : ITelemetryItemFactory
{
    public const string Id = "current-flight-mode";
    private const AsvColorKind DefaultStatusColor = AsvColorKind.Info5;

    public string ItemId => Id;
    public string DisplayName => RS.UavRttItem_Mode;

    public bool CanCreate(in IClientDevice device) =>
        device.GetMicroservice<IModeClient>() is not null;

    public ITelemetryItem Create(in IClientDevice device)
    {
        ArgumentNullException.ThrowIfNull(device);

        var currentMode = device
            .GetRequiredMicroservice<IModeClient>()
            .CurrentMode.Select(mode => mode.Name)
            .Prepend(string.Empty);

        var timeInAir =
            device
                .GetMicroservice<IPositionClientEx>()
                ?.ArmedTime.Select(time => time.TotalSeconds)
                .Prepend(double.NaN) ?? Observable.Return(double.NaN);

        return CreateItem(currentMode, timeInAir);
    }

    public ITelemetryItem CreatePreview()
    {
        return CreateItem(
            Observable.Return("Unknown").Concat(Observable.Never<string>()),
            Observable.Return(125d).Concat(Observable.Never<double>())
        );
    }

    private CurrentFlightModeTelemetryItemViewModel CreateItem(
        Observable<string> currentMode,
        Observable<double> timeInAir
    )
    {
        var timeSpanUnit = unitService.Units[TimeSpanUnit.Id].AvailableUnits[
            TimeSpanHourMinuteSecondUnitItem.Id
        ];

        var modeData = currentMode.CombineLatest(
            timeInAir,
            (mode, time) => new FlightModeRttBoxData(mode, time, timeSpanUnit)
        );

        return new CurrentFlightModeTelemetryItemViewModel(
            Id,
            loggerFactory,
            modeData,
            DefaultStatusColor
        );
    }
}
