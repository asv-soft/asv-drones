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
    public class ShellStatusPortsViewModel : ViewModelBase, IShellStatusItem
    {
        private readonly IncrementalRateCounter _rxRate = new();
        private readonly IncrementalRateCounter _txRate = new();

        public ShellStatusPortsViewModel() : base(new(WellKnownUri.ShellStatusUri, "ports"))
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
                TotalRateString = $"{localization.ByteRate.GetValueWithUnits(_txRate.Calculate(totalTx))} | {localization.ByteRate.GetValueWithUnits(_rxRate.Calculate(totalRx))}";

            }).DisposeItWith(Disposable);

        }


        public int Order => -2;

        [Reactive]
        public string TotalRateString { get; set; }
    }
    
}