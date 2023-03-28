using System.Diagnostics;
using System.Reactive.Linq;
using Asv.Common;
using Avalonia.Controls.Mixins;
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

    public LinkQualityGbsRttViewModel(IGbsDevice gbs) : base(gbs, GenerateUri(gbs,"linkquality"))
    {
        Order = 1;

        Gbs.Client.Heartbeat.LinkQuality
            .Subscribe(_ =>
            {
                Debug.WriteLine(_);
                LinkQuality = _;
            })
            .DisposeItWith(Disposable);

        Gbs.Client.Heartbeat.LinkQuality
            .Subscribe(_ => LinkQualityString = _.ToString("P0"))
            .DisposeItWith(Disposable);
    }
    
    [Reactive]
    public double LinkQuality { get; set; }

    [Reactive]
    public string LinkQualityString { get; set; } = RS.GbsRttItem_ValueNotAvailable;
}