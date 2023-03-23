using Asv.Drones.Gui.Core;
using Asv.Mavlink;
using ReactiveUI.Fody.Helpers;

namespace Asv.Drones.Gui.Gbs;

public abstract class GbsRttItem : ViewModelBase, IGbsRttItem
{
    protected IVehicle Vehicle { get; }

    public static Uri GenerateRtt(IVehicle vehicle, string name) =>
        new(FlightGbsViewModel.GenerateUri(vehicle), $"rtt/{name}");

    public GbsRttItem():base(new Uri($"fordesigntime:{Guid.NewGuid()}"))
    {
        
    }
    
    protected GbsRttItem(IVehicle vehicle,Uri id) : base(id)
    {
        IsVisible = true;
        Vehicle = vehicle;
    }
    
    [Reactive]
    public int Order { get; set; }
    [Reactive]
    public bool IsVisible { get; set; }
}