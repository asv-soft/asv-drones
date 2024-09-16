using System.Collections.ObjectModel;
using System.Composition;
using System.Reactive;
using System.Reactive.Linq;
using System.Reflection;
using Asv.Common;
using DynamicData;
using DynamicData.Binding;
using Material.Icons;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Asv.Drones.Gui.Api;

public class TreePageExplorerViewModel : DisposableReactiveObject,ITreePageExplorer
{
    private readonly ITreePageContext _context;
    private readonly ILogService _log;
    private readonly ReadOnlyObservableCollection<TreePartMenuItemContainer> _tree;
    private readonly CircularBuffer2<TreePartMenuItemContainer> _backwardHistory = new(10);
    private readonly CircularBuffer2<TreePartMenuItemContainer> _forwardHistory = new(10);
    private bool _addToHistory = true;
    private TreePartMenuItemContainer? _lastNavigationItem;
    private int _isNavigationInProgress;

    public TreePageExplorerViewModel() : this(TreePageExplorerDesignTime.Instances, TreePageExplorerDesignTime.Instance,
        NullLogService.Instance)
    {
        DesignTime.ThrowIfNotDesignMode();
        Title = "Tree view";
        GoBack = ReactiveCommand.Create(() => { });
        GoForward = ReactiveCommand.Create(() => { });
        BreadCrumb.AddRange(TreePageExplorerDesignTime.Instance.BreadCrumbs);
    }


    public TreePageExplorerViewModel(IEnumerable<IViewModelProvider<ITreePageMenuItem>> items, ITreePageContext context,
        ILogService log)
    {
        _context = context;
        _log = log;
        items.Select(x => x.Items)
            .MergeChangeSets()
            .TransformToTree(x => x.ParentId)
            .Transform(x => new TreePartMenuItemContainer(x))
            .SortBy(x => x.Base.Order)
            .Bind(out _tree)
            .DisposeMany()
            .Subscribe()
            .DisposeItWith(Disposable);
        

        this.WhenValueChanged(x => x.SelectedMenuContainer, notifyOnInitialValue:false)
            .Where(x=>x != null)
            .Do(AddToHistory)
            .Do(x=>SelectedMenu = x?.Base)
            .Subscribe(x=>Navigate(x).Wait())
            .DisposeItWith(Disposable);

        Disposable.AddAction(() =>
        {
            if (CurrentPage != null && CurrentPage.GetType().GetCustomAttribute<SharedAttribute>() == null)
            {
                CurrentPage.Dispose();
            }
        });

        GoForward = ReactiveCommand.Create(GoForwardImpl, this.WhenAnyValue(x => x.CanGoForward))
            .DisposeItWith(Disposable);
        GoBack = ReactiveCommand.Create(GoBackImpl, this.WhenAnyValue(x => x.CanGoBack)).DisposeItWith(Disposable);
        BreadCrumb.CollectionChanged += (sender, args) =>
        {

        };
    }

    [Reactive] public bool IsCompactMode { get; set; }
    [Reactive] public bool IsTitleCompactMode { get; set; }

    public Task<bool> GoTo(Uri pageId)
    {
        ArgumentNullException.ThrowIfNull(pageId);
        if (pageId.Scheme.Equals(WellKnownUri.UriScheme) == false)
        {
            _log.Error(Title, $"Unknown uri scheme. Want {WellKnownUri.UriScheme}. Got:{pageId.Scheme}");
            return Task.FromResult(false);
        }

        return Navigate(Find(pageId));
    }

    [Reactive]
    public ITreePageMenuItem? SelectedMenu { get; set; }

    private TreePartMenuItemContainer? Find(Uri pageId, TreePartMenuItemContainer? root = null)
    {
        if (root != null)
        {
            if (root.Id == pageId) return root;
        }
        var items = root?.Items ?? _tree;
        foreach (var item in items)
        {
            var result = Find(pageId, item);
            if (result != null) return result;
        }
        return null;
    }
    
