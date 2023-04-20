using Asv.Drones.Gui.Core;
using Asv.Mavlink;
using ReactiveUI.Fody.Helpers;

namespace Asv.Drones.Gui.Sdr;

public abstract class SdrRttItem : ViewModelBase, ISdrRttItem
{
    protected ISdrClientDevice Payload { get; }

    public static Uri GenerateUri(ISdrClientDevice gbs, string name) =>
        new(FlightSdrViewModel.GenerateUri(gbs), $"rtt/{name}");

    public SdrRttItem():base(new Uri($"fordesigntime:{Guid.NewGuid()}"))
    {
        Payload = null!;
    }
    
    protected SdrRttItem(ISdrClientDevice payload,Uri id) : base(id)
    {
        IsVisible = true;
        Payload = payload;
    }
    
    [Reactive]
    public int Order { get; set; }
    [Reactive]
    public bool IsVisible { get; set; }
}