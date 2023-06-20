using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Reactive.Linq;
using Asv.Common;
using DynamicData;
using DynamicData.Binding;
using Material.Icons;
using ReactiveUI;

namespace Asv.Drones.Gui.Core
{
    [Export(HeaderMenuItem.UriString, typeof(IHeaderMenuItem))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class HeaderFileMenu:HeaderMenuItem
    {
        public const string UriString = HeaderMenuItem.UriString + "/file";
        public static readonly Uri Uri = new(UriString);
        private readonly ReadOnlyObservableCollection<IHeaderMenuItem> _items;

        [ImportingConstructor]
        public HeaderFileMenu([ImportMany(UriString)]IEnumerable<IViewModelProvider<IHeaderMenuItem>> fileItemProviders) : base(Uri)
        {
            fileItemProviders
                .Select(_ => _.Items)
                .Merge()
                .SortBy(_ => _.Order)
                .Bind(out _items)
                .Subscribe()
                .DisposeItWith(Disposable);
            
            Header = RS.HeaderFileMenu_Header;
            Icon = MaterialIconKind.File;
            Order = short.MinValue;
        }

        public override ReadOnlyObservableCollection<IHeaderMenuItem>? Items => _items;
    }
}