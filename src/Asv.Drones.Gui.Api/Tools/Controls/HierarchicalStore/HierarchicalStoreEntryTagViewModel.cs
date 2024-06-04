using System.Windows.Input;
using Avalonia.Media;
using Material.Icons;
using ReactiveUI;

namespace Asv.Drones.Gui.Api;

public class HierarchicalStoreEntryTagViewModel : ReactiveObject
{
    public MaterialIconKind Icon { get; set; } = MaterialIconKind.Tag;
    public IBrush Color { get; set; }
    public string Name { get; set; }
    public ICommand? Remove { get; set; }
}