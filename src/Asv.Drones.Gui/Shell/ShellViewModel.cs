using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Composition;
using System.Linq;
using System.Reactive.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Web;
using Asv.Cfg;
using Asv.Common;
using Asv.Drones.Gui.Api;
using Avalonia.Controls;
using DynamicData;
using DynamicData.Binding;
using Material.Icons;
using ReactiveUI.Fody.Helpers;
using MenuItem = Asv.Drones.Gui.Api.MenuItem;


namespace Asv.Drones.Gui;

[Export(typeof(IShell))]
[Shared]
public class ShellViewModel : ViewModelBase, IShell
{
    private readonly IApplicationHost _host;
    private readonly ILogService _log;
    private readonly ReadOnlyObservableCollection<IShellMenuItem> _menuItems = null!;
    private readonly ReadOnlyObservableCollection<IShellMenuItem> _topMenuItems = null!;
    private readonly ReadOnlyObservableCollection<IShellMenuItem> _bottomMenuItems = null!;
    private readonly ReadOnlyObservableCollection<LogMessageViewModel> _messages = null!;
    private readonly ReadOnlyObservableCollection<IShellStatusItem> _statusItems = null!;
    private readonly SourceList<LogMessage> _messageSourceList = new();
    private readonly ReadOnlyObservableCollection<IMenuItem> _headerMenu = null!;
    private readonly IConfiguration _config;
    private readonly IShellMenuForSelectedPage _menuForSelectedPage;
    private int _selectionInProgressFlag = 0;
    private IShellMenuItem? _previousSuccessSelectedMenu;
    private IObservable<IChangeSet<IMenuItem, Uri>> _headerMenuSource;

    public ShellViewModel() : base(WellKnownUri.Shell)
    {
        if (!Design.IsDesignMode) throw new Exception("This ctor can be used only for Design Mode");
        Title = "MissionFile.asv";
        _headerMenu = new(new(new IMenuItem[]
        {
            new MenuItem(new Uri($"asv://{Guid.NewGuid()}")) { Header = "File", Icon = MaterialIconKind.File },
        }));
        _menuItems = new(new(new IShellMenuItem[]
        {
        }));
        _topMenuItems = new(new(new IShellMenuItem[]
        {
        }));
        _bottomMenuItems = new(new(new IShellMenuItem[]
        {
        }));

        _statusItems =
            new ReadOnlyObservableCollection<IShellStatusItem>(new ObservableCollection<IShellStatusItem>(
                new IShellStatusItem[]
                {
                }));
        _messages = new ReadOnlyObservableCollection<LogMessageViewModel>(
            new ObservableCollection<LogMessageViewModel>(new LogMessageViewModel[]
            {
                new(new LogMessage(DateTime.Now, LogMessageType.Error, "Application",
                    "Lorep ipsum asdasd asd a sdasdasd asd ",
                    "asdasdasd a sd asd asd asd a sd a sd asd a sd a sda sd a sdasd")),
                new(new LogMessage(DateTime.Now, LogMessageType.Info, "Application",
                    "Lorep ipsum asdasd asd a sdasdasd asd ",
                    "asdasdasd a sd asd asd asd a sd a sd asd a sd a sda sd a sdasd")),
                new(new LogMessage(DateTime.Now, LogMessageType.Trace, "Application",
                    "Lorep ipsum asdasd asd a sdasdasd asd ",
                    "asdasdasd a sd asd asd asd a sd a sd asd a sd a sda sd a sdasd")),
            }));
    }

    [ImportingConstructor]
    public ShellViewModel(
        IApplicationHost app,
        ILogService log,
        IConfiguration config,
        IShellMenuForSelectedPage menuForSelectedPage,
        [ImportMany(WellKnownUri.ShellHeaderMenu)]
        IEnumerable<IViewModelProvider<IMenuItem>> headerMenuProviders,
        [ImportMany] IEnumerable<IViewModelProvider<IShellMenuItem>> menuProviders,
        [ImportMany] IEnumerable<IViewModelProvider<IShellStatusItem>> statusProviders) : base(WellKnownUri.Shell)
    {
        _host = app ?? throw new ArgumentNullException(nameof(app));
        _log = log ?? throw new ArgumentNullException(nameof(log));
        _config = config ?? throw new ArgumentNullException(nameof(config));
        _menuForSelectedPage = menuForSelectedPage ?? throw new ArgumentNullException(nameof(menuForSelectedPage));
        ArgumentNullException.ThrowIfNull(log);
        ArgumentNullException.ThrowIfNull(menuProviders);
        ArgumentNullException.ThrowIfNull(statusProviders);

        Title = app.Info.Name;

        #region Header menu

        headerMenuProviders.Select(_ => _.Items)
            .Merge()
            .SortBy(_ => _.Order)
            .Bind(out _headerMenu)
            .DisposeMany()
            .Subscribe()
            .DisposeItWith(Disposable);

        #endregion

        #region Subscribe to notifications

        _messageSourceList
            .Connect()
            .Transform(_ => new LogMessageViewModel(_messageSourceList, _))
            .Bind(out _messages)
            .DisposeMany()
            .Subscribe()
            .DisposeItWith(Disposable);
        
        _messageSourceList
            .LimitSizeTo(3)
            .Subscribe()
            .DisposeItWith(Disposable);
        // push new information messages to source cache
        // they will automatically be deleted after a timeout or if the user closes them
        log
            .OnMessage
            .Where(_ => _.Type != LogMessageType.Trace)
            .Subscribe(_ => _messageSourceList.Add(_))
            .DisposeItWith(Disposable);

        #endregion

        #region Build main menu

        var menuItemsProviders = menuProviders as IViewModelProvider<IShellMenuItem>[] ?? menuProviders.ToArray();
        // menu items
        menuItemsProviders.Select(_ => _.Items)
            .Merge()
            .SortBy(_ => _.Order)
            .Bind(out _menuItems)
            .DisposeMany()
            .Subscribe()
            .DisposeItWith(Disposable);
        // filter top menu items
        menuItemsProviders.Select(_ => _.Items)
            .Merge()
            .Filter(_ => _.Position == ShellMenuPosition.Top)
            .SortBy(_ => _.Order)
            .Bind(out _topMenuItems)
            .DisposeMany()
            .Subscribe()
            .DisposeItWith(Disposable);
        // filter bottom menu items
        menuItemsProviders.Select(_ => _.Items)
            .Merge()
            .Filter(_ => _.Position == ShellMenuPosition.Bottom)
            .SortBy(_ => _.Order)
            .Bind(out _bottomMenuItems)
            .DisposeMany()
            .Subscribe()
            .DisposeItWith(Disposable);

        #endregion

        #region Build status items

        statusProviders.Select(_ => _.Items)
            .Merge()
            .SortBy(_ => _.Order)
            .Bind(out _statusItems)
            .DisposeMany()
            .Subscribe()
            .DisposeItWith(Disposable);

        #endregion

        this.WhenValueChanged(x => x.SelectedMenu)
            .Subscribe(OnSelectionChanged)
            .DisposeItWith(Disposable);
        this.WhenValueChanged(x => x.IsPaneOpen)
            .Subscribe(OnPaneOpenChanged)
            .DisposeItWith(Disposable);
    }

