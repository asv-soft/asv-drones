using System.Reactive.Linq;
using Asv.Common;
using Asv.Mavlink;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Asv.Drones.Gui.Uav;

public class BatteryUavRttViewModel:UavRttItem
{
    public BatteryUavRttViewModel()
    {
        BatteryLevel = 0.7;
        BatteryLevelString = "0.7";
    }
    
    public BatteryUavRttViewModel(IVehicleClient vehicle) : base(vehicle, GenerateRtt(vehicle,"battery"))
    {
        Vehicle.Rtt.BatteryCharge
            .Subscribe(_ => BatteryLevel = _)
            .DisposeItWith(Disposable);
        
        Vehicle.Rtt.BatteryCharge
            .Subscribe(_ => BatteryLevelString = _.ToString("P0"))
            .DisposeItWith(Disposable);

        IsMinimizedVisible = true;
    }

    [Reactive]
    public double BatteryLevel { get; set; }
    
    [Reactive]
    public string BatteryLevelString { get; set; } = RS.UavRttItem_ValueNotAvailable;

}