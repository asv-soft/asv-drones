using System.Collections.ObjectModel;
using System.Windows.Input;
using Avalonia.Input;
using Material.Icons;
using ReactiveUI.Fody.Helpers;

namespace Asv.Drones.Gui.Core
{
    public class HeaderMenuItem : ViewModelBase, IHeaderMenuItem
    {
        public const string? UriString = "asv:shell.header.menu";

        public HeaderMenuItem(Uri id) : base(id)
        {
            
        }
        public HeaderMenuItem(string id) : base(id)
        {
            
        }
        
        [Reactive]
        public int Order { get; set; }
        [Reactive]
        public MaterialIconKind Icon { get; set; }
        [Reactive]
        public string Header { get; set; }
        [Reactive]
        public ICommand Command { get; set; }
        [Reactive]
        public object? CommandParameter { get; set; }
        [Reactive]
        public bool IsVisible { get; set; } = true;
        [Reactive]
        public bool StaysOpenOnClick { get; set; }
        [Reactive] 
        public bool IsEnabled { get; set; } = true;
        public virtual ReadOnlyObservableCollection<IHeaderMenuItem>? Items { get; set; }
        [Reactive]
        public KeyGesture? HotKey { get; set; }
    }
}