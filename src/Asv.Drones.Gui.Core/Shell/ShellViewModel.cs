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
        private IShellMenuItem _selectedMenu;
        private readonly ReadOnlyObservableCollection<IShellMenuItem> _menuItems;
        private readonly ReadOnlyObservableCollection<IShellMenuItem> _footerMenuItems;

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
            }
        }

        [ImportingConstructor]
        public ShellViewModel(
            IConfiguration config,
            IAppService appService,
            IThemeService themeService,
            INavigationService navigation,
            ILocalizationService localization,
            [ImportMany] IEnumerable<IViewModelProvider<IShellMenuItem>> menuItems
            ) : this()
        {
            if (menuItems == null) throw new ArgumentNullException(nameof(menuItems));
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _appService = appService ?? throw new ArgumentNullException(nameof(appService));
            _themeService = themeService;
            _localization = localization ?? throw new ArgumentNullException(nameof(localization));
            _navigation = navigation ?? throw new ArgumentNullException(nameof(navigation));
            _navigation.Init(this);

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
    }
}