    private async Task<bool> Navigate(TreePartMenuItemContainer? menu)
    {
        if (menu == null) return false;
        if (Interlocked.CompareExchange(ref _isNavigationInProgress, 1, 0) != 0)
        {
            // recursive or fast change navigation will be ignored
            return false;
        }

        try
        {
            var currentPage = CurrentPage;
            if (currentPage != null)
            {
                var canClose = await currentPage.TryClose();
                if (canClose == false) return false; // can't close, it's busy now
                if (currentPage.GetType().GetCustomAttribute<SharedAttribute>() == null)
                {
                    currentPage.Dispose();
                }
            }

            var part = menu.Base.CreatePage(_context) ?? new TreeGroupViewModel(menu, Navigate);


            CurrentPage = part;
            
            var stack = new Stack<ITreePageMenuItem>();
            stack.Push(menu.Base);
            var current = menu.Node.Parent;
            while (current.HasValue)
            {
                stack.Push(current.Value.Item);
                current = current.Value.Parent;
            }

            BreadCrumb.Clear();
            while (stack.Count > 0)
            {
                var item = stack.Pop();
                BreadCrumb.Add(new BreadCrumbItem(BreadCrumb.Count == 0, item));
            }

            menu.IsSelected = true;
            return true;
        }
        catch (Exception? e)
        {
            _log.Error(Title, $"Can't create page {menu.Base.Name}:{e.Message}", e);
            return false;
        }
        finally
        {
            Interlocked.Exchange(ref _isNavigationInProgress, 0);
        }
    }

    [Reactive] public string Title { get; set; }
    [Reactive] public MaterialIconKind Icon { get; set; }

    [Reactive] public TreePartMenuItemContainer? SelectedMenuContainer { get; set; }

    [Reactive] public ITreePage? CurrentPage { get; set; }

    public ReadOnlyObservableCollection<TreePartMenuItemContainer> Items => _tree;

    public ObservableCollectionExtended<BreadCrumbItem> BreadCrumb { get; } = [];

    #region Navigation

    private void AddToHistory(TreePartMenuItemContainer? menu)
    {
        if (_addToHistory && _lastNavigationItem != null)
        {
            _backwardHistory.PushFront(_lastNavigationItem);
            _forwardHistory.Clear();
            CanGoForward = false;
            CanGoBack = true;
        }

        _lastNavigationItem = menu;
    }

    [Reactive] public bool CanGoForward { get; set; }
    public ReactiveCommand<Unit, Unit> GoForward { get; }

    private async void GoForwardImpl()
    {
        if (_forwardHistory.IsEmpty) return;
        var item = _forwardHistory[0];
        _forwardHistory.PopFront();
        CanGoForward = _forwardHistory.IsEmpty == false;
        _addToHistory = false;
        await Navigate(item);
        _addToHistory = true;
    }

    [Reactive] public bool CanGoBack { get; set; }
    public ReactiveCommand<Unit, Unit> GoBack { get; }


    private async void GoBackImpl()
    {
        if (_backwardHistory.IsEmpty) return;
        var item = _backwardHistory[0];
        _backwardHistory.PopFront();
        CanGoBack = _backwardHistory.IsEmpty == false;
        if (SelectedMenuContainer != null)
        {
            _forwardHistory.PushFront(SelectedMenuContainer);
        }

        CanGoForward = true;
        _addToHistory = false;
        await Navigate(item);
        _addToHistory = true;
    }

    #endregion
}

public class BreadCrumbItem(bool isFirst, ITreePageMenuItem item)
{
    public bool IsFirst { get; } = isFirst;
    public ITreePageMenuItem Item { get; } = item;
}

public class TreePartMenuItemContainer : ViewModelBase
{
    private readonly ReadOnlyObservableCollection<TreePartMenuItemContainer> _items;

    public TreePartMenuItemContainer(Node<ITreePageMenuItem, Uri> node) : base(node.Item.Id)
    {
        Node = node;
        node.Children.Connect()
            .Transform(x => new TreePartMenuItemContainer(x))
            .SortBy(x => x.Base.Order)
            .Bind(out _items)
            .DisposeMany()
            .Subscribe()
            .DisposeItWith(Disposable);

        Base = node.Item;
    }

    public Node<ITreePageMenuItem, Uri> Node { get; }

    public ITreePageMenuItem Base { get; set; }

    public ReadOnlyObservableCollection<TreePartMenuItemContainer> Items => _items;

    [Reactive] public bool IsExpanded { get; set; } = true;

    [Reactive] public bool IsSelected { get; set; }
}