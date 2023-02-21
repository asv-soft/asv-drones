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
using Asv.Common;
using DynamicData.Binding;

namespace Asv.Drones.Gui.Core
{
    [Export]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class ShellViewModel : DisposableViewModelBase, IShell
    {
        private readonly INavigationService _navigation = null!;
        private IShellMenuItem _selectedMenu = null!;
        private readonly ReadOnlyObservableCollection<IShellMenuItem> _menuItems = null!;
        private readonly ReadOnlyObservableCollection<IShellMenuItem> _footerMenuItems = null!;
        private readonly ReadOnlyObservableCollection<LogMessageViewModel> _messages = null!;
        private readonly SourceList<LogMessage> _messageCache = new();

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
            INavigationService navigation, 
            ILogService logService, 
            [ImportMany] IEnumerable<IViewModelProvider<IShellMenuItem>> menuItems) : this()    
        {
            if (menuItems == null) throw new ArgumentNullException(nameof(menuItems));
            var logService1 = logService ?? throw new ArgumentNullException(nameof(logService));
            _navigation = navigation ?? throw new ArgumentNullException(nameof(navigation));
            _navigation.Init(this);

            #region Subscribe to notifications

            _messageCache
                .Connect()
                .ObserveOn(RxApp.MainThreadScheduler)
                .Transform(_ => new LogMessageViewModel(_messageCache, _))
                .Bind(out _messages)
                .DisposeMany()
                .Subscribe()
                .DisposeItWith(Disposable);
            // push new information messages to source cache
            // they will automatically be deleted after a timeout or if the user closes them
            logService1
                .OnMessage
                .Where(_ => _.Type != LogMessageType.Trace)
                .Subscribe(_ => _messageCache.Add(_))
                .DisposeItWith(Disposable);

            #endregion

            #region Build main menu

            var menuItemsProviders = menuItems as IViewModelProvider<IShellMenuItem>[] ?? menuItems.ToArray();
            // filter top menu items
            menuItemsProviders.Select(_ => _.Items)
                .Merge()
                .Filter(_ => _.Position == ShellMenuPosition.Top)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Bind(out _menuItems)
                .SortBy(_ => _.Order)
                .Subscribe()
                .DisposeItWith(Disposable);
            // filter bottom menu items
            menuItemsProviders.Select(_ => _.Items)
                .Merge()
                .Filter(_ => _.Position == ShellMenuPosition.Bottom)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Bind(out _footerMenuItems)
                .SortBy(_ => _.Order)
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
    }
}