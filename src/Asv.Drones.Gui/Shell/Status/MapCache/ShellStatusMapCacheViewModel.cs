using System;
using System.Composition;
using System.Reactive;
using System.Reactive.Linq;
using Asv.Common;
using Asv.Drones.Gui.Api;
using Avalonia.Controls;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Asv.Drones.Gui
{
    [Export(typeof(IShellStatusItem))]
    public class ShellStatusMapCacheViewModel : ShellStatusItem
    {
        public ShellStatusMapCacheViewModel() : base(WellKnownUri.Undefined)
        {
            DesignTime.ThrowIfNotDesignMode();
            CacheSizeString = "1 024 KB";
            NavigateToSettings = ReactiveCommand.Create(() => { });
        }

       

        [ImportingConstructor]
        public ShellStatusMapCacheViewModel(IMapService app, ILocalizationService localization, IApplicationHost host)
            : base(WellKnownUri.ShellStatusMapCache)
        {
            Observable.Timer(TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(60), RxApp.MainThreadScheduler)
                .Subscribe(_ =>
                {
                    CacheSizeString = localization.ByteSize.ConvertToStringWithUnits(app.CalculateMapCacheSize());
                }).DisposeItWith(Disposable);
            Description = app.MapCacheDirectory;
            NavigateToSettings = ReactiveCommand.Create(() =>
            {
                host.Shell?.GoTo(SettingsPageViewModel.GenerateUri(true));
                if (host.Shell?.CurrentPage is SettingsPageViewModel settings)
                {
                    settings.Settings.GoTo(WellKnownUri.ShellPageSettingsConnectionsUri);
                }
            });
        }

        public ReactiveCommand<Unit,Unit> NavigateToSettings { get; set; }
        public override int Order => -1;

        [Reactive] public string CacheSizeString { get; set; }

        public string Description { get; }
    }
}