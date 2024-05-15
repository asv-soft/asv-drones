using System;
using System.Composition;
using System.Reactive;
using System.Reactive.Linq;
using Asv.Common;
using Asv.Drones.Gui.Api;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Asv.Drones.Gui
{
    [Export(typeof(IShellStatusItem))]
    public class ShellStatusPortsViewModel : ShellStatusItem
    {
        private readonly IncrementalRateCounter _rxRate = new();
        private readonly IncrementalRateCounter _txRate = new();

        /// <summary>
        /// This constructor is used by design time tools
        /// </summary>
        public ShellStatusPortsViewModel() : base(WellKnownUri.UndefinedUri)
        {
            DesignTime.ThrowIfNotDesignMode();
            TotalRateOutString = "1 024 kB";
            TotalRateInString = "1 024 kB";
            NavigateToSettings = ReactiveCommand.Create(() => { });
        }

        [ImportingConstructor]
        public ShellStatusPortsViewModel(IMavlinkDevicesService deviceSvc, ILocalizationService localization, IApplicationHost host) : base(
            WellKnownUri.ShellStatusPorts)
        {
            Observable.Timer(TimeSpan.FromMilliseconds(100), TimeSpan.FromSeconds(1), RxApp.MainThreadScheduler)
                .Subscribe(_ =>
                {
                    var totalRx = deviceSvc.Router.RxBytes;
                    var totalTx = deviceSvc.Router.TxBytes;
                    TotalRateOutString = localization.ByteRate.ConvertToStringWithUnits(_txRate.Calculate(totalTx));
                    TotalRateInString = localization.ByteRate.ConvertToStringWithUnits(_rxRate.Calculate(totalRx));
                }).DisposeItWith(Disposable);
            NavigateToSettings = ReactiveCommand.Create(() =>
            {
                host.Shell?.GoTo(SettingsPageViewModel.GenerateUri(true));
                if (host.Shell?.CurrentPage is SettingsPageViewModel settings)
                {
                    settings.Settings.GoTo(WellKnownUri.ShellPageSettingsConnectionsPortsUri);
                }
            });
            
        }


        public override int Order => -2;

        [Reactive] public string TotalRateInString { get; set; }
        [Reactive] public string TotalRateOutString { get; set; }
        
        public ReactiveCommand<Unit,Unit> NavigateToSettings { get; }
    }
}