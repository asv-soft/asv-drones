using Asv.Drones.Gui.Core;
using Asv.Mavlink;
using ReactiveUI.Fody.Helpers;

namespace Asv.Drones.Gui.Uav;

public abstract class UavRttItem : ViewModelBase, IUavRttItem
{
    protected IVehicle Vehicle { get; }

    public static Uri GenerateRtt(IVehicle vehicle, string name) =>
        new(FlightUavViewModel.GenerateUri(vehicle), $"rtt/{name}");

    public UavRttItem():base(new Uri($"fordesigntime:{Guid.NewGuid()}"))
    {
        
    }
    
    protected UavRttItem(IVehicle vehicle,Uri id) : base(id)
    {
        IsVisible = true;
        Vehicle = vehicle;
    }
    
    [Reactive]
    public int Order { get; set; }
    [Reactive]
    public bool IsVisible { get; set; }
}