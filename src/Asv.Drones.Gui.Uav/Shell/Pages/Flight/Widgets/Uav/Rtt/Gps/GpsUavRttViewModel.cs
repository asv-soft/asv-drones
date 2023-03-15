using Asv.Common;
using Asv.Mavlink;
using Asv.Mavlink.V2.Common;
using ReactiveUI.Fody.Helpers;

namespace Asv.Drones.Gui.Uav;

public class GpsUavRttViewModel : UavRttItem
{
    public GpsUavRttViewModel()
    {
        FixType = GpsFixType.GpsFixTypeNoGps;
        DopStatus = DopStatusEnum.Unknown;
    }

    public GpsUavRttViewModel(IVehicle vehicle) : base(vehicle, GenerateRtt(vehicle, "gps"))
    {
        Vehicle.GpsInfo.Subscribe(_ => FixType = _.FixType).DisposeItWith(Disposable);
        Vehicle.GpsInfo.Subscribe(_ => DopStatus = _.PdopStatus).DisposeItWith(Disposable);
    }
    
    [Reactive]
    public GpsFixType FixType { get; set; }
    [Reactive]
    public DopStatusEnum DopStatus { get; set; }
}