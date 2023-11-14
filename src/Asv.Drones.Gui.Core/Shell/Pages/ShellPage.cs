using Asv.Common;
using DynamicData;
using Material.Icons;
using ReactiveUI.Fody.Helpers;

namespace Asv.Drones.Gui.Core;

public class ShellPage : ViewModelBase,IShellPage
{
    protected ShellPage(Uri uri) : base(uri)
    {
        HeaderItemsSource = new SourceCache<IHeaderMenuItem, Uri>(x=>x.Id).DisposeItWith(Disposable);
        StatusItemsSource = new SourceCache<IShellStatusItem, Uri>(x=>x.Id).DisposeItWith(Disposable);
    }
    protected ShellPage(string uri) : base(uri)
    {
        HeaderItemsSource = new SourceCache<IHeaderMenuItem, Uri>(x=>x.Id).DisposeItWith(Disposable);
        StatusItemsSource = new SourceCache<IShellStatusItem, Uri>(x=>x.Id).DisposeItWith(Disposable);
    }
    
    [Reactive]
    public MaterialIconKind Icon { get; set; }
    [Reactive]
    public string Title { get; set; }
    
    protected ISourceCache<IHeaderMenuItem,Uri> HeaderItemsSource { get; }
    protected ISourceCache<IShellStatusItem,Uri> StatusItemsSource { get; }
    
    public IObservable<IChangeSet<IHeaderMenuItem, Uri>> HeaderItems => HeaderItemsSource.Connect();

    public IObservable<IChangeSet<IShellStatusItem, Uri>> StatusItems => StatusItemsSource.Connect();

    public virtual void SetArgs(Uri link)
    {
            
    }

    public virtual Task<bool> TryClose()
    {
        return Task.FromResult(true);
    }
}