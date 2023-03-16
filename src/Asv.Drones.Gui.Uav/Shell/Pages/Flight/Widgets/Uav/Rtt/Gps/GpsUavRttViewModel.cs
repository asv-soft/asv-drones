using Asv.Common;
using Asv.Mavlink;
using Asv.Mavlink.V2.Common;
using ReactiveUI.Fody.Helpers;

namespace Asv.Drones.Gui.Uav;

public enum GpsInfo
{
    GpsInfo,
    Gps2Info
}

public class GpsUavRttViewModel : UavRttItem
{
    public GpsUavRttViewModel()
    {
        FixType = GpsFixType.GpsFixTypeNoGps;
        DopStatus = DopStatusEnum.Unknown;
    }

    public GpsUavRttViewModel(IVehicle vehicle, GpsInfo gpsInfo) : base(vehicle, GenerateRtt(vehicle, "gps"))
    {
        switch (gpsInfo)
        {
            case GpsInfo.GpsInfo:
                if (Vehicle.GpsInfo.Value == null || Vehicle.GpsInfo.Value.FixType == GpsFixType.GpsFixTypeNoGps)
                {
                    IsVisible = false;
                    break;
                }
                
                Vehicle.GpsInfo.Subscribe(_ => FixType = _.FixType).DisposeItWith(Disposable);
                Vehicle.GpsInfo.Subscribe(_ => DopStatus = _.PdopStatus).DisposeItWith(Disposable);
                Vehicle.GpsInfo.Subscribe(_ => FixTypeText = SetFixTypeText(_.FixType)).DisposeItWith(Disposable);
                break;
            case GpsInfo.Gps2Info:
                if (Vehicle.Gps2Info.Value == null || Vehicle.Gps2Info.Value.FixType == GpsFixType.GpsFixTypeNoGps)
                {
                    IsVisible = false;
                    break;
                }

                Vehicle.Gps2Info.Subscribe(_ => FixType = _.FixType).DisposeItWith(Disposable);
                Vehicle.Gps2Info.Subscribe(_ => DopStatus = _.PdopStatus).DisposeItWith(Disposable);
                Vehicle.Gps2Info.Subscribe(_ => FixTypeText = SetFixTypeText(_.FixType)).DisposeItWith(Disposable);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(gpsInfo), gpsInfo, null);
        }
    }

    private static string SetFixTypeText(GpsFixType fixType)
    {
        return fixType switch
        {
            GpsFixType.GpsFixTypeNoGps => @"N\A",
            GpsFixType.GpsFixTypeNoFix => "No Fix",
            GpsFixType.GpsFixType2dFix => "2D",
            GpsFixType.GpsFixType3dFix => "3D",
            GpsFixType.GpsFixTypeDgps => "DGPS",
            GpsFixType.GpsFixTypeRtkFloat => "RTK Float",
            GpsFixType.GpsFixTypeRtkFixed => "RTK Fixed",
            GpsFixType.GpsFixTypeStatic => "N\\A",
            GpsFixType.GpsFixTypePpp => @"N\A"
        };
    }
    
    [Reactive]
    public GpsFixType FixType { get; set; }
    [Reactive]
    public DopStatusEnum DopStatus { get; set; }
    [Reactive]
    public string FixTypeText { get; set; }
}