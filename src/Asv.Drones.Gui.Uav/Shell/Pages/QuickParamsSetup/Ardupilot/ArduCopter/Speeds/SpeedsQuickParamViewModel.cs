using System.ComponentModel.Composition;
using Asv.Common;
using Asv.Drones.Gui.Core;
using Asv.Mavlink;
using DynamicData.Binding;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using ReactiveUI.Validation.Extensions;

namespace Asv.Drones.Gui.Uav;


[Export(typeof(IQuickParamsPart))]
[PartCreationPolicy(CreationPolicy.NonShared)]
public class SpeedsQuickParamViewModel : QuickParamsPartBase
{
    private static readonly Uri Uri = new(QuickParamsPartBase.Uri, "velocity");
    private readonly IVehicleClient _vehicle;
    private readonly ILocalizationService _loc;
    private readonly ILogService _log;
    public override int Order => 1;

    public override bool IsRebootRequired => false;

    public override bool IsVisibleInAdvancedMode => false;
    
    [ImportingConstructor]
    public SpeedsQuickParamViewModel(IVehicleClient vehicle, ILocalizationService loc, ILogService log) : base(Uri)
    {
        _vehicle = vehicle;
        _loc = loc;
        _log = log;
        
        this.WhenAnyValue(_ => _.WpNavSpeed)
            .Subscribe(_ =>
            {
                WpNavSpeedDescription = string.Format(RS.SpeedsQuickParamView_WPNAV_SPEED_Description,
                    _ * 100f,
                    _loc.Velocity.AvailableUnits.First(__ => __.Id == VelocityUnits.KilometersPerHour).ConvertFromSi(_),
                    _loc.Velocity.AvailableUnits.First(__ => __.Id == VelocityUnits.MilesPerHour).ConvertFromSi(_)
                );
                
                SyncCheck();
            })
            .DisposeItWith(Disposable);
        
        this.WhenAnyValue(_ => _.WpNavSpeedDn)
            .Subscribe(_ =>
            {
                WpNavSpeedDnDescription = string.Format(RS.SpeedsQuickParamView_WPNAV_SPEED_DN_Description,
                    _ * 100f,
                    _loc.Velocity.AvailableUnits.First(__ => __.Id == VelocityUnits.KilometersPerHour).ConvertFromSi(_),
                    _loc.Velocity.AvailableUnits.First(__ => __.Id == VelocityUnits.MilesPerHour).ConvertFromSi(_)
                );
                
                SyncCheck();
            })
            .DisposeItWith(Disposable);
        
        this.WhenAnyValue(_ => _.WpNavSpeedUp)
            .Subscribe(_ =>
            {
                WpNavSpeedUpDescription = string.Format(RS.SpeedsQuickParamView_WPNAV_SPEED_UP_Description,
                    _ * 100f,
                    _loc.Velocity.AvailableUnits.First(__ => __.Id == VelocityUnits.KilometersPerHour).ConvertFromSi(_),
                    _loc.Velocity.AvailableUnits.First(__ => __.Id == VelocityUnits.MilesPerHour).ConvertFromSi(_)
                );
                
                SyncCheck();
            })
            .DisposeItWith(Disposable);
        
        this.WhenAnyValue(_ => _.RtlSpeed)
            .Subscribe(_ =>
            {
                RtlSpeedDescription = string.Format(RS.SpeedsQuickParamView_RTL_SPEED_Description,
                    _ * 100f,
                    _loc.Velocity.AvailableUnits.First(__ => __.Id == VelocityUnits.KilometersPerHour).ConvertFromSi(_),
                    _loc.Velocity.AvailableUnits.First(__ => __.Id == VelocityUnits.MilesPerHour).ConvertFromSi(_)
                );
                
                SyncCheck();
            })
            .DisposeItWith(Disposable);
        
        this.WhenAnyValue(_ => _.LoitSpeed)
            .Subscribe(_ =>
            {
                LoitSpeedDescription = string.Format(RS.SpeedsQuickParamView_LOIT_SPEED_Description,
                    _ * 100f,
                    _loc.Velocity.AvailableUnits.First(__ => __.Id == VelocityUnits.KilometersPerHour).ConvertFromSi(_),
                    _loc.Velocity.AvailableUnits.First(__ => __.Id == VelocityUnits.MilesPerHour).ConvertFromSi(_)
                );
                
                SyncCheck();
            })
            .DisposeItWith(Disposable);
        
        this.WhenAnyValue(_ => _.LandSpeed)
            .Subscribe(_ =>
            {
                LandSpeedDescription = string.Format(RS.SpeedsQuickParamView_LAND_SPEED_Description,
                    _ * 100f,
                    _loc.Velocity.AvailableUnits.First(__ => __.Id == VelocityUnits.KilometersPerHour).ConvertFromSi(_),
                    _loc.Velocity.AvailableUnits.First(__ => __.Id == VelocityUnits.MilesPerHour).ConvertFromSi(_)
                );
                
                SyncCheck();
            })
            .DisposeItWith(Disposable);
        
        this.WhenAnyValue(_ => _.LandSpeedHigh)
            .Subscribe(_ =>
            {
                LandSpeedHighDescription = string.Format(RS.SpeedsQuickParamView_LAND_SPEED_HIGH_Description,
                    _ * 100f,
                    _loc.Velocity.AvailableUnits.First(__ => __.Id == VelocityUnits.KilometersPerHour).ConvertFromSi(_),
                    _loc.Velocity.AvailableUnits.First(__ => __.Id == VelocityUnits.MilesPerHour).ConvertFromSi(_)
                );
                
                SyncCheck();
            })
            .DisposeItWith(Disposable);
        
        this.WhenAnyValue(_ => _.LandAltLow)
            .Subscribe(_ =>
            {
                LandAltLowDescription = string.Format(RS.SpeedsQuickParamView_LAND_ALT_LOW_Description,
                    _ * 100f,
                    _loc.Altitude.AvailableUnits.First(__ => __.Id == AltitudeUnits.Feets).ConvertFromSi(_)
                );
                
                SyncCheck();
            })
            .DisposeItWith(Disposable);
        
        this.WhenAnyValue(_ => _.PilotSpeedDn)
            .Subscribe(_ =>
            {
                PilotSpeedDnDescription = string.Format(RS.SpeedsQuickParamView_PILOT_SPEED_DN_Description,
                    _ * 100f,
                    _loc.Velocity.AvailableUnits.First(__ => __.Id == VelocityUnits.KilometersPerHour).ConvertFromSi(_),
                    _loc.Velocity.AvailableUnits.First(__ => __.Id == VelocityUnits.MilesPerHour).ConvertFromSi(_)
                );
                
                SyncCheck();
            })
            .DisposeItWith(Disposable);
        
        this.WhenAnyValue(_ => _.PilotSpeedUp)
            .Subscribe(_ =>
            {
                PilotSpeedUpDescription = string.Format(RS.SpeedsQuickParamView_PILOT_SPEED_UP_Description,
                    _ * 100f,
                    _loc.Velocity.AvailableUnits.First(__ => __.Id == VelocityUnits.KilometersPerHour).ConvertFromSi(_),
                    _loc.Velocity.AvailableUnits.First(__ => __.Id == VelocityUnits.MilesPerHour).ConvertFromSi(_)
                );
                
                SyncCheck();
            })
            .DisposeItWith(Disposable);
    }
    
