using System.Reactive.Linq;
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
        FixTypeText = "RTK Fixed";
    }

    public GpsUavRttViewModel(IVehicle vehicle, IRxValue<GpsInfo> gpsInfo) : base(vehicle, GenerateRtt(vehicle, "gps"))
    {
        IsVisible = false;
        gpsInfo.Subscribe(_ => IsVisible = _ != null && _.FixType != GpsFixType.GpsFixTypeNoGps);
        var validDataPipe = gpsInfo
            .Where(_ => IsVisible && _ != null && _.FixType != GpsFixType.GpsFixTypeNoGps);
        
        validDataPipe.Select(_=>_.FixType).DistinctUntilChanged().Subscribe(_ =>
        {
            FixType = _;
            FixTypeText = SetFixTypeText(_);
        }).DisposeItWith(Disposable);

        validDataPipe.Select(_ => Math.Round(_.Pdop.Value, 1)).DistinctUntilChanged()
            .Subscribe(_ =>TopStatusText = _.ToString("F1") )
            .DisposeItWith(Disposable);
        
        validDataPipe.Select(_ => _.SatellitesVisible).DistinctUntilChanged()
            .Subscribe(_ =>BottomStatusText = _.ToString() )
            .DisposeItWith(Disposable);
        
        validDataPipe.Select(_=>_.PdopStatus).DistinctUntilChanged().Subscribe(_ =>
        {
            ToolTipText = _.GetDescription();
            DopStatus = _;
        }).DisposeItWith(Disposable);
        
        
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
    public string FixTypeText { get; set; } = RS.UavRttItem_ValueNotAvailable;
    [Reactive]
    public string ToolTipText { get; set; }
    [Reactive]
    public string TopStatusText { get;set; }
    [Reactive]
    public string BottomStatusText { get;set; }
}