    #region Navigation

    public async Task<bool> GoTo(Uri link)
    {
        ArgumentNullException.ThrowIfNull(link);
        if (link.Scheme.Equals(WellKnownUri.UriScheme) == false)
        {
            _log.Error(Title, $"Unknown uri scheme. Want {WellKnownUri.UriScheme}. Got:{link.Scheme}");
        }

        var current = CurrentPage;
        if (current != null)
        {
            var canClose = await current.TryClose();
            if (canClose == false) return false;

            if (current.GetType().GetCustomAttribute<SharedAttribute>() == null)
            {
                current.Dispose();
            }
        }

        try
        {
            var viewModel = _host.Container.GetExport<IShellPage>(link.AbsolutePath);
            if (viewModel == null)
            {
                throw new Exception($"Can't find view model for uri {link}");
            }

            if (string.IsNullOrWhiteSpace(link.Query) == false)
            {
                viewModel.SetArgs(HttpUtility.ParseQueryString(link.Query));
            }
            else
            {
                viewModel.SetArgs([]);
            }

            CurrentPage = viewModel;
            if (SelectedMenu?.NavigateTo != CurrentPage.Id)
            {
                SelectedMenu = _menuItems.FirstOrDefault(x => x.NavigateTo == CurrentPage.Id);
            }
            return true;
        }
        catch (Exception? e)
        {
            _log.Error(Title, $"Error to navigate {link}", e);
            return false;
        }
    }

    #endregion


    private void OnPaneOpenChanged(bool isPanOpened)
    {
        if (SelectedMenu == null) return;
        if (isPanOpened)
        {
        }
        else
        {
            if (_previousSuccessSelectedMenu?.Parent != null)
            {
                _previousSuccessSelectedMenu.Parent.IsSelected = true;
            }
        }
    }


    private async void OnSelectionChanged(object? item)
    {
        var newItem = item as IShellMenuItem;
        if (newItem == null) return;
        // if we don't need change selection
        if (newItem == _previousSuccessSelectedMenu) return;
        if (newItem.Type != ShellMenuItemType.PageNavigation) return;

        var isNavigationSuccess = await GoTo(newItem.NavigateTo);
        if (isNavigationSuccess)
        {
            _previousSuccessSelectedMenu = newItem;
            _menuForSelectedPage.SelectedPageChanged(CurrentPage);
        }
        else
        {
            if (_previousSuccessSelectedMenu?.Parent != null)
            {
                if (IsPaneOpen)
                {
                    _previousSuccessSelectedMenu.IsSelected = true;
                    SelectedMenu = _previousSuccessSelectedMenu;
                }
                else
                {
                    _previousSuccessSelectedMenu.Parent.IsSelected = true;
                }
            }
            else
            {
                if (_previousSuccessSelectedMenu != null)
                {
                    _previousSuccessSelectedMenu.IsSelected = true;
                }
            }
        }
    }

    [Reactive] public string Title { get; set; } = null!;

    [Reactive] public IShellPage? CurrentPage { get; set; }

    [Reactive] public IShellMenuItem? SelectedMenu { get; set; }
    public ReadOnlyObservableCollection<IMenuItem> HeaderMenuItems => _headerMenu;
    public ReadOnlyObservableCollection<IShellMenuItem> TopMenuItems => _topMenuItems;
    public ReadOnlyObservableCollection<IShellMenuItem> BottomMenuItems => _bottomMenuItems;
    public ReadOnlyObservableCollection<LogMessageViewModel> Messages => _messages;
    public ReadOnlyObservableCollection<IShellStatusItem> StatusItems => _statusItems;

    [Reactive] public bool IsPaneOpen { get; set; }
}