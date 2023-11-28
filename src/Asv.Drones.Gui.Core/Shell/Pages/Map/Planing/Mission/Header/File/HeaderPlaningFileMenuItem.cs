using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Reactive.Linq;
using Asv.Common;
using DynamicData;
using Material.Icons;

namespace Asv.Drones.Gui.Core;

public class HeaderPlaningFileMenuItem : HeaderMenuItem
{
    public const string UriString = HeaderMenuItem.UriString + "/planing-file";
    public static readonly Uri Uri = new(UriString);
    private readonly ReadOnlyObservableCollection<IHeaderMenuItem> _items;

    [ImportingConstructor]
    public HeaderPlaningFileMenuItem() : base(Uri)
    {
        Header = RS.HeaderPlaningFileMenuItem_Title;
        Order = 0;
    }

    public override ReadOnlyObservableCollection<IHeaderMenuItem>? Items { get; set; }
}