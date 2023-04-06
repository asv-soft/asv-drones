using Asv.Common;
using ReactiveUI.Fody.Helpers;

namespace Asv.Drones.Gui.Gbs;

public class VisibleSatellitesGbsRttViewModel : GbsRttItem
{
    public VisibleSatellitesGbsRttViewModel()
    {
        
    }

    public VisibleSatellitesGbsRttViewModel(IGbsDevice gbs) : base(gbs, GenerateUri(gbs,"visiblesatellites"))
    {
        Order = 1;
        
        Gbs.DeviceClient.AllSatellites
            .Subscribe(_ => VisibleSatellites = _.ToString())
            .DisposeItWith(Disposable);
    }
    
    [Reactive]
    public string VisibleSatellites { get; set; } = RS.GbsRttItem_ValueNotAvailable;
}