    [Reactive]
    public float WpNavSpeed { get; set; }

    [Reactive]
    public string WpNavSpeedDescription { get; set; }
    
    [Reactive]
    public float WpNavSpeedDn { get; set; }
    
    [Reactive]
    public string WpNavSpeedDnDescription { get; set; }

    [Reactive]
    public float WpNavSpeedUp { get; set; }
    
    [Reactive]
    public string WpNavSpeedUpDescription { get; set; }
    
    [Reactive]
    public float RtlSpeed { get; set; }

    [Reactive]
    public string RtlSpeedDescription { get; set; }
    
    [Reactive]
    public float LoitSpeed { get; set; }

    [Reactive]
    public string LoitSpeedDescription { get; set; }
    
    [Reactive]
    public float LandSpeed { get; set; }

    [Reactive]
    public string LandSpeedDescription { get; set; }
    
    [Reactive]
    public float LandSpeedHigh { get; set; }
    
    [Reactive]
    public string LandSpeedHighDescription { get; set; }
    
    [Reactive]
    public float LandAltLow { get; set; }

    [Reactive]
    public string LandAltLowDescription { get; set; }
    
    [Reactive]
    public float PilotSpeedDn { get; set; }

    [Reactive]
    public string PilotSpeedDnDescription { get; set; }
    
    [Reactive]
    public float PilotSpeedUp { get; set; }
    
    [Reactive]
    public string PilotSpeedUpDescription { get; set; }
    
