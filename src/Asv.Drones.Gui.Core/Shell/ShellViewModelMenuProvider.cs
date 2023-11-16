using System.ComponentModel.Composition;
using Asv.Common;
using DynamicData;

namespace Asv.Drones.Gui.Core;

public interface IShellMenuForSelectedPage
{
    void SelectedPageChanged(IShellPage? page);
}

[Export(typeof(IShellMenuForSelectedPage))]
[Export(HeaderMenuItem.UriString, typeof(IViewModelProvider<IHeaderMenuItem>))]
[Export(typeof(IViewModelProvider<IShellStatusItem>))]
[PartCreationPolicy(CreationPolicy.Shared)]
public class ShellViewModelMenuProvider : DisposableOnceWithCancel,IViewModelProvider<IHeaderMenuItem>, IViewModelProvider<IShellStatusItem>,IShellMenuForSelectedPage
{
    private readonly SourceCache<IHeaderMenuItem, Uri> _header;
    private readonly SourceCache<IShellStatusItem, Uri> _status;
    private IDisposable? _headerSubscription;
    private IDisposable? _statusSubscription;

    [ImportingConstructor]
    public ShellViewModelMenuProvider()
    {
        _header = new SourceCache<IHeaderMenuItem, Uri>(model => model.Id).DisposeItWith(Disposable);
        _status = new SourceCache<IShellStatusItem, Uri>(model => model.Id).DisposeItWith(Disposable);
        Disposable.AddAction(() =>
        {
            _headerSubscription?.Dispose();
            _statusSubscription?.Dispose();
        });
    }
    
    IObservable<IChangeSet<IHeaderMenuItem, Uri>> IViewModelProvider<IHeaderMenuItem>.Items => _header.Connect();

    IObservable<IChangeSet<IShellStatusItem, Uri>> IViewModelProvider<IShellStatusItem>.Items => _status.Connect();
    
    public void SelectedPageChanged(IShellPage? page)
    {
        _headerSubscription?.Dispose();
        _statusSubscription?.Dispose();
        _header.Clear();
        _status.Clear();
        if (page == null) return;
        _headerSubscription = page.HeaderItems.PopulateInto(_header);
        _statusSubscription = page.StatusItems.PopulateInto(_status);
    }
}