using Asv.Drones.Gui.Api;
using Asv.Mavlink;
using ReactiveUI.Fody.Helpers;

namespace Asv.Drones.Gui;

public abstract class UavRttItem : ViewModelBase, IUavRttItem
{
    protected IVehicleClient Vehicle { get; }

    public UavRttItem() : base(WellKnownUri.UndefinedUri)
    {
        DesignTime.ThrowIfNotDesignMode();
    }

    protected UavRttItem(IVehicleClient vehicle, string name) : base(
        $"{WellKnownUri.ShellPageMapFlightWidgetUav}/{vehicle.FullId}/rtt/{name}")
    {
        IsVisible = true;
        Vehicle = vehicle;
    }

    [Reactive] public int Order { get; set; }
    [Reactive] public bool IsVisible { get; set; }
    [Reactive] public bool IsMinimizedVisible { get; set; } = false;
}