    public override async Task Write()
    {
        try
        {
            WpNavSpeed = await _vehicle.Params.WriteOnce("WPNAV_SPEED", WpNavSpeed * 100) / 100f;
            WpNavSpeedDn = await _vehicle.Params.WriteOnce("WPNAV_SPEED_DN", WpNavSpeedDn * 100) / 100f;
            WpNavSpeedUp = await _vehicle.Params.WriteOnce("WPNAV_SPEED_UP", WpNavSpeedUp * 100) / 100f;
            RtlSpeed = await _vehicle.Params.WriteOnce("RTL_SPEED", (int)(RtlSpeed * 100)) / 100;
            LoitSpeed = await _vehicle.Params.WriteOnce("LOIT_SPEED", LoitSpeed * 100) / 100f;
            LandSpeed = await _vehicle.Params.WriteOnce("LAND_SPEED", (int)(LandSpeed * 100)) / 100;
            LandSpeedHigh = await _vehicle.Params.WriteOnce("LAND_SPEED_HIGH", (int)(LandSpeedHigh * 100)) / 100;
            LandAltLow = await _vehicle.Params.WriteOnce("LAND_ALT_LOW", (int)(LandAltLow * 100)) / 100;
            PilotSpeedDn = await _vehicle.Params.WriteOnce("PILOT_SPEED_DN", (int)(PilotSpeedDn * 100)) / 100;
            PilotSpeedUp = await _vehicle.Params.WriteOnce("PILOT_SPEED_UP", (int)(PilotSpeedUp * 100)) / 100;
        }
        catch (Exception e)
        {
            _log.Error("SpeedsQuickParam", e.Message, e);   
        }
    }

    public async Task SyncCheck()
    {
        try
        {
            IsSynced = WpNavSpeed.Equals(await _vehicle.Params.ReadOnce("WPNAV_SPEED") / 100f) &
                       WpNavSpeedDn.Equals(await _vehicle.Params.ReadOnce("WPNAV_SPEED_DN") / 100f) &
                       WpNavSpeedUp.Equals(await _vehicle.Params.ReadOnce("WPNAV_SPEED_UP") / 100f) &
                       RtlSpeed.Equals(Convert.ToSingle((int) await _vehicle.Params.ReadOnce("RTL_SPEED")) / 100) &
                       LoitSpeed.Equals(await _vehicle.Params.ReadOnce("LOIT_SPEED") / 100f) &
                       LandSpeed.Equals(Convert.ToSingle((int)await _vehicle.Params.ReadOnce("LAND_SPEED")) / 100) &
                       LandSpeedHigh.Equals(Convert.ToSingle((int) await _vehicle.Params.ReadOnce("LAND_SPEED_HIGH")) / 100) &
                       LandAltLow.Equals(Convert.ToSingle((int) await _vehicle.Params.ReadOnce("LAND_ALT_LOW")) / 100) &
                       PilotSpeedDn.Equals(Convert.ToSingle((int) await _vehicle.Params.ReadOnce("PILOT_SPEED_DN")) / 100) &
                       PilotSpeedUp.Equals(Convert.ToSingle((int) await _vehicle.Params.ReadOnce("PILOT_SPEED_UP")) / 100);
        }
        catch (Exception e)
        {
            _log.Error("SpeedsQuickParam", e.Message, e);
        }
    }

    public override async Task Read()
    {
        try
        {
            WpNavSpeed = await _vehicle.Params.ReadOnce("WPNAV_SPEED") / 100f;
            WpNavSpeedDn = await _vehicle.Params.ReadOnce("WPNAV_SPEED_DN") / 100f;
            WpNavSpeedUp = await _vehicle.Params.ReadOnce("WPNAV_SPEED_UP") / 100f;
            RtlSpeed = Convert.ToSingle((int) await _vehicle.Params.ReadOnce("RTL_SPEED")) / 100;
            LoitSpeed = await _vehicle.Params.ReadOnce("LOIT_SPEED") / 100f;
            LandSpeed = Convert.ToSingle((int)await _vehicle.Params.ReadOnce("LAND_SPEED")) / 100;
            LandSpeedHigh = Convert.ToSingle((int) await _vehicle.Params.ReadOnce("LAND_SPEED_HIGH")) / 100;
            LandAltLow = Convert.ToSingle((int) await _vehicle.Params.ReadOnce("LAND_ALT_LOW")) / 100;
            PilotSpeedDn = Convert.ToSingle((int) await _vehicle.Params.ReadOnce("PILOT_SPEED_DN")) / 100;
            PilotSpeedUp = Convert.ToSingle((int) await _vehicle.Params.ReadOnce("PILOT_SPEED_UP")) / 100;
        }
        catch (Exception e)
        {
            _log.Error("SpeedsQuickParam", e.Message, e);
        }
    }
}