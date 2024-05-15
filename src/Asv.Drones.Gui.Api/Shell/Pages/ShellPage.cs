using System.Collections.Specialized;
using Asv.Common;
using DynamicData;
using Material.Icons;
using ReactiveUI.Fody.Helpers;

namespace Asv.Drones.Gui.Api;

public class ShellPage : ViewModelBase, IShellPage
{
    protected ShellPage(Uri uri) : base(uri)
    {
        HeaderItemsSource = new SourceCache<IMenuItem, Uri>(x => x.Id).DisposeItWith(Disposable);
        StatusItemsSource = new SourceCache<IShellStatusItem, Uri>(x => x.Id).DisposeItWith(Disposable);
    }

    protected ShellPage(string uri) : this(new Uri(uri))
    {
    }

    [Reactive] public MaterialIconKind Icon { get; set; }
    [Reactive] public string Title { get; set; }

    protected ISourceCache<IMenuItem, Uri> HeaderItemsSource { get; }
    protected ISourceCache<IShellStatusItem, Uri> StatusItemsSource { get; }

    public IObservable<IChangeSet<IMenuItem, Uri>> HeaderItems => HeaderItemsSource.Connect();

    public IObservable<IChangeSet<IShellStatusItem, Uri>> StatusItems => StatusItemsSource.Connect();

    public virtual void SetArgs(NameValueCollection args)
    {
    }

    public virtual Task<bool> TryClose()
    {
        return Task.FromResult(true);
    }
}