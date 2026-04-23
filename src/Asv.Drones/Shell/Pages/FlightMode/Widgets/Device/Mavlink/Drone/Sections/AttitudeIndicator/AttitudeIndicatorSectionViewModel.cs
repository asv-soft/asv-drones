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
    public const string WidgetId = "alt-indicator-widget-section";

    public AttitudeIndicatorSectionViewModel(ILoggerFactory loggerFactory)
        : base(WidgetId, loggerFactory) { }

    public IReadOnlyBindableReactiveProperty<double> Roll { get; private set; }
    public IReadOnlyBindableReactiveProperty<double> Pitch { get; private set; }
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
    }

    public override IEnumerable<IRoutable> GetChildren()
    {
        return [];
    }
}
