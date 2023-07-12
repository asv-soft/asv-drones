using Avalonia.Controls;

namespace Asv.Drones.Gui.Core;


public interface IMapAction:IViewModel
{
    int Order { get; }
    IMapAction Init(IMap context);
}