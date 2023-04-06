using Asv.Common;
using ReactiveUI.Fody.Helpers;

namespace Asv.Drones.Gui.Gbs;

public class LinkQualityGbsRttViewModel : GbsRttItem
{
    public LinkQualityGbsRttViewModel()
    {
        LinkQuality = 0.3;
        LinkQualityString = "30%";
    }

    public LinkQualityGbsRttViewModel(IGbsDevice gbs) : base(gbs, GenerateUri(gbs,"linkquality"))
    {
        Order = 1;
        
        Gbs.MavlinkClient.Heartbeat.LinkQuality
            .Subscribe(_ => LinkQuality = _)
            .DisposeItWith(Disposable);

        Gbs.MavlinkClient.Heartbeat.LinkQuality
            .Subscribe(_ => LinkQualityString = _.ToString("P0"))
            .DisposeItWith(Disposable);
    }
    
    [Reactive]
    public double LinkQuality { get; set; }

    [Reactive]
    public string LinkQualityString { get; set; } = RS.GbsRttItem_ValueNotAvailable;
}