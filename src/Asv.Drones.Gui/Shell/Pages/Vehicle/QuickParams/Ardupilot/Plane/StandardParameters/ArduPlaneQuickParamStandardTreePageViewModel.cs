using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Asv.Common;
using Asv.Drones.Gui.Api;
using Asv.Mavlink;
using Asv.Mavlink.V2.Common;
using DynamicData;
using DynamicData.Binding;
using Material.Icons;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Asv.Drones.Gui;

public class ArduPlaneQuickParamStandardTreePageViewModel : TreePageViewModel
{
    private readonly ITreePageContext _context;
    private ReadOnlyObservableCollection<IMenuItem> _actions;
    private ReadOnlyObservableCollection<QuickParameterViewModel> _quickParams;
    private readonly SourceCache<IMenuItem, Uri> _actionsSource = new(item => item.Id);
    private readonly SourceCache<QuickParameterViewModel, Uri> _quickParamsSource = new(item => item.Id);
    private readonly IParamsClientEx? _paramsClientEx;

    public ArduPlaneQuickParamStandardTreePageViewModel(ITreePageContext context) : base(WellKnownUri
        .ShellPageQuickParamsArduPlaneVehicleStandard)
    {
        _context = context;

        if (context is VehicleQuickParamPageViewModel vehicleQuickParamPage)
        {
            _paramsClientEx = vehicleQuickParamPage.ParamsClientEx;
        }

        IsChanged = new RxValue<bool>().DisposeItWith(Disposable);

        #region Actions

        _actionsSource.Connect()
            .SortBy(param => param.Order)
            .Bind(out _actions)
            .Subscribe()
            .DisposeItWith(Disposable);

        Actions = _actions;

        _actionsSource.AddOrUpdate(new MenuItem($"{Id}.write-all-menu")
        {
            Order = 0,
            Header = RS.VehicleQuickParamsStandardTreePage_Write,
            Icon = MaterialIconKind.Pen,
            Command = ReactiveCommand.CreateFromTask(WriteAllParams, IsChanged)
        });
        _actionsSource.AddOrUpdate(new MenuItem($"{Id}.refresh-all-menu")
        {
            Order = 1,
            Header = RS.VehicleQuickParamsStandardTreePage_Refresh,
            Icon = MaterialIconKind.Refresh,
            Command = ReactiveCommand.CreateFromTask(RefreshAllParams)
        });

        #endregion

        #region Quick Params

        _quickParamsSource.Connect()
            .AutoRefresh(_ => _.IsChanged)
            .SortBy(param => param.Id.AbsoluteUri)
            .Bind(out _quickParams)
            .Subscribe(_ => { IsChanged.OnNext(_quickParams.Any(quickParam => quickParam.IsChanged)); })
            .DisposeItWith(Disposable);
        
        _quickParamsSource.AddOrUpdate(new QuickParametersSliderViewModel(
            $"{Id}.TKOFF_THR_MINSPD", _paramsClientEx, new ParamDescription
            {
                Name = "TKOFF_THR_MINSPD",
                DisplayName = "Takeoff throttle min speed",
                Description =
                    "Minimum GPS ground speed in m/s used by the speed check that un-suppresses throttle in auto-takeoff. This can be be used for catapult launches where you want the motor to engage only after the plane leaves the catapult, but it is preferable to use the TKOFF_THR_MINACC and TKOFF_THR_DELAY parameters for catapult launches due to the errors associated with GPS measurements. For hand launches with a",
                Units = "m/s",
                Min = 0,
                Max = 30,
                Increment = 1
            }));
        _quickParamsSource.AddOrUpdate(new QuickParametersSliderViewModel(
            $"{Id}.TKOFF_TDRAG_SPD1", _paramsClientEx, new ParamDescription
            {
                Name = "TKOFF_TDRAG_SPD1",
                DisplayName = "Takeoff tail dragger speed1",
                Description =
                    "This parameter sets the airspeed at which to stop holding the tail down and transition to rudder control of steering on the ground. When TKOFF_TDRAG_SPD1 is reached the pitch of the aircraft will be held level until TKOFF_ROTATE_SPD is reached, at which point the takeoff pitch specified in the mission will be used to \"rotate\" the pitch for takeoff climb. Set TKOFF_TDRAG_SPD1 to zero to",
                Units = "m/s",
                Min = 0,
                Max = 30,
                Increment = 1
            }));        
        _quickParamsSource.AddOrUpdate(new QuickParametersSliderViewModel(
            $"{Id}.TKOFF_TDRAG_ELEV", _paramsClientEx, new ParamDescription
            {
                Name = "TKOFF_TDRAG_ELEV",
                DisplayName = "Takeoff tail dragger elevator",
                Description =
                    "This parameter sets the amount of elevator to apply during the initial stage of a takeoff. It is used to hold the tail wheel of a taildragger on the ground during the initial takeoff stage to give maximum steering. This option should be combined with the TKOFF_TDRAG_SPD1 option and the GROUND_STEER_ALT option along with tuning of the ground steering controller. A value of zero means",
                Units = "%",
                Min = -100,
                Max = 100,
                Increment = 1
            }));
        _quickParamsSource.AddOrUpdate(new QuickParametersSliderViewModel(
            $"{Id}.TKOFF_ROTATE_SPD", _paramsClientEx, new ParamDescription
            {
                Name = "TKOFF_ROTATE_SPD",
                DisplayName = "Takeoff rotate speed",
                Description =
                    "This parameter sets the airspeed at which the aircraft will \"rotate\", setting climb pitch specified in the mission. If TKOFF_ROTATE_SPD is zero then the climb pitch will be used as soon as takeoff is started. For hand launch and catapult launches a TKOFF_ROTATE_SPD of zero should be set. For all ground launches TKOFF_ROTATE_SPD should be set above the stall speed, usually by about 10 to",
                Units = "m/s",
                Min = 0,
                Max = 30,
                Increment = 1
            }));
        _quickParamsSource.AddOrUpdate(new QuickParametersSliderViewModel(
            $"{Id}.TECS_LAND_THR", _paramsClientEx, new ParamDescription
            {
                Name = "TECS_LAND_THR",
                DisplayName = "Cruise throttle during landing approach",
                Description =
                    "Use this parameter instead of LAND_ARSPD if your platform does not have an airspeed sensor. It is the cruise throttle during landing approach. If this value is negative then it is disabled and TECS_LAND_ARSPD is used instead.",
                Units = "%",
                Min = -1,
                Max = 100,
                Increment = 1
            }));
        _quickParamsSource.AddOrUpdate(new QuickParametersSliderViewModel(
            $"{Id}.TECS_LAND_ARSPD", _paramsClientEx, new ParamDescription
            {
                Name = "TECS_LAND_ARSPD",
                DisplayName = "Airspeed during landing approach",
                Description =
                    "When performing an autonomus landing, this value is used as the goal airspeed during approach. Max airspeed allowed is Trim Airspeed or AIRSPEED_MAX as defined by LAND_OPTIONS bitmask. Note that this parameter is not useful if your platform does not have an airspeed sensor (use TECS_LAND_THR instead). If negative then this value is halfway between AIRSPEED_MIN and TRIM_CRUISE_CM speed for fixed wing autolandings.",
                Units = "m/s",
                Min = -1,
                Max = 127,
                Increment = 1
            }));
        _quickParamsSource.AddOrUpdate(new QuickParametersSliderViewModel(
            $"{Id}.TECS_LAND_ARSPD", _paramsClientEx, new ParamDescription
            {
                Name = "TECS_LAND_ARSPD",
                DisplayName = "Airspeed during landing approach",
                Description =
                    "When performing an autonomus landing, this value is used as the goal airspeed during approach. Max airspeed allowed is Trim Airspeed or AIRSPEED_MAX as defined by LAND_OPTIONS bitmask. Note that this parameter is not useful if your platform does not have an airspeed sensor (use TECS_LAND_THR instead). If negative then this value is halfway between AIRSPEED_MIN and TRIM_CRUISE_CM speed for fixed wing autolandings.",
                Units = "m/s",
                Min = -1,
                Max = 127,
                Increment = 1
            }));
        _quickParamsSource.AddOrUpdate(new QuickParametersSliderViewModel(
            $"{Id}.STEER2SRV_MINSPD", _paramsClientEx, new ParamDescription
            {
                Name = "STEER2SRV_MINSPD",
                DisplayName = "Minimum speed",
                Description =
                    "This is the minimum assumed ground speed in meters/second for steering. Having a minimum speed prevents oscillations when the vehicle first starts moving. The vehicle can still drive slower than this limit, but the steering calculations will be done based on this minimum speed.",
                Units = "m/s",
                Min = 0,
                Max = 5,
                Increment = 1
            }));
        _quickParamsSource.AddOrUpdate(new QuickParametersSliderViewModel(
            $"{Id}.Q_WVANE_SPD_MAX", _paramsClientEx, new ParamDescription
            {
                Name = "Q_WVANE_SPD_MAX",
                DisplayName = "Weathervaning max ground speed",
                Description =
                    "Below this ground speed weathervaning is permitted. Set to 0 to ignore this condition when checking if vehicle should weathervane.",
                Units = "m/s",
                Min = 0,
                Max = 50,
                Increment = 1
            }));
        _quickParamsSource.AddOrUpdate(new QuickParametersComboBoxViewModel(
            $"{Id}.ARSPD2_TYPE", _paramsClientEx, new ParamDescription
            {
                Name = "ARSPD2_TYPE",
                DisplayName = "Airspeed type",
                Description = "Type of airspeed sensor.",
                AvailableValues =
                {
                    new ParamDescriptionValue { Code = 0, Description = "None" },
                    new ParamDescriptionValue { Code = 1, Description = "I2C-MS4525D0" },
                    new ParamDescriptionValue { Code = 2, Description = "Analog" },
                    new ParamDescriptionValue { Code = 3, Description = "I2C-MS5525" },
                    new ParamDescriptionValue { Code = 4, Description = "I2C-MS5525 (0x76)" },
                    new ParamDescriptionValue { Code = 5, Description = "I2C-MS5525 (0x77)" },
                    new ParamDescriptionValue { Code = 6, Description = "I2C-SDP3X" },
                    new ParamDescriptionValue { Code = 7, Description = "I2C-DLVR-5in" },
                    new ParamDescriptionValue { Code = 8, Description = "DroneCAN" },
                    new ParamDescriptionValue { Code = 9, Description = "I2C-DLVR-10in" },
                    new ParamDescriptionValue { Code = 10, Description = "I2C-DLVR-20in" },
                    new ParamDescriptionValue { Code = 11, Description = "I2C-DLVR-30in" },
                    new ParamDescriptionValue { Code = 12, Description = "I2C-DLVR-60in" },
                    new ParamDescriptionValue { Code = 13, Description = "NMEA water speed" },
                    new ParamDescriptionValue { Code = 14, Description = "MSP" },
                    new ParamDescriptionValue { Code = 15, Description = "ASP5033" },
                    new ParamDescriptionValue { Code = 16, Description = "ExtemalAHRS" },
                    new ParamDescriptionValue { Code = 17, Description = "SITL" }
                }
            }));
        _quickParamsSource.AddOrUpdate(new QuickParametersComboBoxViewModel(
            $"{Id}.ARSPD_USE", _paramsClientEx, new ParamDescription
            {
                Name = "ARSPD_USE",
                DisplayName = "Airspeed use",
                Description =
                    "Enables airspeed use for automatic throttle modes and replaces control from THR_TRIM. Continues to display and log airspeed if set to 0. Uses airspeed for control if set to 1. Only uses airspeed when throttle = 0 if set to 2 (useful for gliders with airspeed sensors behind propellers).",
                AvailableValues =
                {
                    new ParamDescriptionValue { Code = 0, Description = "Do not use" },
                    new ParamDescriptionValue { Code = 1, Description = "Use" },
                    new ParamDescriptionValue { Code = 2, Description = "Use when zero throttle" }
                }
            }));
        _quickParamsSource.AddOrUpdate(new QuickParametersComboBoxViewModel(
            $"{Id}.ARSPD_TYPE", _paramsClientEx, new ParamDescription
            {
                Name = "ARSPD_TYPE",
                DisplayName = "Airspeed type",
                Description = "Type of airspeed sensor",
                AvailableValues =
                {
                    new ParamDescriptionValue { Code = 0, Description = "None" },
                    new ParamDescriptionValue { Code = 1, Description = "I2C-MS4525D0" },
                    new ParamDescriptionValue { Code = 2, Description = "Analog" },
                    new ParamDescriptionValue { Code = 3, Description = "I2C-MS5525" },
                    new ParamDescriptionValue { Code = 4, Description = "I2C-MS5525 (0x76)" },
                    new ParamDescriptionValue { Code = 5, Description = "I2C-MS5525 (0x77)" },
                    new ParamDescriptionValue { Code = 6, Description = "I2C-SDP3X" },
                    new ParamDescriptionValue { Code = 7, Description = "I2C-DLVR-5in" },
                    new ParamDescriptionValue { Code = 8, Description = "DroneCAN" },
                    new ParamDescriptionValue { Code = 9, Description = "I2C-DLVR-10in" },
                    new ParamDescriptionValue { Code = 10, Description = "I2C-DLVR-20in" },
                    new ParamDescriptionValue { Code = 11, Description = "I2C-DLVR-30in" },
                    new ParamDescriptionValue { Code = 12, Description = "I2C-DLVR-60in" },
                    new ParamDescriptionValue { Code = 13, Description = "NMEA water speed" },
                    new ParamDescriptionValue { Code = 14, Description = "MSP" },
                    new ParamDescriptionValue { Code = 15, Description = "ASP5033" },
                    new ParamDescriptionValue { Code = 16, Description = "ExternalAHRS" },
                    new ParamDescriptionValue { Code = 17, Description = "SITL" }
                }
            }));

        _quickParamsSource.AddOrUpdate(new QuickParametersSliderViewModel(
            $"{Id}.ARSPD_FBW_MIN", _paramsClientEx, new ParamDescription
            {
                Name = "ARSPD_FBW_MIN",
                DisplayName = "Minimum airspeed",
                Description =
                    "Minimum airspeed demanded in automatic throttle modes. Should be set to 20% higher than level flight stall speed.",
                Units = "m/s",
                Min = 5,
                Max = 100,
                Increment = 1
            }));

        _quickParamsSource.AddOrUpdate(new QuickParametersSliderViewModel(
            $"{Id}.ARSPD_FBW_MAX", _paramsClientEx, new ParamDescription
            {
                Name = "ARSPD_FBW_MAX",
                DisplayName = "Maximum airspeed",
                Description =
                    "Maximum airspeed demanded in automatic throttle modes. Should be set slightly less than level flight speed at THR_MAX and also at least 50% above ARSPD_FBW_MIN to allow for accurate TECS altitude control.",
                Units = "m/s",
                Min = 5,
                Max = 100,
                Increment = 1
            }));
        _quickParamsSource.AddOrUpdate(new QuickParametersBitmaskViewModel($"{Id}.ARMING_CHECK", _paramsClientEx,
            new ParamDescription
            {
                Name = "ARMING_CHECK",
                DisplayName = "Arm checks to perform",
                Description =
                    "Checks prior to arming motor. This is a bitmask of checks that will be performed before allowing arming. For most users it is recommended to leave this at the default of 1 (all checks enabled). You can select whatever checks you prefer by adding together the values of each check type to set this parameter. For example, to only allow arming when you have GPS lock and no RC failsafe you would set ARMING_CHECK to 72.",
                AvailableValues =
                {
                    new ParamDescriptionValue { Code = 0, Description = "All" },
                    new ParamDescriptionValue { Code = 0, Description = "Barometer" },
                    new ParamDescriptionValue { Code = 0, Description = "Compass" },
                    new ParamDescriptionValue { Code = 0, Description = "GPS lock" },
                    new ParamDescriptionValue { Code = 0, Description = "INS" },
                    new ParamDescriptionValue { Code = 0, Description = "Parameters" },
                    new ParamDescriptionValue { Code = 0, Description = "RC channels" },
                    new ParamDescriptionValue { Code = 0, Description = "Board voltage" },
                    new ParamDescriptionValue { Code = 0, Description = "Battery level" },
                    new ParamDescriptionValue { Code = 0, Description = "Airspeed" },
                    new ParamDescriptionValue { Code = 0, Description = "Logging available" },
                    new ParamDescriptionValue { Code = 0, Description = "Hardware safety switch" },
                    new ParamDescriptionValue { Code = 0, Description = "GPS configuration" },
                    new ParamDescriptionValue { Code = 0, Description = "System" },
                    new ParamDescriptionValue { Code = 0, Description = "Mission" },
                    new ParamDescriptionValue { Code = 0, Description = "Rangefinder" },
                    new ParamDescriptionValue { Code = 0, Description = "Camera" },
                    new ParamDescriptionValue { Code = 0, Description = "AuxAuth" },
                    new ParamDescriptionValue { Code = 0, Description = "FFT" }
                }
            }));
        _quickParamsSource.AddOrUpdate(new QuickParametersSliderViewModel($"{Id}.NAVL1_PERIOD", _paramsClientEx,
            new ParamDescription
            {
                Name = "NAVL1_PERIOD",
                DisplayName = "L1 Control period",
                Description =
                    "Period in seconds of L1 tracking loop. This parameter is the primary control for agressiveness of turns in auto mode. This needs to be larger for less responsive airframes. The default of 20 is quite conservative, but for most RC aircraft will lead to reasonable flight. For smaller more agile aircraft a value closer to 15 is appropriate, or even as low as 10 for",
                Units = "s",
                Min = 1,
                Max = 60,
                Increment = 1
            }));
        _quickParamsSource.AddOrUpdate(new QuickParametersSliderViewModel($"{Id}.WP_RADIUS", _paramsClientEx,
            new ParamDescription
            {
                Name = "WP_RADIUS",
                DisplayName = "Waypoint radius",
                Description =
                    "Defines the maximum distance from a waypoint that when crossed indicates the waypoint may be complete. To avoid the aircraft looping around the waypoint in case it misses by more than the WP_RADIUS an additional check is made to see if the aircraft has crossed a \"finish line\" passing through the waypoint and perpendicular to the flight path from the previous waypoint. If that finish",
                Units = "m",
                Min = 1,
                Max = 32767,
                Increment = 1
            }));

        #endregion

        Observable.Timer(TimeSpan.Zero)
            .ObserveOn(RxApp.MainThreadScheduler)
            .Subscribe(RefreshAllParamsOnNext)
            .DisposeItWith(Disposable);
    }

    private async void RefreshAllParamsOnNext(long _)
    {
        await RefreshAllParams();
    }

    private async Task WriteAllParams(CancellationToken cancellationToken = default)
    {
        foreach (var param in _quickParams)
        {
            if (param.IsChanged) await param.WriteParam(cancellationToken);
            if (cancellationToken.IsCancellationRequested) return;
        }
    }

    private async Task RefreshAllParams(CancellationToken cancellationToken = default)
    {
        foreach (var param in _quickParams)
        {
            await param.ReadParam(cancellationToken);
            if (cancellationToken.IsCancellationRequested) return;
        }
    }

    public ReadOnlyObservableCollection<QuickParameterViewModel> QuickParams => _quickParams;

    public RxValue<bool> IsChanged { get; }
}