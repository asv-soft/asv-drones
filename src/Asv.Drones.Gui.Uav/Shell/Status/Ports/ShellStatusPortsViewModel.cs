using Asv.Drones.Gui.Core;
using Avalonia.Controls;
using ReactiveUI.Fody.Helpers;
using System.ComponentModel.Composition;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Asv.Common;

namespace Asv.Drones.Gui.Uav
{

    [Export(typeof(IShellStatusItem))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class ShellStatusPortsViewModel : ViewModelBase, IShellStatusItem
    {
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
            
            Observable.Timer(TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(5)).Subscribe(_ =>
            {
                var info = deviceSvc.Router.GetPorts().Select(_ => deviceSvc.Router.GetInfo(_)).Where(_ => _ != null).ToArray();
                var totalRx = info.Sum(_ => _.RxBytes);
                var totalTx = info.Sum(_ => _.TxBytes);

                TotalRateString = $"{localization.RateToString(totalRx)} | {localization.RateToString(totalTx)}";
            }).DisposeItWith(Disposable);

        }


        public int Order => 0;

        [Reactive]
        public string TotalRateString { get; set; }
    }
    
}