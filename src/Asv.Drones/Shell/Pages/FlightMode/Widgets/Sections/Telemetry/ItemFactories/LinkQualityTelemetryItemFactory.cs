using System;
using Asv.Avalonia;
using Asv.Common;
using Asv.Drones.Api;
using Asv.IO;
using Asv.Mavlink;
using Material.Icons;
using Microsoft.Extensions.Logging;
using R3;
using TelemetryHelper = Asv.Drones.Api.TelemetryHelper;

namespace Asv.Drones;

public sealed class LinkQualityTelemetryItemFactory(IUnitService unitService)
    : ITelemetryItemFactory
{
    public const string Id = "link-quality";

    public string ItemId => Id;

    public bool CanCreate(in IClientDevice device) =>
        device.GetMicroservice<IHeartbeatClient>() is not null;

    public ITileViewModel Create(in IClientDevice device)
    {
        ArgumentNullException.ThrowIfNull(device);

        var heartbeatClient = device.GetRequiredMicroservice<IHeartbeatClient>();
        var linkQuality = heartbeatClient
            .LinkQuality.Prepend(double.NaN)
            .CombineLatest(
                unitService.Units[ProgressUnit.Id].CurrentUnitItem,
                heartbeatClient.Link.State,
                (value, unit, state) => new LinkQualityTelemetryData(value * 100d, unit, state)
            )
            .ObserveOnUIThreadDispatcher()
            .ThrottleLast(TimeSpan.FromMilliseconds(200));

        return new TelemetryViewModel<LinkQualityTelemetryData>(Id, linkQuality, Update)
        {
            Density = TileDensity.Inline,
            Header = RS.LinkTelemetry_Header,
            ShortHeader = RS.LinkTelemetry_Header,
            Icon = MaterialIconKind.Wifi,
        };

        static void Update(
            TelemetryViewModel<LinkQualityTelemetryData> t,
            LinkQualityTelemetryData changes
        )
        {
            t.StatusIcon = GetStatusIcon(changes.LinkState);
            t.Text = changes.ProgressUnit.PrintFromSi(changes.LinkQuality, "F2");
            t.Units = changes.ProgressUnit.Symbol;
            t.Progress = changes.LinkQuality;

            var status = GetStatusColor(changes.LinkState);
            t.ProgressColor = status;
            t.StatusColor = status;
            t.StatusIconColor = status;
        }
    }

    private static AsvColorKind GetStatusColor(LinkState state)
    {
        return state switch
        {
            LinkState.Downgrade => AsvColorKind.Warning | AsvColorKind.Blink,
            LinkState.Disconnected => AsvColorKind.Error,
            _ => AsvColorKind.Success,
        };
    }

    private static MaterialIconKind GetStatusIcon(LinkState percent)
    {
        return percent switch
        {
            LinkState.Downgrade => MaterialIconKind.AlertCircle,
            LinkState.Disconnected => MaterialIconKind.CloseCircle,
            _ => MaterialIconKind.CheckCircle,
        };
    }
}

#pragma warning disable SA1313
public record LinkQualityTelemetryData(
    double LinkQuality,
    IUnitItem ProgressUnit,
    LinkState LinkState
);
#pragma warning restore SA1313
