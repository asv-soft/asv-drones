using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Reactive.Linq;
using Asv.Common;
using Avalonia.Controls;
using DynamicData;
using DynamicData.Binding;
using ReactiveUI;

namespace Asv.Drones.Gui.Core
{


    [ExportShellPage(UriString)]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class SettingsViewModel:ViewModelBase,IShellPage
    {
        public const string UriString = ShellPage.UriString + ".settings";
        public static readonly Uri Uri = new Uri(UriString);
        
        private readonly ReadOnlyObservableCollection<ISettingsPart> _items;
        private readonly ObservableAsPropertyHelper<bool> _isRebootRequired;
        /// <summary>
        /// This is a design time constructor
        /// </summary>
        public SettingsViewModel():base(Uri)
        {
            if (Design.IsDesignMode)
            {
                CurrentVersion = "0.0.0";
                AppName = "Asv.Drones";
                AppLicense = "MIT License";
                AppUrl = "https://github.com/asvol/asv-drones";
                CurrentAvaloniaVersion = "0.0.0";
                _isRebootRequired = Observable.Return(true).ToProperty(this, _ => _.IsRebootRequired);
                _items = new ReadOnlyObservableCollection<ISettingsPart>(new ObservableCollection<ISettingsPart>(
                    new ISettingsPart[]
                    {
                        new SettingsThemeViewModel()
                    }));
            }
        }

        [ImportingConstructor]
        public SettingsViewModel(IAppService appService, [ImportMany]IEnumerable<IViewModelProvider<ISettingsPart>> settings):this()
        {
            var appService1 = appService;
            CurrentVersion = appService1.Info.Version;
            AppUrl = appService1.Info.AppUrl;
            Author = appService1.Info.Author;
            AppLicense = appService1.Info.AppLicense;
            AppName = appService1.Info.Name;
            CurrentAvaloniaVersion = appService1.Info.CurrentAvaloniaVersion;

            settings.Select(_ => _.Items)
                .Merge()
                .SortBy(_ => _.Order)
                .Bind(out _items)
                .DisposeMany()
                .Subscribe()
                .DisposeItWith(Disposable);
            
            _items.ToObservableChangeSet()
                .AutoRefresh(_ => _.IsRebootRequired) // update collection when any part require reboot
                .ToCollection()
                .Select(parts => parts.Any(part => part.IsRebootRequired)) // check if any part require reboot
                .ToProperty(this, _ => _.IsRebootRequired, out _isRebootRequired)
                .DisposeItWith(Disposable);
        }

        public string Author { get; set; }
        public string CurrentVersion { get; }
        public string AppUrl { get; }
        public string AppName { get; }
        public string AppLicense { get; }
        public string CurrentAvaloniaVersion { get; }

        public ReadOnlyObservableCollection<ISettingsPart> Items => _items;

        public bool IsRebootRequired => _isRebootRequired.Value;
        
        public void SetArgs(Uri link)
        {
            
        }
    }
}