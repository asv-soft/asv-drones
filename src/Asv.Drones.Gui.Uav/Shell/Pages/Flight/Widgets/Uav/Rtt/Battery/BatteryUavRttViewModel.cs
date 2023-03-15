using Asv.Common;
using Asv.Mavlink;
using ReactiveUI.Fody.Helpers;

namespace Asv.Drones.Gui.Uav;

public class BatteryUavRttViewModel:UavRttItem
{
    public BatteryUavRttViewModel()
    {
        BatteryLevel = 0.7;
    }
    public BatteryUavRttViewModel(IVehicle vehicle) : base(vehicle, GenerateRtt(vehicle,"battery"))
    {
        Vehicle.BatteryCharge.Subscribe(_ => BatteryLevel = _.Value).DisposeItWith(Disposable);
    }
    [Reactive]
    public double BatteryLevel { get; set; }
    
}