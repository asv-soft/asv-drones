using Asv.Drones.Gui.Core;

namespace Asv.Drones.Gui.Gbs;

public interface IGbsRttItem:IViewModel
{
    int Order { get; }
    bool IsVisible { get; set; }
}