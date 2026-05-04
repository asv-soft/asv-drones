using System;
using Asv.Avalonia;
using Asv.Common;
using Asv.Drones.Api;
using Asv.IO;
using Asv.Mavlink;
using Microsoft.Extensions.Logging;
using R3;

namespace Asv.Drones;

public sealed class HomeAzimuthTelemetryItemFactory(
    IUnitService unitService,
    ILoggerFactory loggerFactory
) : ITelemetryItemFactory
{
    public const string Id = "home-azimuth-uav";
    private const AsvColorKind DefaultStatusColor = AsvColorKind.Info5;

    public string ItemId => Id;
    public string DisplayName => RS.HomeAzimuthUavIndicatorViewModel_HomeAzimuth;

    public bool CanCreate(in IClientDevice device) =>
        device.GetMicroservice<IPositionClientEx>() is not null;

    public ITelemetryItem Create(in IClientDevice device)
    {
        ArgumentNullException.ThrowIfNull(device);

        var positionClientEx =
            device.GetMicroservice<IPositionClientEx>()
            ?? throw new ArgumentException(
                $"Unable to load {nameof(IPositionClientEx)} from {device.Id}"
            );

        var homeAzimuth = new ReactiveProperty<double>(double.NaN);

        var indicator = new HomeAzimuthUavIndicatorViewModel(
            Id,
            loggerFactory,
            unitService,
            homeAzimuth,
            DefaultStatusColor
        );

        return new TelemetryItemViewModel(
            Id,
            indicator,
            loggerFactory,
            homeAzimuth,
            positionClientEx
                .Current.ObserveOnUIThreadDispatcher()
                .Where(_ => positionClientEx.Home.CurrentValue.HasValue)
                .ThrottleLast(TimeSpan.FromMilliseconds(200))
                .Select(p => p.Azimuth(positionClientEx.Home.CurrentValue ?? GeoPoint.NaN))
                .Subscribe(x => homeAzimuth.Value = x)
        );
    }

    public ITelemetryItem CreatePreview()
    {
        var homeAzimuth = new ReactiveProperty<double>(30);

        var indicator = new HomeAzimuthUavIndicatorViewModel(
            Id,
            loggerFactory,
            unitService,
            homeAzimuth,
            DefaultStatusColor
        );

        return new TelemetryItemViewModel(Id, indicator, loggerFactory, homeAzimuth);
    }
}
