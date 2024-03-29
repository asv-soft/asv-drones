using Avalonia.Controls;
using DynamicData;
using ReactiveUI.Fody.Helpers;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Reactive.Linq;
using Asv.Cfg;
using Asv.Common;
using DynamicData.Binding;
using Material.Icons;

namespace Asv.Drones.Gui.Core
{
    [Export(typeof(IShell))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class ShellViewModel : ViewModelBase, IShell
    {
        public const string UriString = $"asv:shell";
        public static readonly Uri Uri = new(UriString);
        
        private readonly INavigationService _navigation;
        
        private readonly ReadOnlyObservableCollection<IShellMenuItem> _menuItems = null!;
        private readonly ReadOnlyObservableCollection<IShellMenuItem> _footerMenuItems = null!;
        private readonly ReadOnlyObservableCollection<LogMessageViewModel> _messages = null!;
        private readonly ReadOnlyObservableCollection<IShellStatusItem> _statusItems = null!;
        private readonly SourceList<LogMessage> _messageSourceList = new();
        private readonly ReadOnlyObservableCollection<IHeaderMenuItem> _headerMenu = null!;
        private readonly IThemeService _themeService;
        private readonly IConfiguration _config;
        private readonly IShellMenuForSelectedPage _menuForSelectedPage;
        private int _selectionInProgressFlag = 0;
        private IShellMenuItem _previousSuccessSelectedMenu;
        private IObservable<IChangeSet<IHeaderMenuItem,Uri>> _headerMenuSource;

        public ShellViewModel():base(Uri)
        {
            if (Design.IsDesignMode)
            {
                Title = "MissionFile.asv";
                _headerMenu = new(new(new IHeaderMenuItem[]
                {
                    new HeaderMenuItem(new Uri($"asv://{Guid.NewGuid()}")){Header = "File", Icon = MaterialIconKind.File},
                }));
                _menuItems = new (new (new IShellMenuItem[] 
                    {

                    }));
                _footerMenuItems = new (new(new IShellMenuItem[]
                    {

                    }));

                _statusItems =
                    new ReadOnlyObservableCollection<IShellStatusItem>(new ObservableCollection<IShellStatusItem>(new IShellStatusItem[]
                    {
                        new ShellStatusMapCacheViewModel(),
                        new ShellStatusFileStorageViewModel()
                    }));
                _messages = new ReadOnlyObservableCollection<LogMessageViewModel>(
                    new ObservableCollection<LogMessageViewModel>(new LogMessageViewModel[]
                    {
                        new(new LogMessage(DateTime.Now, LogMessageType.Error,"Application","Lorep ipsum asdasd asd a sdasdasd asd ","asdasdasd a sd asd asd asd a sd a sd asd a sd a sda sd a sdasd")),
                        new(new LogMessage(DateTime.Now, LogMessageType.Info,"Application","Lorep ipsum asdasd asd a sdasdasd asd ","asdasdasd a sd asd asd asd a sd a sd asd a sd a sda sd a sdasd")),
                        new(new LogMessage(DateTime.Now, LogMessageType.Trace,"Application","Lorep ipsum asdasd asd a sdasdasd asd ","asdasdasd a sd asd asd asd a sd a sd asd a sd a sda sd a sdasd")),
                    }));
            }
        }

        [ImportingConstructor]
        public ShellViewModel(
            IAppService app,
            INavigationService navigation, 
            ILogService logService, 
            IThemeService themeService,
            IConfiguration config,
            IShellMenuForSelectedPage menuForSelectedPage,
            [ImportMany(HeaderMenuItem.UriString)] IEnumerable<IViewModelProvider<IHeaderMenuItem>> headerMenuProviders,
            [ImportMany] IEnumerable<IViewModelProvider<IShellMenuItem>> menuProviders,
            [ImportMany] IEnumerable<IViewModelProvider<IShellStatusItem>> statusProviders) : this()
        {
            _navigation = navigation ?? throw new ArgumentNullException(nameof(navigation));
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _menuForSelectedPage = menuForSelectedPage ?? throw new ArgumentNullException(nameof(menuForSelectedPage));
            if (logService == null) throw new ArgumentNullException(nameof(logService));
            if (menuProviders == null) throw new ArgumentNullException(nameof(menuProviders));
            if (statusProviders == null) throw new ArgumentNullException(nameof(statusProviders));

            _navigation.Init(this);
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
            // push new information messages to source cache
            // they will automatically be deleted after a timeout or if the user closes them
            logService
                .OnMessage
                .Where(_ => _.Type != LogMessageType.Trace)
                .Subscribe(_ => _messageSourceList.Add(_))
                .DisposeItWith(Disposable);

            #endregion

            #region Build main menu

            var menuItemsProviders = menuProviders as IViewModelProvider<IShellMenuItem>[] ?? menuProviders.ToArray();
            // filter top menu items
            menuItemsProviders.Select(_ => _.Items)
                .Merge()
                .Filter(_ => _.Position == ShellMenuPosition.Top)
                .SortBy(_ => _.Order)
                .Bind(out _menuItems)
                .DisposeMany()
                .Subscribe()
                .DisposeItWith(Disposable);
            // filter bottom menu items
            menuItemsProviders.Select(_ => _.Items)
                .Merge()
                .Filter(_ => _.Position == ShellMenuPosition.Bottom)
                .SortBy(_ => _.Order)
                .Bind(out _footerMenuItems)
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
            
            _themeService = themeService;
            
            this.WhenValueChanged(x => x.SelectedMenu)
                .Subscribe(OnSelectionChanged)
                .DisposeItWith(Disposable);
            this.WhenValueChanged(x => x.IsPaneOpen)
                .Subscribe(OnPaneOpenChanged)
                .DisposeItWith(Disposable);
            
        }
        
        

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

            var isNavigationSuccess = await _navigation.GoTo(newItem.NavigateTo);
            if (isNavigationSuccess)
            {
                _previousSuccessSelectedMenu = newItem;
                _menuForSelectedPage.SelectedPageChanged(CurrentPage);
            }
            else
            {
                if (_previousSuccessSelectedMenu.Parent != null)
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
                    _previousSuccessSelectedMenu.IsSelected = true;
                }

            }


        }

        [Reactive]
        public string Title { get; set; } = null!;

        [Reactive]
        public IShellPage CurrentPage { get; set; } = null!;

        [Reactive] 
        public object? SelectedMenu { get; set; } = null!;
        public ReadOnlyObservableCollection<IHeaderMenuItem> HeaderMenuItems => _headerMenu;
        public ReadOnlyObservableCollection<IShellMenuItem> MenuItems => _menuItems;
        public ReadOnlyObservableCollection<IShellMenuItem> FooterMenuItems => _footerMenuItems;
        public ReadOnlyObservableCollection<LogMessageViewModel> Messages => _messages;
        public ReadOnlyObservableCollection<IShellStatusItem> StatusItems => _statusItems;

        [Reactive]
        public bool IsPaneOpen { get; set; }

        public void OnLoaded()
        {
            var themeConfig = _config.Get<ThemeServiceConfig>();
            var themeFromConfig = _themeService.Themes.FirstOrDefault(_ => _.Id.Equals(themeConfig.Theme));
            if (themeFromConfig != null) _themeService.CurrentTheme.OnNext(themeFromConfig);
        }

    }
}