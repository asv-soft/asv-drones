using System;
using Asv.Avalonia;
using Asv.Common;
using Asv.Drones.Api;
using Material.Icons;
using Microsoft.Extensions.Logging;
using R3;

namespace Asv.Drones;

public class LinkQualityTelemetryItemViewModel : SplitDigitRttBoxViewModel, ITelemetryItem
{
    public LinkQualityTelemetryItemViewModel()
        : this(
            nameof(LinkQualityTelemetryItemViewModel),
            DesignTime.LoggerFactory,
            DeviceTelemetryDesignPreview.UnitService,
            Observable.Return(0.87d).Concat(Observable.Never<double>()),
            Observable.Return(LinkState.Connected).Concat(Observable.Never<LinkState>()),
            DeviceTelemetryDesignPreview.DefaultStatusColor
        )
    {
        DesignTime.ThrowIfNotDesignMode();
    }

    public LinkQualityTelemetryItemViewModel(
        string id,
        ILoggerFactory loggerFactory,
        IUnitService unitService,
        Observable<double> linkQuality,
        Observable<LinkState> linkState,
        AsvColorKind defaultStatusColor,
        TimeSpan? networkErrorTimeout = null
    )
        : base(id, loggerFactory, unitService, ProgressUnit.Id, linkQuality, networkErrorTimeout)
    {
        ItemId = id;
        Header = RS.UavRttItem_Link;
        Icon = MaterialIconKind.Wifi;
        Status = defaultStatusColor;
        FormatString = "F2";

        linkState
            .Skip(1)
            .ObserveOnUIThreadDispatcher()
            .Subscribe(ChangeLinkStatus)
            .DisposeItWith(Disposable);
    }

    public string ItemId { get; }

    private void ChangeLinkStatus(LinkState state)
    {
        Status = state switch
        {
            LinkState.Connected => AsvColorKind.Success,
            LinkState.Downgrade => AsvColorKind.Warning,
            LinkState.Disconnected => AsvColorKind.Error,
            _ => AsvColorKind.None,
        };
    }
}
