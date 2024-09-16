using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Asv.Common;
using Asv.Drones.Gui.Api;
using Asv.Mavlink;
using DynamicData;
using Material.Icons;
using ReactiveUI;

namespace Asv.Drones.Gui;

public class ArduCopterQuickParamStandardTreePageViewModel : TreePageViewModel
{
    private readonly ITreePageContext _context;
    private ReadOnlyObservableCollection<IMenuItem> _actions;
    private ReadOnlyObservableCollection<QuickParameterViewModel> _quickParams;
    private readonly SourceCache<IMenuItem, Uri> _actionsSource = new(item => item.Id);
    private readonly SourceCache<QuickParameterViewModel, Uri> _quickParamsSource = new(item => item.Id);
    private readonly IParamsClientEx? _paramsClientEx;

    public ArduCopterQuickParamStandardTreePageViewModel(ITreePageContext context) : base(WellKnownUri
        .ShellPageQuickParamsArduCopterVehicleStandard)
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
            .Subscribe(_ =>
            {
                IsChanged.OnNext(_quickParams.Any(quickParam => quickParam.IsChanged));
            })
            .DisposeItWith(Disposable);
          
        _quickParamsSource.AddOrUpdate(new QuickParametersSliderViewModel(
            $"{Id}.WPNAV_SPEED_UP", _paramsClientEx, new ParamDescription
            {
                Name = "WPNAV_SPEED_UP",
                DisplayName = "Waypoint Climb Speed Target",
                Description = "Defines the speed in cm/s which the aircraft will attempt to maintain while climbing during a WP mission.",
                Units = "cm/s",
                Min = 10,
                Max = 1000,
                Increment = 1
            }));            
        _quickParamsSource.AddOrUpdate(new QuickParametersSliderViewModel(
            $"{Id}.WPNAV_SPEED_DN", _paramsClientEx, new ParamDescription
            {
                Name = "WPNAV_SPEED_DN",
                DisplayName = "Waypoint Descent Speed Target",
                Description = "Defines the speed in cm/s which the aircraft will attempt to maintain while descending during a WP mission.",
                Units = "cm/s",
                Min = 10,
                Max = 500,
                Increment = 1
            }));
        _quickParamsSource.AddOrUpdate(new QuickParametersSliderViewModel(
            $"{Id}.WPNAV_SPEED", _paramsClientEx, new ParamDescription
            {
                Name = "WPNAV_SPEED",
                DisplayName = "Waypoint Horizontal Speed Target",
                Description = "Defines the speed in cm/s which the aircraft will attempt to maintain horizontally during a WP mission.",
                Units = "cm/s",
                Min = 20,
                Max = 2000,
                Increment = 1
            }));
        _quickParamsSource.AddOrUpdate(new QuickParametersSliderViewModel(
            $"{Id}.WPNAV_SPEED", _paramsClientEx, new ParamDescription
            {
                Name = "WPNAV_SPEED",
                DisplayName = "Waypoint Horizontal Speed Target",
                Description = "Defines the speed in cm/s which the aircraft will attempt to maintain horizontally during a WP mission.",
                Units = "cm/s",
                Min = 20,
                Max = 2000,
                Increment = 1
            }));
        _quickParamsSource.AddOrUpdate(new QuickParametersComboBoxViewModel(
            $"{Id}.THROW_MOT_START", _paramsClientEx, new ParamDescription
            {
                Name = "THROW_MOT_START",
                DisplayName = "Start motors before throwing is detected",
                Description = "Used by Throw mode. Controls whether motors will run at the speed set by MOT_SPIN_MIN or will be stopped when armed and waiting for the throw.",
                AvailableValues =
                {
                    new ParamDescriptionValue { Code = 0, Description = "Stopped" },
                    new ParamDescriptionValue { Code = 1, Description = "Running" },
                }
            }));
        _quickParamsSource.AddOrUpdate(new QuickParametersSliderViewModel(
            $"{Id}.RTL_SPEED", _paramsClientEx, new ParamDescription
            {
                Name = "RTL_SPEED",
                DisplayName = "RTL speed",
                Description = "Defines the speed in cm/s which the aircraft will attempt to maintain horizontally while flying home. If this is set to zero, WPNAV_SPEED will be used instead.",
                Units = "cm/s",
                Min = 0,
                Max = 2000,
                Increment = 1
            }));
        _quickParamsSource.AddOrUpdate(new QuickParametersSliderViewModel(
            $"{Id}.RTL_SPEED", _paramsClientEx, new ParamDescription
            {
                Name = "RTL_SPEED",
                DisplayName = "RTL speed",
                Description = "Defines the speed in cm/s which the aircraft will attempt to maintain horizontally while flying home. If this is set to zero, WPNAV_SPEED will be used instead.",
                Units = "cm/s",
                Min = 0,
                Max = 2000,
                Increment = 1
            }));
        _quickParamsSource.AddOrUpdate(new QuickParametersSliderViewModel(
            $"{Id}.PILOT_SPEED_UP", _paramsClientEx, new ParamDescription
            {
                Name = "PILOT_SPEED_UP",
                DisplayName = "Pilot maximum vertical speed ascending",
                Description = "The maximum vertical ascending velocity the pilot may request in cm/s.",
                Units = "cm/s",
                Min = 50,
                Max = 500,
                Increment = 1
            }));
        _quickParamsSource.AddOrUpdate(new QuickParametersSliderViewModel(
            $"{Id}.PILOT_SPEED_DN", _paramsClientEx, new ParamDescription
            {
                Name = "PILOT_SPEED_DN",
                DisplayName = "Pilot maximum vertical speed descending",
                Description = "The maximum vertical descending velocity the pilot may request in cm/s. If 0 PILOT_SPEED_UP value is used.",
                Units = "cm/s",
                Min = 0,
                Max = 500,
                Increment = 1
            }));
        _quickParamsSource.AddOrUpdate(new QuickParametersSliderViewModel(
            $"{Id}.LOIT_SPEED", _paramsClientEx, new ParamDescription
            {
                Name = "LOIT_SPEED",
                DisplayName = "Loiter Horizontal Maximum Speed",
                Description = "Defines the maximum speed in cm/s which the aircraft will travel horizontally while in loiter mode.",
                Units = "cm/s",
                Min = 20,
                Max = 3500,
                Increment = 1
            }));
        _quickParamsSource.AddOrUpdate(new QuickParametersSliderViewModel(
            $"{Id}.LAND_SPEED_HIGH", _paramsClientEx, new ParamDescription
            {
                Name = "LAND_SPEED_HIGH",
                DisplayName = "Land speed high",
                Description = "The descent speed for the first stage of landing in cm/s. If this is zero then WPNAV_SPEED_DN is used.",
                Units = "cm/s",
                Min = 0,
                Max = 500,
                Increment = 1
            }));
        _quickParamsSource.AddOrUpdate(new QuickParametersSliderViewModel(
            $"{Id}.LAND_SPEED_HIGH", _paramsClientEx, new ParamDescription
            {
                Name = "LAND_SPEED_HIGH",
                DisplayName = "Land speed high",
                Description = "The descent speed for the first stage of landing in cm/s. If this is zero then WPNAV_SPEED_DN is used.",
                Units = "cm/s",
                Min = 0,
                Max = 500,
                Increment = 1
            })); 
        _quickParamsSource.AddOrUpdate(new QuickParametersSliderViewModel(
            $"{Id}.LAND_SPEED", _paramsClientEx, new ParamDescription
            {
                Name = "LAND_SPEED",
                DisplayName = "Land speed",
                Description = "The descent speed for the final stage of landing in cm/s.",
                Units = "cm/s",
                Min = 30,
                Max = 200,
                Increment = 1
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
        _quickParamsSource.AddOrUpdate(new QuickParametersBitmaskViewModel($"{Id}.ARMING_CHECK", _paramsClientEx, 
            new ParamDescription
            {
                Name = "ARMING_CHECK",
                DisplayName = "Arm checks to perform",
                Description = "Checks prior to arming motor. This is a bitmask of checks that will be performed before allowing arming. For most users it is recommended to leave this at the default of 1 (all checks enabled). You can select whatever checks you prefer by adding together the values of each check type to set this parameter. For example, to only allow arming when you have GPS lock and no RC failsafe you would set ARMING_CHECK to 72.",
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
                    new ParamDescriptionValue { Code = 0, Description = "Logging available" },
                    new ParamDescriptionValue { Code = 0, Description = "Hardware safety switch" },
                    new ParamDescriptionValue { Code = 0, Description = "GPS configuration" },
                    new ParamDescriptionValue { Code = 0, Description = "System" },
                    new ParamDescriptionValue { Code = 0, Description = "Mission" },
                    new ParamDescriptionValue { Code = 0, Description = "Rangefinder" },
                    new ParamDescriptionValue { Code = 0, Description = "Camera" },
                    new ParamDescriptionValue { Code = 0, Description = "AuxAuth" },
                    new ParamDescriptionValue { Code = 0, Description = "VisualOdometry" },
                    new ParamDescriptionValue { Code = 0, Description = "FFT" }
                }
            }));
        _quickParamsSource.AddOrUpdate(new QuickParametersSliderViewModel(
            $"{Id}.WPNAV_RADIUS", _paramsClientEx, new ParamDescription
            {
                Name = "WPNAV_RADIUS",
                DisplayName = "Waypoint radius",
                Description = "Defines the distance from a waypoint, that when crossed indicates the wp has been hit.",
                Units = "cm",
                Min = 5,
                Max = 1000,
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