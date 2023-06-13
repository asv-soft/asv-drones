using Asv.Drones.Gui.Core;

namespace Asv.Drones.Gui.Uav;

public interface IUavRttItem:IViewModel
{
    int Order { get; }
    bool IsVisible { get; set; }
    bool IsMinimizedVisible { get; set; }
}