using System;
using Asv.Avalonia;
using Asv.Drones.Api;
using Asv.IO;
using Asv.Mavlink;
using Microsoft.Extensions.Logging;
using R3;

namespace Asv.Drones;

public sealed class CurrentFlightModeTelemetryItemFactory(ILoggerFactory loggerFactory)
    : ITelemetryItemFactory
{
    public const string Id = "current-flight-mode-uav";
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

        return new CurrentFlightModeUavIndicatorViewModel(
            Id,
            loggerFactory,
            currentMode,
            DefaultStatusColor
        );
    }

    public ITelemetryItem CreatePreview()
    {
        var currentMode = Observable.Return("Unknown").Concat(Observable.Never<string>());

        return new CurrentFlightModeUavIndicatorViewModel(
            Id,
            loggerFactory,
            currentMode,
            DefaultStatusColor
        );
    }
}
