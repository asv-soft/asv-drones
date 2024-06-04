using System;
using System.Reactive.Linq;
using Asv.Common;
using Asv.Drones.Gui.Api;
using Asv.Mavlink;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Asv.Drones.Gui;

public class CpuLoadUavRttViewModel : UavRttItem
{
    public CpuLoadUavRttViewModel()
    {
        DesignTime.ThrowIfNotDesignMode();
        CpuLoad = 0.1;
    }

    public CpuLoadUavRttViewModel(IVehicleClient vehicle) : base(vehicle, "cpuload")
    {
        Order = 7;
        Vehicle.Rtt.CpuLoad
            .DistinctUntilChanged()
            .Sample(TimeSpan.FromMilliseconds(500), RxApp.MainThreadScheduler)
            .Subscribe(_ => CpuLoad = _)
            .DisposeItWith(Disposable);
    }

    [Reactive] public double CpuLoad { get; set; }
}