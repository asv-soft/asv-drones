using System.ComponentModel.Composition;
using System.Reactive.Linq;
using Asv.Common;
using Asv.Drones.Gui.Core;
using Asv.Mavlink;
using Avalonia.Controls.Mixins;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Asv.Drones.Gui.Uav;

public class CurrentUavRttViewModel : UavRttItem
{
    public CurrentUavRttViewModel()
    {
        Current = "0.7 A";
    }
    
    public CurrentUavRttViewModel(IVehicleClient vehicle, ILocalizationService localization) : base(vehicle, GenerateRtt(vehicle,"current"))
    {
        Order = 4;
        Vehicle.Rtt.BatteryCurrent
            .DistinctUntilChanged()
            .Sample(TimeSpan.FromMilliseconds(500), RxApp.MainThreadScheduler)
            .Subscribe(_ => Current = $"{localization.Current.ConvertToStringWithUnits(_)}")
            .DisposeWith(Disposable);
    }
    
    [Reactive]
    public string Current { get; set; }
    
}
