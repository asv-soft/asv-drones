using System.Reactive.Linq;
using Asv.Common;
using Asv.Mavlink;
using Avalonia.Controls.Mixins;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Asv.Drones.Gui.Uav;

public class LinkQualityUavRttViewModel : UavRttItem
{
    public LinkQualityUavRttViewModel()
    {
        LinkQuality = 0.3;
        LinkQualityString = "0.3";
    }

    public LinkQualityUavRttViewModel(IVehicleClient vehicle) : base(vehicle, GenerateRtt(vehicle,"linkquality"))
    {
        Order = 8;
        
        Vehicle.Heartbeat.LinkQuality
            .DistinctUntilChanged()
            .Sample(TimeSpan.FromMilliseconds(500))
            .ObserveOn(RxApp.MainThreadScheduler)
            .Subscribe(_ => LinkQuality = _)
            .DisposeWith(Disposable);
        
        Vehicle.Heartbeat.LinkQuality
            .Subscribe(_ => LinkQualityString = _.ToString("P0"))
            .DisposeItWith(Disposable);
    }
    
    [Reactive]
    public double LinkQuality { get; set; }
    
    [Reactive]
    public string LinkQualityString { get; set; } = RS.UavRttItem_ValueNotAvailable;
}