using System;
using System.Collections.Generic;
using Asv.Avalonia;
using Asv.Common;
using Asv.Drones.Api;
using Asv.IO;
using Asv.Mavlink;
using R3;

namespace Asv.Drones;

public class AttitudeIndicatorSectionViewModel : ViewModel, IFlightWidgetSection
{
    public const string SectionId = "alt-indicator-widget-section";

    public AttitudeIndicatorSectionViewModel()
        : base(SectionId)
    {
        DesignTime.ThrowIfNotDesignMode();
    }

    public double Roll
    {
        get;
        private set => SetField(ref field, value);
    }

    public double Pitch
    {
        get;
        private set => SetField(ref field, value);
    }

    public double Heading
    {
        get;
        private set => SetField(ref field, value);
    }

    public double HomeAzimuth
    {
        get;
        private set => SetField(ref field, value);
    }

    public int Order => 0;

    public AttitudeIndicatorSectionViewModel(MavlinkClientDevice device)
        : base(SectionId)
    {
        var positionClientEx = device.GetRequiredMicroservice<IPositionClientEx>();

        positionClientEx
            .Roll.ObserveOnUIThreadDispatcher()
            .Subscribe(x => Roll = x)
            .DisposeItWith(Disposable);
        positionClientEx
            .Pitch.ObserveOnUIThreadDispatcher()
            .Subscribe(x => Pitch = x)
            .DisposeItWith(Disposable);

        positionClientEx
            .Yaw.ThrottleLast(TimeSpan.FromMilliseconds(200))
            .Select(Math.Truncate)
            .ObserveOnUIThreadDispatcher()
            .Subscribe(x => Heading = x)
            .DisposeItWith(Disposable);

        positionClientEx
            .Current.Where(_ => positionClientEx.Home.CurrentValue.HasValue)
            .ThrottleLast(TimeSpan.FromMilliseconds(200))
            .Select(p => p.Azimuth(positionClientEx.Home.CurrentValue ?? GeoPoint.NaN))
            .ObserveOnUIThreadDispatcher()
            .Subscribe(x => HomeAzimuth = x)
            .DisposeItWith(Disposable);
    }

    public override IEnumerable<IViewModel> GetChildren()
    {
        return [];
    }
}
