using Asv.Drones.Gui.Core;
using Avalonia.Controls;
using ReactiveUI.Fody.Helpers;
using System.ComponentModel.Composition;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Asv.Common;

namespace Asv.Drones.Gui.Uav
{

    

    [Export(typeof(IShellStatusItem))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class ShellStatusPortsViewModel : ShellStatusItem
    {
        public static readonly Uri Uri = new(ShellStatusItem.Uri,"ports");
        
        private readonly IncrementalRateCounter _rxRate = new();
        private readonly IncrementalRateCounter _txRate = new();
        
        /// <summary>
        /// This constructor is used by design time tools
        /// </summary>
        public ShellStatusPortsViewModel() : base(Uri)
        {
            if (Design.IsDesignMode)
            {
                TotalRateString = "1 024 kB";
            }
        }

        [ImportingConstructor]
        public ShellStatusPortsViewModel(IMavlinkDevicesService deviceSvc, ILocalizationService localization) : this()
        {
            Observable.Timer(TimeSpan.FromMilliseconds(100), TimeSpan.FromSeconds(1)).Subscribe(_ =>
            {

                var totalRx = deviceSvc.Router.RxBytes;
                var totalTx = deviceSvc.Router.TxBytes;
                TotalRateString = $"{localization.ByteRate.ConvertToStringWithUnits(_txRate.Calculate(totalTx))} | {localization.ByteRate.ConvertToStringWithUnits(_rxRate.Calculate(totalRx))}";

            }).DisposeItWith(Disposable);

        }


        public override int Order => -2;

        [Reactive]
        public string TotalRateString { get; set; }
    }
    
}