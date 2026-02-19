using System;
using System.Collections.Generic;
using Asv.Avalonia;
using Asv.Common;
using Asv.Drones.Api;
using Asv.IO;
using Asv.Mavlink;
using Microsoft.Extensions.Logging;
using R3;

namespace Asv.Drones;

public class AttitudeIndicatorSectionViewModel
    : RoutableViewModel,
        IFlightWidgetSection<MavlinkClientDevice>
{
    public const string SectionId = "alt-indicator-widget-section";

    public AttitudeIndicatorSectionViewModel(ILoggerFactory loggerFactory)
        : base(SectionId, loggerFactory) { }

    public IReadOnlyBindableReactiveProperty<double> Roll { get; private set; }
    public IReadOnlyBindableReactiveProperty<double> Pitch { get; private set; }
    public IReadOnlyBindableReactiveProperty<double> Heading { get; private set; }
    public IReadOnlyBindableReactiveProperty<double> HomeAzimuth { get; private set; }

    public int Order => 0;

    public void InitWith(MavlinkClientDevice device)
    {
        ArgumentNullException.ThrowIfNull(device);

        var positionClientEx =
            device.GetMicroservice<IPositionClientEx>()
            ?? throw new ArgumentException(
                $"Unable to load {nameof(PositionClientEx)} from {device.Id}"
            );

        Roll = positionClientEx.Roll.ToReadOnlyBindableReactiveProperty().DisposeItWith(Disposable);
        Pitch = positionClientEx
            .Pitch.ToReadOnlyBindableReactiveProperty()
            .DisposeItWith(Disposable);

        Heading = positionClientEx
            .Yaw.ObserveOnUIThreadDispatcher()
            .ThrottleLast(TimeSpan.FromMilliseconds(200))
            .Select(Math.Truncate)
            .ToReadOnlyBindableReactiveProperty()
            .DisposeItWith(Disposable);

        HomeAzimuth = positionClientEx
            .Current.ObserveOnUIThreadDispatcher()
            .Where(_ => positionClientEx.Home.CurrentValue.HasValue)
            .ThrottleLast(TimeSpan.FromMilliseconds(200))
            .Select(p => p.Azimuth(positionClientEx.Home.CurrentValue ?? GeoPoint.NaN))
            .ToReadOnlyBindableReactiveProperty()
            .DisposeItWith(Disposable);
    }

    public override IEnumerable<IRoutable> GetChildren()
    {
        return [];
    }
}
