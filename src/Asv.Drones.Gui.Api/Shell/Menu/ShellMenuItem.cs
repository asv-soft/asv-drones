#nullable enable
using System.Collections.ObjectModel;
using Asv.Common;
using DynamicData.Binding;
using FluentAvalonia.UI.Controls;
using ReactiveUI.Fody.Helpers;

namespace Asv.Drones.Gui.Api
{
    public class ShellMenuItem : ViewModelBase, IShellMenuItem
    {
        private readonly ReadOnlyObservableCollection<IShellMenuItem>? _items;

        public ShellMenuItem(Uri id) : base(id)
        {
        }

        public ShellMenuItem(string id) : base(id)
        {
        }

        public InfoBadge InfoBadge { get; set; }
        public IShellMenuItem Parent { get; set; }

        [Reactive] public string Name { get; set; }
        [Reactive] public Uri NavigateTo { get; set; }
        [Reactive] public string Icon { get; init; }
        public ShellMenuPosition Position { get; init; }
        public ShellMenuItemType Type { get; init; }
        public int Order { get; init; }

        public ReadOnlyObservableCollection<IShellMenuItem>? Items
        {
            get => _items;
            init
            {
                _items = value;
                if (value == null) return;
                foreach (var item in value)
                {
                    item.Parent = this;
                }

                value.ObserveCollectionChanges().Subscribe(_ =>
                {
                    if (_.EventArgs.NewItems == null) return;
                    foreach (var newItem in _.EventArgs.NewItems)
                    {
                        (newItem as IShellMenuItem)!.Parent = this;
                    }
                }).DisposeItWith(Disposable);
            }
        }

        [Reactive] public bool IsSelected { get; set; }

        [Reactive] public bool IsVisible { get; set; } = true;
    }
}