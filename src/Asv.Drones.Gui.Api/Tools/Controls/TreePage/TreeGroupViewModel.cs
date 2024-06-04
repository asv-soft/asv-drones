using System.Collections.ObjectModel;
using System.Reactive;
using Asv.Common;
using DynamicData;
using ReactiveUI;

namespace Asv.Drones.Gui.Api;

public class TreeGroupViewModel : ViewModelBase, ITreePage
{
    private readonly Func<TreePartMenuItemContainer?, Task<bool>> _navigate;

    public TreeGroupViewModel() : base(TreePageExplorerDesignTime.Instance.BreadCrumbs.First().Item.Id)
    {
        var item = TreePageExplorerDesignTime.Instance.BreadCrumbs.First().Item;
        var node = new Node<ITreePageMenuItem, Uri>(item, WellKnownUri.UndefinedUri);

        Items = new ReadOnlyObservableCollection<TreePartMenuItemContainer>(
            new ObservableCollection<TreePartMenuItemContainer>(
                new[]
                {
                    new TreePartMenuItemContainer(new Node<ITreePageMenuItem, Uri>(item, WellKnownUri.UndefinedUri)),
                    new TreePartMenuItemContainer(new Node<ITreePageMenuItem, Uri>(item, WellKnownUri.UndefinedUri))
                }));


        Item = new TreePartMenuItemContainer(node);
    }

    public TreeGroupViewModel(TreePartMenuItemContainer menu, Func<TreePartMenuItemContainer?, Task<bool>> navigate) :
        base(menu.Base.Id)
    {
        _navigate = navigate;
        Item = menu;
        Items = menu.Items;
        NavigateCommand = ReactiveCommand.Create<TreePartMenuItemContainer, Unit>(Navigate).DisposeItWith(Disposable);
    }

    private Unit Navigate(TreePartMenuItemContainer arg)
    {
        _navigate(arg);
        return Unit.Default;
    }

    public TreePartMenuItemContainer Item { get; }
    public ReadOnlyObservableCollection<TreePartMenuItemContainer> Items { get; }
    public ReactiveCommand<TreePartMenuItemContainer, Unit> NavigateCommand { get; }
    public ReadOnlyObservableCollection<IMenuItem>? Actions { get; } = null;
    public virtual Task<bool> TryClose()
    {
        return Task.FromResult(true);
    }
}