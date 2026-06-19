using System;
using Asv.Avalonia;
using Asv.Common;
using Asv.Drones.Api;
using Asv.IO;
using Asv.Mavlink;
using Material.Icons;
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

    public bool CanCreate(in IClientDevice device) =>
        device.GetMicroservice<IHeartbeatClient>() is not null;

    public IRttBoxViewModel Create(in IClientDevice device)
    {
        ArgumentNullException.ThrowIfNull(device);

        var heartbeatClient = device.GetRequiredMicroservice<IHeartbeatClient>();
        var linkQuality = heartbeatClient
            .LinkQuality.Prepend(double.NaN)
            .CombineLatest(
                unitService.Units[ProgressUnit.Id].CurrentUnitItem,
                heartbeatClient.Link.State,
                (value, _, state) => new LinkQualityRttBoxData(value * 100d, state)
            );

        return CreateItem(linkQuality);
    }

    public IRttBoxViewModel CreatePreview()
    {
        var linkQuality = Observable
            .Return(1d)
            .Concat(Observable.Never<double>())
            .CombineLatest(
                unitService.Units[ProgressUnit.Id].CurrentUnitItem,
                Observable.Return(LinkState.Connected).Concat(Observable.Never<LinkState>()),
                (value, _, state) => new LinkQualityRttBoxData(value, state)
            );

        return CreateItem(linkQuality);
    }

    private IRttBoxViewModel CreateItem(Observable<LinkQualityRttBoxData> linkQualityData)
    {
        return new LinkQualityRttBoxViewModel(loggerFactory, unitService, linkQualityData)
        {
            Header = RS.UavRttItem_Link,
            Icon = MaterialIconKind.Wifi,
            FormatString = "F2",
        };
    }

    private sealed class LinkQualityRttBoxViewModel : SplitDigitRttBoxViewModel
    {
        public LinkQualityRttBoxViewModel(
            ILoggerFactory loggerFactory,
            IUnitService unitService,
            Observable<LinkQualityRttBoxData> linkQualityData
        )
            : base(
                LinkQualityTelemetryItemFactory.Id,
                loggerFactory,
                unitService,
                ProgressUnit.Id,
                linkQualityData.Select(data => data.LinkQuality),
                null
            )
        {
            Status = DefaultStatusColor;
            linkQualityData
                .ObserveOnUIThreadDispatcher()
                .Select(data => GetLinkStatus(data.LinkState))
                .DistinctUntilChanged()
                .Subscribe(status => Status = status)
                .DisposeItWith(Disposable);
        }
    }

    private static AsvColorKind GetLinkStatus(LinkState state)
    {
        return state switch
        {
            LinkState.Connected => AsvColorKind.Success,
            LinkState.Downgrade => AsvColorKind.Warning,
            LinkState.Disconnected => AsvColorKind.Error,
            _ => AsvColorKind.None,
        };
    }
}

#pragma warning disable SA1313
public record LinkQualityRttBoxData(double LinkQuality, LinkState LinkState);
#pragma warning restore SA1313
