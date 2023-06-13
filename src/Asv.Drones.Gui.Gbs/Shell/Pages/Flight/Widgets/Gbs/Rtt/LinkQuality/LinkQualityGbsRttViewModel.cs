using System.Reactive.Linq;
using Asv.Common;
using Asv.Mavlink;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Asv.Drones.Gui.Gbs;

public class LinkQualityGbsRttViewModel : GbsRttItem
{
    public LinkQualityGbsRttViewModel()
    {
        LinkQuality = 0.3;
        LinkQualityString = "30%";
    }

    public LinkQualityGbsRttViewModel(IGbsClientDevice baseStation) : base(baseStation, GenerateUri(baseStation,"gbslinkquality"))
    {
        Order = 1;
        
        BaseStation.Heartbeat.LinkQuality
            .ObserveOn(RxApp.MainThreadScheduler)
            .Subscribe(_ => LinkQuality = _)
            .DisposeItWith(Disposable);

        BaseStation.Heartbeat.LinkQuality
            .ObserveOn(RxApp.MainThreadScheduler)
            .Subscribe(_ => LinkQualityString = _.ToString("P0"))
            .DisposeItWith(Disposable);

        IsMinimizedVisible = true;
    }
    
    [Reactive]
    public double LinkQuality { get; set; }

    [Reactive]
    public string LinkQualityString { get; set; } = RS.GbsRttItem_ValueNotAvailable;
}