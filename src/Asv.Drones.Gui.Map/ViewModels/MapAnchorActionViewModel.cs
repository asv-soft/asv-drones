using System.Windows.Input;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Asv.Avalonia.Map
{
    public class MapAnchorActionViewModel: ReactiveObject
    {
        [Reactive]
        public object? Icon { get; set; }
        [Reactive]
        public string Title { get; set; }
        [Reactive]
        public ICommand Command { get; set; }
        [Reactive]
        public object CommandParameter { get; set; }
        
    }
}