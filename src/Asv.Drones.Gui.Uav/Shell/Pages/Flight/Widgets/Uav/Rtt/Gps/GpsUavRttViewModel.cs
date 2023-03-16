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

    public GpsUavRttViewModel(IVehicle vehicle, IRxValue<GpsInfo> gpsInfo) : base(vehicle, GenerateRtt(vehicle, "gps"))
    {
        if (gpsInfo.Value == null || gpsInfo.Value.FixType == GpsFixType.GpsFixTypeNoGps)
        {
            IsVisible = false;
        }
        else
        {
            gpsInfo.Subscribe(_ => FixType = _.FixType).DisposeItWith(Disposable);
            gpsInfo.Subscribe(_ => DopStatus = _.PdopStatus).DisposeItWith(Disposable);
            gpsInfo.Subscribe(_ => FixTypeText = SetFixTypeText(_.FixType)).DisposeItWith(Disposable);
            ToolTipText = gpsInfo.Value.FixType.GetShortDisplayName();
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
    [Reactive]
    public string ToolTipText { get; set; }
}