using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using Asv.Common;
using Material.Icons;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Asv.Drones.Gui.Core;

[Export(HeaderMenuItem.UriString, typeof(IHeaderMenuItem))]
[PartCreationPolicy(CreationPolicy.Shared)]
public class HeaderAnchorsMenu : HeaderMenuItem
{
    public const string UriString = HeaderMenuItem.UriString + "/anchors";
    public static readonly Uri Uri = new(UriString);

    [ImportingConstructor]
    public HeaderAnchorsMenu() : base(Uri)
    {
        Header = RS.HeaderAnchorsMenu_Title;
        Icon = MaterialIconKind.Anchor;
        Order = short.MinValue;

        this.WhenAnyValue(_ => _.Items)
            .Subscribe(_ => IsVisible = _ != null)
            .DisposeItWith(Disposable);
    }

    [Reactive]
    public override ReadOnlyObservableCollection<IHeaderMenuItem>? Items { get; set; }
}