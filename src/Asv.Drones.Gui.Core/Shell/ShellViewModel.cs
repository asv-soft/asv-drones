using Avalonia.Controls;
using DynamicData;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Reactive.Linq;
using Asv.Common;

namespace Asv.Drones.Gui.Core
{
    [Export]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class ShellViewModel : ViewModelBase, IShell
    {
        private readonly INavigationService _navigation;
        private IShellMenuItem _selectedMenu = null!;
        private readonly ReadOnlyObservableCollection<IShellMenuItem> _menuItems = null!;
        private readonly ReadOnlyObservableCollection<IShellMenuItem> _footerMenuItems = null!;
        private readonly ReadOnlyObservableCollection<LogMessageViewModel> _messages = null!;
        private readonly ReadOnlyObservableCollection<IShellStatusItem> _statusItems = null!;
        private readonly SourceList<LogMessage> _messageSourceList = new();
        

        public ShellViewModel():base(WellKnownUri.ShellBaseUri)
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

                _statusItems =
                    new ReadOnlyObservableCollection<IShellStatusItem>(new ObservableCollection<IShellStatusItem>(new IShellStatusItem[]
                    {
                        new ShellStatusMapCacheViewModel()
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
            INavigationService navigation, 
            ILogService logService, 
            [ImportMany] IEnumerable<IViewModelProvider<IShellMenuItem>> menuProviders,
            [ImportMany] IEnumerable<IViewModelProvider<IShellStatusItem>> statusProviders) : this()    
        {
            _navigation = navigation ?? throw new ArgumentNullException(nameof(navigation));
            if (logService == null) throw new ArgumentNullException(nameof(logService));
            if (menuProviders == null) throw new ArgumentNullException(nameof(menuProviders));
            if (statusProviders == null) throw new ArgumentNullException(nameof(statusProviders));

            _navigation.Init(this);

            #region Subscribe to notifications

            _messageSourceList
                .Connect()
                .ObserveOn(RxApp.MainThreadScheduler)
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
                .ObserveOn(RxApp.MainThreadScheduler)
                .SortBy(_ => _.Order)
                .Bind(out _menuItems)
                .Subscribe()
                .DisposeItWith(Disposable);
            // filter bottom menu items
            menuItemsProviders.Select(_ => _.Items)
                .Merge()
                .Filter(_ => _.Position == ShellMenuPosition.Bottom)
                .ObserveOn(RxApp.MainThreadScheduler)
                .SortBy(_ => _.Order)
                .Bind(out _footerMenuItems)
                .Subscribe()
                .DisposeItWith(Disposable);

            #endregion

            #region Build status items

            statusProviders.Select(_ => _.Items)
                .Merge()
                .ObserveOn(RxApp.MainThreadScheduler)
                .SortBy(_ => _.Order)
                .Bind(out _statusItems)
                .Subscribe()
                .DisposeItWith(Disposable);

            #endregion
        }
        
        [Reactive]
        public string Title { get; set; } = null!;

        [Reactive]
        public IShellPage CurrentPage { get; set; } = null!;

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
        public ReadOnlyObservableCollection<IShellStatusItem> StatusItems => _statusItems;

    }
}