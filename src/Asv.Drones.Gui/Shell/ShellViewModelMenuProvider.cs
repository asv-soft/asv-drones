using System;
using System.Composition;
using Asv.Common;
using Asv.Drones.Gui.Api;
using DynamicData;

namespace Asv.Drones.Gui;

public interface IShellMenuForSelectedPage
{
    void SelectedPageChanged(IShellPage? page);
}

[Export(typeof(IShellMenuForSelectedPage))]
[Export(WellKnownUri.ShellHeaderMenu, typeof(IViewModelProvider<IMenuItem>))]
[Export(typeof(IViewModelProvider<IShellStatusItem>))]
[Shared]
public class ShellViewModelMenuProvider : DisposableOnceWithCancel, IViewModelProvider<IMenuItem>,
    IViewModelProvider<IShellStatusItem>, IShellMenuForSelectedPage
{
    private readonly SourceCache<IMenuItem, Uri> _header;
    private readonly SourceCache<IShellStatusItem, Uri> _status;
    private IDisposable? _headerSubscription;
    private IDisposable? _statusSubscription;

    [ImportingConstructor]
    public ShellViewModelMenuProvider()
    {
        _header = new SourceCache<IMenuItem, Uri>(model => model.Id).DisposeItWith(Disposable);
        _status = new SourceCache<IShellStatusItem, Uri>(model => model.Id).DisposeItWith(Disposable);
        Disposable.AddAction(() =>
        {
            _headerSubscription?.Dispose();
            _statusSubscription?.Dispose();
        });
    }

    IObservable<IChangeSet<IMenuItem, Uri>> IViewModelProvider<IMenuItem>.Items => _header.Connect();

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