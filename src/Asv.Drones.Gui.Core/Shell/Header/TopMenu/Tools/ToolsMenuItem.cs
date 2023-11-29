using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Reactive.Linq;
using Asv.Common;
using Asv.Drones.Gui.Core;
using DynamicData;
using Material.Icons;

[Export(HeaderMenuItem.UriString, typeof(IHeaderMenuItem))]
[PartCreationPolicy(CreationPolicy.NonShared)]
public class HeaderToolsMenu : HeaderMenuItem
{
    public const string UriString = HeaderMenuItem.UriString + "/tools";
    public static readonly Uri Uri = new(UriString);
    private readonly ReadOnlyObservableCollection<IHeaderMenuItem> _items;

    [ImportingConstructor]
    public HeaderToolsMenu([ImportMany(UriString)]IEnumerable<IViewModelProvider<IHeaderMenuItem>> fileItemProviders) : base(Uri)
    {
        fileItemProviders
            .Select(_ => _.Items)
            .Merge()
            .SortBy(_ => _.Order)
            .Bind(out _items)
            .Subscribe()
            .DisposeItWith(Disposable);
        
        Header = RS.HeaderToolsMenu_Header;
        Icon = MaterialIconKind.Tools;
        Order = ushort.MaxValue;
    }

    public override ReadOnlyObservableCollection<IHeaderMenuItem>? Items => _items;
}