using Asv.Drones.Gui.Core;
using Asv.Mavlink;
using ReactiveUI.Fody.Helpers;

namespace Asv.Drones.Gui.Gbs;

public abstract class GbsRttItem : ViewModelBase, IGbsRttItem
{
    protected IGbsClientDevice BaseStation { get; }

    public static Uri GenerateUri(IGbsClientDevice gbs, string name) =>
        new(FlightGbsViewModel.GenerateUri(gbs), $"rtt/{name}");

    public GbsRttItem():base(new Uri($"fordesigntime:{Guid.NewGuid()}"))
    {
        
    }
    
    protected GbsRttItem(IGbsClientDevice baseStation,Uri id) : base(id)
    {
        IsVisible = true;
        BaseStation = baseStation;
    }
    
    [Reactive]
    public int Order { get; set; }
    [Reactive]
    public bool IsVisible { get; set; }
}