using System.Windows.Input;
using Material.Icons;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Asv.Avalonia.Map
{
   

    public class MapAnchorActionViewModel: ReactiveObject
    {
        public int Order { get; }
        [Reactive]
        public MaterialIconKind? Icon { get; set; }
        [Reactive]
        public string Title { get; set; }
        [Reactive]
        public ICommand Command { get; set; }
        [Reactive]
        public object CommandParameter { get; set; }
        
    }
}