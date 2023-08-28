using System.Windows.Input;
using Avalonia.Media;
using Material.Icons;
using ReactiveUI;

namespace Asv.Drones.Gui.Core;

public class HierarchicalStoreEntryTagViewModel:ReactiveObject
{
    private const double CellWidth = 50;
    private string _name;
    public MaterialIconKind Icon { get; set; } = MaterialIconKind.Tag;
    public IBrush Color { get; set; }

    public string Name
    {
        get => _name;
        set
        {
            _name = value;
            Width = (int)((_name.Length * 7 + 18) / CellWidth + 1) * CellWidth;
        }
    }
    public double Width { get; private set; }
    
    public ICommand? Remove { get; set; }
}