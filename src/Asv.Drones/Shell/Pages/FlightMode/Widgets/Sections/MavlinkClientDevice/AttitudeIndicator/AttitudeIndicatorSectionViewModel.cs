using System;
using System.Collections.Generic;
using Asv.Avalonia;
using Asv.Common;
using Asv.Drones.Api;
using Asv.IO;
using Asv.Mavlink;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using R3;

namespace Asv.Drones;

public class AttitudeIndicatorSectionViewModel
    : RoutableViewModel,
        IFlightWidgetSection<MavlinkClientDevice>
{
    public const string SectionId = "alt-indicator-widget-section";

    private readonly BindableReactiveProperty<double> _roll;
    private readonly BindableReactiveProperty<double> _pitch;
    private readonly BindableReactiveProperty<double> _heading;
    private readonly BindableReactiveProperty<double> _homeAzimuth;

    public AttitudeIndicatorSectionViewModel()
        : this(NullLoggerFactory.Instance) { }

    public AttitudeIndicatorSectionViewModel(ILoggerFactory loggerFactory)
        : base(SectionId, loggerFactory)
    {
        _roll = new BindableReactiveProperty<double>().DisposeItWith(Disposable);
        _pitch = new BindableReactiveProperty<double>().DisposeItWith(Disposable);
        _heading = new BindableReactiveProperty<double>().DisposeItWith(Disposable);
        _homeAzimuth = new BindableReactiveProperty<double>().DisposeItWith(Disposable);

        Roll = _roll;
        Pitch = _pitch;
        Heading = _heading;
        HomeAzimuth = _homeAzimuth;
    }

    public IReadOnlyBindableReactiveProperty<double> Roll { get; }
    public IReadOnlyBindableReactiveProperty<double> Pitch { get; }
    public IReadOnlyBindableReactiveProperty<double> Heading { get; }
    public IReadOnlyBindableReactiveProperty<double> HomeAzimuth { get; }

    public int Order => 0;

    public void InitWith(MavlinkClientDevice device)
    {
        ArgumentNullException.ThrowIfNull(device);

        var positionClientEx =
            device.GetMicroservice<IPositionClientEx>()
            ?? throw new ArgumentException(
                $"Unable to load {nameof(PositionClientEx)} from {device.Id}"
            );

        positionClientEx.Roll.Subscribe(x => _roll.Value = x).DisposeItWith(Disposable);
        positionClientEx.Pitch.Subscribe(x => _pitch.Value = x).DisposeItWith(Disposable);

        positionClientEx
            .Yaw.ObserveOnUIThreadDispatcher()
            .ThrottleLast(TimeSpan.FromMilliseconds(200))
            .Select(Math.Truncate)
            .Subscribe(x => _heading.Value = x)
            .DisposeItWith(Disposable);

        positionClientEx
            .Current.ObserveOnUIThreadDispatcher()
            .Where(_ => positionClientEx.Home.CurrentValue.HasValue)
            .ThrottleLast(TimeSpan.FromMilliseconds(200))
            .Select(p => p.Azimuth(positionClientEx.Home.CurrentValue ?? GeoPoint.NaN))
            .Subscribe(x => _homeAzimuth.Value = x)
            .DisposeItWith(Disposable);
    }

    public override IEnumerable<IRoutable> GetChildren()
    {
        return [];
    }
}
