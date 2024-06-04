using System;
using System.Reactive.Linq;
using Asv.Common;
using Asv.Drones.Gui.Api;
using Asv.Mavlink;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Asv.Drones.Gui;

public class LinkQualityUavRttViewModel : UavRttItem
{
    public LinkQualityUavRttViewModel()
    {
        DesignTime.ThrowIfNotDesignMode();
        LinkQuality = 0.3;
        LinkQualityString = "0.3";
    }

    public LinkQualityUavRttViewModel(IVehicleClient vehicle) : base(vehicle, "linkquality")
    {
        Order = 8;

        Vehicle.Heartbeat.LinkQuality
            .DistinctUntilChanged()
            .Sample(TimeSpan.FromMilliseconds(500))
            .ObserveOn(RxApp.MainThreadScheduler)
            .Subscribe(_ => LinkQuality = _)
            .DisposeItWith(Disposable);

        Vehicle.Heartbeat.LinkQuality
            .ObserveOn(RxApp.MainThreadScheduler)
            .Subscribe(_ => LinkQualityString = _.ToString("P0"))
            .DisposeItWith(Disposable);

        IsMinimizedVisible = true;
    }

    [Reactive] public double LinkQuality { get; set; }

    [Reactive] public string LinkQualityString { get; set; } = RS.UavRttItem_ValueNotAvailable;
}