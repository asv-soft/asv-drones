using Avalonia.Controls;

namespace Asv.Drones.Gui.Core;


public interface IMapAction:IViewModel
{
    Dock Dock { get; }
    int Order { get; }
    IMapAction Init(IMap context);
}