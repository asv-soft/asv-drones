using System.Reactive.Linq;
using Asv.Mavlink;
using Avalonia.Controls.Mixins;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Asv.Drones.Gui.Uav;

public class CpuLoadUavRttViewModel : UavRttItem
{
    public CpuLoadUavRttViewModel()
    {
        CpuLoad = 0.1;
    }

    public CpuLoadUavRttViewModel(IVehicleClient vehicle) : base(vehicle, GenerateRtt(vehicle,"cpuload"))
    {
        Order = 7;
        Vehicle.Rtt.CpuLoad
            .DistinctUntilChanged()
            .Sample(TimeSpan.FromMilliseconds(500))
            .ObserveOn(RxApp.MainThreadScheduler)
            .Subscribe(_ => CpuLoad = _)
            .DisposeWith(Disposable);
    }
    
    [Reactive]
    public double CpuLoad { get; set; }
}