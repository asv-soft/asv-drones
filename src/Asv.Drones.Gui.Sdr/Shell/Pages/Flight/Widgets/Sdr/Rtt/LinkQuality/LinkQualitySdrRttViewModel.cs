using System.ComponentModel.Composition;
using System.Reactive.Linq;
using Asv.Common;
using Asv.Mavlink;
using Avalonia.Controls.Mixins;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Asv.Drones.Gui.Sdr;

public class LinkQualitySdrRttViewModel : SdrRttItem
{
    public LinkQualitySdrRttViewModel()
    {
        
    }
    
    [ImportingConstructor]
    public LinkQualitySdrRttViewModel(ISdrClientDevice device) 
        : base(device, SdrRttItem.GenerateUri(device,$"linkquality"))
    {
        device.Heartbeat.LinkQuality
            .DistinctUntilChanged()
            .Sample(TimeSpan.FromMilliseconds(500))
            .ObserveOn(RxApp.MainThreadScheduler)
            .Subscribe(_ => LinkQuality = _)
            .DisposeWith(Disposable);
        
        device.Heartbeat.LinkQuality
            .Subscribe(_ => LinkQualityString = _.ToString("P0"))
            .DisposeItWith(Disposable);
    }
    
    [Reactive]
    public double LinkQuality { get; set; }
    
    [Reactive]
    public string LinkQualityString { get; set; } = RS.SdrRttItem_ValueNotAvailable;
}