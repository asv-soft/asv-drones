using System;
using Asv.Avalonia;
using Asv.Common;
using Asv.Drones.Api;
using Asv.IO;
using Asv.Mavlink;
using Microsoft.Extensions.Logging;
using R3;

namespace Asv.Drones;

public sealed class LinkQualityTelemetryItemFactory(
    IUnitService unitService,
    ILoggerFactory loggerFactory
) : ITelemetryItemFactory
{
    public const string Id = "link-quality";
    private const AsvColorKind DefaultStatusColor = AsvColorKind.Info5;

    public string ItemId => Id;
    public string DisplayName => RS.UavRttItem_Link;

    public bool CanCreate(in IClientDevice device) =>
        device.GetMicroservice<IHeartbeatClient>() is not null;

    public ITelemetryItem Create(in IClientDevice device)
    {
        ArgumentNullException.ThrowIfNull(device);

        var heartbeatClient = device.GetRequiredMicroservice<IHeartbeatClient>();
        var linkQuality = heartbeatClient
            .LinkQuality.Prepend(double.NaN)
            .CombineLatest(
                unitService.Units[ProgressUnit.Id].CurrentUnitItem,
                (value, _) => value * 100d
            );

        return new LinkQualityTelemetryItemViewModel(
            Id,
            loggerFactory,
            unitService,
            linkQuality,
            heartbeatClient.Link.State,
            DefaultStatusColor
        );
    }

    public ITelemetryItem CreatePreview()
    {
        var linkQuality = Observable
            .Return(1d)
            .Concat(Observable.Never<double>())
            .CombineLatest(unitService.Units[ProgressUnit.Id].CurrentUnitItem, (value, _) => value);
        var linkState = Observable
            .Return(LinkState.Connected)
            .Concat(Observable.Never<LinkState>());

        return new LinkQualityTelemetryItemViewModel(
            Id,
            loggerFactory,
            unitService,
            linkQuality,
            linkState,
            DefaultStatusColor
        );
    }
}
