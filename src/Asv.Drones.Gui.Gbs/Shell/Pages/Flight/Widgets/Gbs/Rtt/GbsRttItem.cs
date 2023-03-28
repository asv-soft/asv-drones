using Asv.Drones.Gui.Core;
using Asv.Mavlink;
using ReactiveUI.Fody.Helpers;

namespace Asv.Drones.Gui.Gbs;

public abstract class GbsRttItem : ViewModelBase, IGbsRttItem
{
    protected IGbsDevice Gbs { get; }

    public static Uri GenerateUri(IGbsDevice gbs, string name) =>
        new(FlightGbsViewModel.GenerateUri(gbs), $"rtt/{name}");

    public GbsRttItem():base(new Uri($"fordesigntime:{Guid.NewGuid()}"))
    {
        
    }
    
    protected GbsRttItem(IGbsDevice gbs,Uri id) : base(id)
    {
        IsVisible = true;
        Gbs = gbs;
    }
    
    [Reactive]
    public int Order { get; set; }
    [Reactive]
    public bool IsVisible { get; set; }
}