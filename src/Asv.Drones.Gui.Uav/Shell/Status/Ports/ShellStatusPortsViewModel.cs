using Asv.Drones.Gui.Core;
using Avalonia.Controls;
using ReactiveUI.Fody.Helpers;
using System.ComponentModel.Composition;
using System.Reactive.Linq;
using Asv.Common;
using ReactiveUI;

namespace Asv.Drones.Gui.Uav
{

    

    [Export(typeof(IShellStatusItem))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class ShellStatusPortsViewModel : ShellStatusItem
    {
        private readonly IncrementalRateCounter _rxRate = new();
        private readonly IncrementalRateCounter _txRate = new();
        
        /// <summary>
        /// This constructor is used by design time tools
        /// </summary>
        public ShellStatusPortsViewModel() : base("asv:shell.status.ports")
        {
            if (Design.IsDesignMode)
            {
                TotalRateOutString = "1 024 kB";
                TotalRateInString = "1 024 kB";
            }
        }

        [ImportingConstructor]
        public ShellStatusPortsViewModel(IMavlinkDevicesService deviceSvc, ILocalizationService localization) : this()
        {
            Observable.Timer(TimeSpan.FromMilliseconds(100), TimeSpan.FromSeconds(1),RxApp.MainThreadScheduler).Subscribe(_ =>
            {
                var totalRx = deviceSvc.Router.RxBytes;
                var totalTx = deviceSvc.Router.TxBytes;
                TotalRateOutString = localization.ByteRate.ConvertToStringWithUnits(_txRate.Calculate(totalTx));
                TotalRateInString = localization.ByteRate.ConvertToStringWithUnits(_rxRate.Calculate(totalRx));

            }).DisposeItWith(Disposable);

        }


        public override int Order => -2;

        [Reactive]
        public string TotalRateInString { get; set; }
        [Reactive]
        public string TotalRateOutString { get; set; }
    }
    
}