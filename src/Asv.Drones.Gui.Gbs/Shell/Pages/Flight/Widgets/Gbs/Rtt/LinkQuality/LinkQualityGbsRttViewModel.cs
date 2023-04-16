using Asv.Common;
using Asv.Mavlink;
using ReactiveUI.Fody.Helpers;

namespace Asv.Drones.Gui.Gbs;

public class LinkQualityGbsRttViewModel : GbsRttItem
{
    public LinkQualityGbsRttViewModel()
    {
        LinkQuality = 0.3;
        LinkQualityString = "30%";
    }

    public LinkQualityGbsRttViewModel(IGbsClientDevice baseStation) : base(baseStation, GenerateUri(baseStation,"linkquality"))
    {
        Order = 1;
        
        BaseStation.Heartbeat.LinkQuality
            .Subscribe(_ => LinkQuality = _)
            .DisposeItWith(Disposable);

        BaseStation.Heartbeat.LinkQuality
            .Subscribe(_ => LinkQualityString = _.ToString("P0"))
            .DisposeItWith(Disposable);
    }
    
    [Reactive]
    public double LinkQuality { get; set; }

    [Reactive]
    public string LinkQualityString { get; set; } = RS.GbsRttItem_ValueNotAvailable;
}