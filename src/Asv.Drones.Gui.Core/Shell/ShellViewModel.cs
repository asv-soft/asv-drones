using Avalonia.Controls;
using DynamicData;
using Material.Icons;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel.Composition;
using System.Drawing;
using System.Reactive;
using System.Reactive.Linq;
using Asv.Cfg;
using Asv.Common;
using DynamicData.Binding;

namespace Asv.Drones.Gui.Core
{
    [Export]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class ShellViewModel : DisposableViewModelBase, IShell
    {
        private readonly IConfiguration _config;
        private readonly IAppService _appService;
        private readonly IThemeService _themeService;
        private readonly INavigationService _navigation;
        private readonly ILocalizationService _localization;
        private readonly ILogService _logService;
        private IShellMenuItem _selectedMenu;
        private readonly ReadOnlyObservableCollection<IShellMenuItem> _menuItems;
        private readonly ReadOnlyObservableCollection<IShellMenuItem> _footerMenuItems;
        private readonly ReadOnlyObservableCollection<LogMessageViewModel> _messages;
        private readonly SourceList<LogMessage> _messageCache;

        public ShellViewModel()
        {
            if (Design.IsDesignMode)
            {
                Title = "MissionFile.asv";
                _menuItems =
                    new ReadOnlyObservableCollection<IShellMenuItem>(new ObservableCollection<IShellMenuItem>(new IShellMenuItem[] 
                    {

                    }));
                _footerMenuItems = new ReadOnlyObservableCollection<IShellMenuItem>(
                    new ObservableCollection<IShellMenuItem>(new IShellMenuItem[]
                    {

                    }));
                _messages = new ReadOnlyObservableCollection<LogMessageViewModel>(
                    new ObservableCollection<LogMessageViewModel>(new LogMessageViewModel[]
                    {
                        new(new LogMessage(DateTime.Now, LogMessageType.Error,"Application","Lorep ipsum asdasd asd a sdasdasd asd ","asdasdasd a sd asd asd asd a sd a sd asd a sd a sda sd a sdasd")),
                        new(new LogMessage(DateTime.Now, LogMessageType.Info,"Application","Lorep ipsum asdasd asd a sdasdasd asd ","asdasdasd a sd asd asd asd a sd a sd asd a sd a sda sd a sdasd")),
                        new(new LogMessage(DateTime.Now, LogMessageType.Trace,"Application","Lorep ipsum asdasd asd a sdasdasd asd ","asdasdasd a sd asd asd asd a sd a sd asd a sd a sda sd a sdasd")),
                        new(new LogMessage(DateTime.Now, LogMessageType.Info,"Application","Lorep ipsum asdasd asd a sdasdasd asd ","asdasdasd a sd asd asd asd a sd a sd asd a sd a sda sd a sdasd"))
                    }));
            }
        }

        [ImportingConstructor]
        public ShellViewModel(
            IConfiguration config,
            IAppService appService,
            IThemeService themeService,
            INavigationService navigation,
            ILocalizationService localization,
            ILogService logService,
            [ImportMany] IEnumerable<IViewModelProvider<IShellMenuItem>> menuItems
            ) : this()
        {
            if (menuItems == null) throw new ArgumentNullException(nameof(menuItems));
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _appService = appService ?? throw new ArgumentNullException(nameof(appService));
            _themeService = themeService;
            _localization = localization ?? throw new ArgumentNullException(nameof(localization));
            _logService = logService ?? throw new ArgumentNullException(nameof(logService));
            _navigation = navigation ?? throw new ArgumentNullException(nameof(navigation));
            _navigation.Init(this);


            _messageCache = new SourceList<LogMessage>();
            _messageCache
                .Connect()
                .ObserveOn(RxApp.MainThreadScheduler)
                .Transform(_=>new LogMessageViewModel(_messageCache,_))
                .Bind(out _messages)
                .DisposeMany()
                .Subscribe()
                .DisposeItWith(Disposable);
            _logService
                .OnMessage
                .Where(_ => _.Type != LogMessageType.Trace)
                .Subscribe(_ => _messageCache.Add(_))
                .DisposeItWith(Disposable);



            menuItems.Select(_ => _.Items)
                .Merge()
                .Filter(_ => _.Position == ShellMenuPosition.Top)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Bind(out _menuItems)
                .SortBy(_ => _.Order)
                .Subscribe()
                .DisposeItWith(Disposable);

            menuItems.Select(_ => _.Items)
                .Merge()
                .Filter(_ => _.Position == ShellMenuPosition.Bottom)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Bind(out _footerMenuItems)
                .SortBy(_ => _.Order)
                .Subscribe()
                .DisposeItWith(Disposable);
        }
        
        [Reactive]
        public string Title { get; set; }

        [Reactive]
        public IShellPage CurrentPage { get; set; }

        public IShellMenuItem SelectedMenu
        {
            get => _selectedMenu;
            set
            {
                _selectedMenu = value;
                if (_selectedMenu.Type == ShellMenuItemType.PageNavigation)
                {
                    _navigation.GoTo(_selectedMenu.NavigateTo);
                }
            }
        }

        public ReadOnlyObservableCollection<IShellMenuItem> MenuItems => _menuItems;
        public ReadOnlyObservableCollection<IShellMenuItem> FooterMenuItems => _footerMenuItems;
        public ReadOnlyObservableCollection<LogMessageViewModel> Messages => _messages;
    }
}