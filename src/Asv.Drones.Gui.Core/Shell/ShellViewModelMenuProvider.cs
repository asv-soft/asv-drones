using System.ComponentModel.Composition;
using Asv.Common;
using DynamicData;

namespace Asv.Drones.Gui.Core;

/// <summary>
/// Represents an interface for a shell menu that is associated with a selected page.
/// </summary>
public interface IShellMenuForSelectedPage
{
    /// <summary>
    /// Occurs when the selected page in the shell changes.
    /// </summary>
    /// <param name="page">The newly selected <see cref="IShellPage"/> or null if no page is selected.</param>
    /// <remarks>
    /// This event is triggered whenever the selected page in the shell is changed.
    /// The selected page represents the current active page in the shell application.
    /// </remarks>
    void SelectedPageChanged(IShellPage? page);
}

/// <summary>
/// Provides the menu items for the shell view model and handles the changes in the selected page.
/// </summary>
[Export(typeof(IShellMenuForSelectedPage))]
[Export(HeaderMenuItem.UriString, typeof(IViewModelProvider<IHeaderMenuItem>))]
[Export(typeof(IViewModelProvider<IShellStatusItem>))]
[PartCreationPolicy(CreationPolicy.Shared)]
public class ShellViewModelMenuProvider : DisposableOnceWithCancel,IViewModelProvider<IHeaderMenuItem>, IViewModelProvider<IShellStatusItem>,IShellMenuForSelectedPage
{
    /// <summary>
    /// Represents a source cache of header menu items and their corresponding URIs.
    /// </summary>
    private readonly SourceCache<IHeaderMenuItem, Uri> _header;

    /// <summary>
    /// Represents a source cache of shell status items. </summary>
    /// /
    private readonly SourceCache<IShellStatusItem, Uri> _status;
    private IDisposable? _headerSubscription;

    /// <summary>
    /// Represents the subscription to the status.
    /// </summary>
    private IDisposable? _statusSubscription;

    /// <summary>
    /// Represents a menu provider for the ShellViewModel.
    /// </summary>
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

    /// <summary>
    /// Gets the collection of items displayed in the header.
    /// </summary>
    /// <value>The observable collection of header menu items and their corresponding URIs.</value>
    /// <remarks>
    /// When this property is accessed, it returns an <see cref="IObservable{T}"/> of <see cref="IChangeSet{TObject,TKey}"/> where the object type is <see cref="IHeaderMenuItem"/> and the
    /// key type is <see cref="Uri"/>.
    /// This property is part of the <see cref="IViewModelProvider{T}"/> interface and is implemented by connecting to the <see cref="_header"/> observable sequence.
    /// </remarks>
    IObservable<IChangeSet<IHeaderMenuItem, Uri>> IViewModelProvider<IHeaderMenuItem>.Items => _header.Connect();

    /// <summary>
    /// Gets an observable collection of changes to the <see cref="IShellStatusItem"/> items.
    /// </summary>
    /// <remarks>
    /// This property provides an <see cref="IObservable{T}"/> collection of changes (<see cref="IChangeSet{TKey,T}"/>) to the <see cref="IShellStatusItem"/> items.
    /// </remarks>
    /// <returns>
    /// An <see cref="IObservable{T}"/> collection of changes to the <see cref="IShellStatusItem"/> items.
    /// </returns>
    IObservable<IChangeSet<IShellStatusItem, Uri>> IViewModelProvider<IShellStatusItem>.Items => _status.Connect();

    /// <summary>
    /// Event handler for when the selected page in the shell changes.
    /// </summary>
    /// <param name="page">The selected page in the shell.</param>
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