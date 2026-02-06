using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Asv.Avalonia;
using Asv.Avalonia.GeoMap;
using Asv.Avalonia.IO;
using Asv.Common;
using Asv.Drones.Api;
using Asv.IO;
using Asv.Mavlink;
using Asv.Mavlink.Ardupilotmega;
using Asv.Mavlink.Minimal;
using Avalonia.Media;
using Avalonia.Threading;
using Material.Icons;
using Microsoft.Extensions.Logging;
using R3;
using Observable = R3.Observable;

namespace Asv.Drones;

public class UavWidgetViewModel : MapWidget, IUavFlightWidget
{
    private const string WidgetId = "widget-uav";
    private const AsvColorKind DefaultStatusColor = AsvColorKind.Info5;

    private readonly IUnit _velocityUnit;
    private readonly IUnit _altitudeUnit;
    private readonly IUnit _angleUnit;
    private readonly IUnit _capacityUnit;
    private readonly IUnit _amperageUnit;
    private readonly IUnit _voltageUnit;
    private readonly IUnit _progressUnit;

    private readonly IGnssClientEx? _gnssClient;
    private const int CriticalAltitude = 40;
    private const int DangerHighSpeed = 10;
    private const int DangerSatelliteCount = 10;
    private static readonly Range WarningSatelliteAmount = 15..20;

#pragma warning disable SA1313
    private record BatteryRttBoxData(
        double Charge,
        double Amperage,
        double Voltage,
        double Consumed,
        IUnitItem ProgressUnit,
        IUnitItem AmperageUnit,
        IUnitItem CapacityUnit,
        IUnitItem VoltageUnit
    );

    private record AltitudeRttBoxData(
        double AltitudeAgl,
        double AltitudeMsl,
        IUnitItem AltitudeUnit
    );

    private record GnssRttBoxData(
        int Sattelites,
        double HdopCount,
        double VdopCount,
        Mavlink.Common.GpsFixType Mode
    );
#pragma warning restore SA1313

    public UavWidgetViewModel()
        : base(WidgetId, DesignTime.LoggerFactory)
    {
        DesignTime.ThrowIfNotDesignMode();
        TakeOff = new ReactiveCommand();
        AutoMode = new ReactiveCommand();
        Rtl = new ReactiveCommand();
        Guided = new ReactiveCommand();
        StartMission = new ReactiveCommand();
        Land = new ReactiveCommand();
        InitArgs("1");
        MissionProgress = new MissionProgressViewModel().DisposeItWith(Disposable);

        NullUnitService.Instance.Extend(
            new VelocityBase(
                DesignTime.Configuration,
                [new MetersPerSecondVelocityUnit(), new MilesPerHourVelocityUnit()]
            )
        );
        NullUnitService.Instance.Extend(
            new ProgressBase(
                DesignTime.Configuration,
                [new PercentProgressUnit(), new InPartsProgressUnit()]
            )
        );

        var unitService = NullUnitService.Instance;

        Icon = MaterialIconKind.AccountFile;
        IconColor = AsvColorKind.Info4;
        var unitItem = unitService.Units.Values.First();
        _altitudeUnit = unitItem;
        _velocityUnit = unitItem;
        _angleUnit = unitItem;
        _capacityUnit = unitItem;
        _amperageUnit = unitItem;
        _voltageUnit = unitItem;
        _progressUnit = unitItem;

        var linkQuality = new ReactiveProperty<double>(100).DisposeItWith(Disposable);
        var altitudeAgl = new ReactiveProperty<double>(10).DisposeItWith(Disposable);
        var altitudeMsl = new ReactiveProperty<double>(14).DisposeItWith(Disposable);
        var heading = new ReactiveProperty<double>(29).DisposeItWith(Disposable);
        var azimuth = new ReactiveProperty<double>(39).DisposeItWith(Disposable);
        var homeAzimuth = new ReactiveProperty<double>(30).DisposeItWith(Disposable);
        var satelliteCount = new ReactiveProperty<int>(10).DisposeItWith(Disposable);
        var hdopCount = new ReactiveProperty<double>(2).DisposeItWith(Disposable);
        var vdopCount = new ReactiveProperty<double>(4).DisposeItWith(Disposable);
        var gpsFixType = new ReactiveProperty<GpsFixType>(GpsFixType.GpsFixTypeDgps).DisposeItWith(
            Disposable
        );
        var velocity = new ReactiveProperty<double>(199).DisposeItWith(Disposable);
        var batteryAmperage = new ReactiveProperty<double>(39).DisposeItWith(Disposable);
        var batteryVoltage = new ReactiveProperty<double>(34).DisposeItWith(Disposable);
        var batteryCharge = new ReactiveProperty<double>(123).DisposeItWith(Disposable);
        var batteryConsumed = new ReactiveProperty<double>(39).DisposeItWith(Disposable);
        var currentFlightMode = new ReactiveProperty<string>("Unknown").DisposeItWith(Disposable);

        CurrentFlightModeRttBox = new SingleRttBoxViewModel<string>(
            nameof(CurrentFlightModeRttBox),
            DesignTime.LoggerFactory,
            currentFlightMode,
            null
        )
        {
            Header = RS.UavRttItem_Mode,
            Icon = MaterialIconKind.FlightMode,
            UpdateAction = (model, mode) => model.ValueString = mode,
        }
            .SetRoutableParent(this)
            .DisposeItWith(Disposable);

        AltitudeRttBox = new TwoColumnRttBoxViewModel<Unit>(
            nameof(AltitudeRttBox),
            DesignTime.LoggerFactory,
            Observable
                .Merge(
                    altitudeAgl.Select(_ => Unit.Default),
                    altitudeMsl.Select(_ => Unit.Default),
                    _altitudeUnit.CurrentUnitItem.Select(_ => Unit.Default)
                )
                .ObserveOnUIThreadDispatcher()
                .ThrottleLast(TimeSpan.FromMilliseconds(200)),
            null
        )
        {
            Header = RS.UavRttItem_Altitude,
            Icon = MaterialIconKind.Altimeter,
            UpdateAction = (model, changes) =>
            {
                model.Left.ValueString = _altitudeUnit.CurrentUnitItem.Value.PrintFromSi(
                    altitudeAgl.Value,
                    "F2"
                );
                model.Right.ValueString = _altitudeUnit.CurrentUnitItem.Value.PrintFromSi(
                    altitudeMsl.Value,
                    "F2"
                );

                model.Left.UnitSymbol = _altitudeUnit.CurrentUnitItem.Value.Symbol;
                model.Right.UnitSymbol = _altitudeUnit.CurrentUnitItem.Value.Symbol;
            },
        }
            .SetRoutableParent(this)
            .DisposeItWith(Disposable);
        AltitudeRttBox.Left.Header = "AGL";
        AltitudeRttBox.Right.Header = "MSL";

        VelocityRttBox = new SplitDigitRttBoxViewModel(
            nameof(VelocityRttBox),
            DesignTime.LoggerFactory,
            unitService,
            AngleBase.Id,
            velocity,
            null
        )
        {
            Header = RS.UavRttItem_Velocity,
            ShortHeader = "GS",
            Icon = MaterialIconKind.Speedometer,
        }
            .SetRoutableParent(this)
            .DisposeItWith(Disposable);

        AzimuthRttBox = new SplitDigitRttBoxViewModel(
            nameof(AzimuthRttBox),
            DesignTime.LoggerFactory,
            unitService,
            VelocityBase.Id,
            azimuth,
            null
        )
        {
            Header = RS.UavRttItem_Azimuth,
            Icon = MaterialIconKind.SunAzimuth,
        }
            .SetRoutableParent(this)
            .DisposeItWith(Disposable);

        BatteryRttBox = new KeyValueRttBoxViewModel<Unit>(
            nameof(BatteryRttBox),
            DesignTime.LoggerFactory,
            Observable
                .Merge(
                    batteryCharge.Select(_ => Unit.Default),
                    batteryAmperage.Select(_ => Unit.Default),
                    batteryVoltage.Select(_ => Unit.Default),
                    batteryConsumed.Select(_ => Unit.Default),
                    _progressUnit.CurrentUnitItem.Select(_ => Unit.Default),
                    _capacityUnit.CurrentUnitItem.Select(_ => Unit.Default),
                    _voltageUnit.CurrentUnitItem.Select(_ => Unit.Default),
                    _amperageUnit.CurrentUnitItem.Select(_ => Unit.Default)
                )
                .ObserveOnUIThreadDispatcher()
                .ThrottleLast(TimeSpan.FromMilliseconds(200)),
            null
        )
        {
            Header = RS.UavRttItem_Battery,
            Icon = MaterialIconKind.Battery10,
            UpdateAction = (model, _) =>
            {
                model[0, "Charge", _progressUnit.CurrentUnitItem.Value.Symbol].ValueString =
                    _progressUnit.CurrentUnitItem.Value.PrintFromSi(batteryCharge.Value, "F2");
                model[1, "Amperage", _amperageUnit.CurrentUnitItem.Value.Symbol].ValueString =
                    _amperageUnit.CurrentUnitItem.Value.PrintFromSi(batteryAmperage.Value, "F2");
                model[2, "Voltage", _voltageUnit.CurrentUnitItem.Value.Symbol].ValueString =
                    _voltageUnit.CurrentUnitItem.Value.PrintFromSi(batteryVoltage.Value, "F2");
                model[3, "Consumed", _capacityUnit.CurrentUnitItem.Value.Symbol].ValueString =
                    _capacityUnit.CurrentUnitItem.Value.PrintFromSi(batteryConsumed.Value, "F2");
            },
        }
            .SetRoutableParent(this)
            .DisposeItWith(Disposable);

        GnssRttBox = new KeyValueRttBoxViewModel<Unit>(
            nameof(GnssRttBox),
            DesignTime.LoggerFactory,
            Observable
                .Merge(
                    satelliteCount.Select(_ => Unit.Default),
                    hdopCount.Select(_ => Unit.Default),
                    vdopCount.Select(_ => Unit.Default),
                    gpsFixType.Select(_ => Unit.Default)
                )
                .ObserveOnUIThreadDispatcher()
                .ThrottleLast(TimeSpan.FromMilliseconds(200)),
            null
        )
        {
            Header = RS.UavRttItem_GNSS,
            Icon = MaterialIconKind.GpsFixed,
            UpdateAction = (model, _) =>
            {
                model[0, "Satellite count", null].ValueString = satelliteCount.Value.ToString();
                model[1, "Hdop count", null].ValueString = hdopCount.Value.ToString("F2");
                model[2, "Vdop count", null].ValueString = vdopCount.Value.ToString("F2");
                model[3, "Rtk mode", null].ValueString = gpsFixType.Value.ToString();
            },
        }
            .SetRoutableParent(this)
            .DisposeItWith(Disposable);

        LinkQualityRttBox = new SplitDigitRttBoxViewModel(
            nameof(LinkQualityRttBox),
            DesignTime.LoggerFactory,
            unitService,
            ProgressBase.Id,
            linkQuality,
            null
        )
        {
            Header = RS.UavRttItem_Link,
            Icon = MaterialIconKind.Wifi,
        }
            .SetRoutableParent(this)
            .DisposeItWith(Disposable);

        AltitudeAgl = altitudeAgl.ToReadOnlyBindableReactiveProperty().DisposeItWith(Disposable);
        AltitudeMsl = altitudeMsl.ToReadOnlyBindableReactiveProperty().DisposeItWith(Disposable);
        Azimuth = azimuth.ToReadOnlyBindableReactiveProperty().DisposeItWith(Disposable);
        Heading = heading.ToReadOnlyBindableReactiveProperty().DisposeItWith(Disposable);
        HomeAzimuth = homeAzimuth.ToReadOnlyBindableReactiveProperty().DisposeItWith(Disposable);
        Velocity = velocity.ToReadOnlyBindableReactiveProperty().DisposeItWith(Disposable);
        Roll = new BindableReactiveProperty<double>().DisposeItWith(Disposable);
        Pitch = new BindableReactiveProperty<double>().DisposeItWith(Disposable);
        IsArmed = new BindableReactiveProperty<bool>().DisposeItWith(Disposable);
        StatusText = new BindableReactiveProperty<string>().DisposeItWith(Disposable);
        VibrationX = new BindableReactiveProperty<float>().DisposeItWith(Disposable);
        VibrationY = new BindableReactiveProperty<float>().DisposeItWith(Disposable);
        VibrationZ = new BindableReactiveProperty<float>().DisposeItWith(Disposable);
        Clipping0 = new BindableReactiveProperty<uint>().DisposeItWith(Disposable);
        Clipping1 = new BindableReactiveProperty<uint>().DisposeItWith(Disposable);
        Clipping2 = new BindableReactiveProperty<uint>().DisposeItWith(Disposable);
        ArmedTime = new BindableReactiveProperty<TimeSpan>().DisposeItWith(Disposable);
        Roll = new BindableReactiveProperty<double>().DisposeItWith(Disposable);
        Pitch = new BindableReactiveProperty<double>().DisposeItWith(Disposable);
        IsArmed = new BindableReactiveProperty<bool>().DisposeItWith(Disposable);
        StatusText = new BindableReactiveProperty<string>().DisposeItWith(Disposable);
        VibrationX = new BindableReactiveProperty<float>().DisposeItWith(Disposable);
        VibrationY = new BindableReactiveProperty<float>().DisposeItWith(Disposable);
        VibrationZ = new BindableReactiveProperty<float>().DisposeItWith(Disposable);
        Clipping0 = new BindableReactiveProperty<uint>().DisposeItWith(Disposable);
        Clipping1 = new BindableReactiveProperty<uint>().DisposeItWith(Disposable);
        Clipping2 = new BindableReactiveProperty<uint>().DisposeItWith(Disposable);
        ArmedTime = new BindableReactiveProperty<TimeSpan>().DisposeItWith(Disposable);
    }

    public UavWidgetViewModel(
        IClientDevice device,
        INavigationService navigation,
        IUnitService unitService,
        IDeviceManager dev,
        ILoggerFactory loggerFactory
    )
        : base(WidgetId, loggerFactory)
    {
        ArgumentNullException.ThrowIfNull(device);
        Device = device;
        Position = WorkspaceDock.Left;
        Icon = dev.GetIcon(device.Id);
        IconColor = dev.GetDeviceColor(device.Id);
        _altitudeUnit = unitService.Units[AltitudeBase.Id];
        _velocityUnit = unitService.Units[VelocityBase.Id];
        _angleUnit = unitService.Units[AngleBase.Id];
        _capacityUnit = unitService.Units[CapacityBase.Id];
        _amperageUnit = unitService.Units[AmperageBase.Id];
        _voltageUnit = unitService.Units[VoltageBase.Id];
        _progressUnit = unitService.Units[ProgressBase.Id];
        device.Name.Subscribe(x => Header = x).DisposeItWith(Disposable);
        InitArgs(device.Id.AsString());
        MissionProgress = new MissionProgressViewModel(device, unitService, loggerFactory)
            .SetRoutableParent(this)
            .DisposeItWith(Disposable);
        var positionClientEx =
            device.GetMicroservice<IPositionClientEx>()
            ?? throw new ArgumentException(
                $"Unable to load {nameof(PositionClientEx)} from {device.Id}"
            );
        _gnssClient =
            device.GetMicroservice<IGnssClientEx>()
            ?? throw new ArgumentException(
                $"Unable to load {nameof(GnssClientEx)} from {device.Id}"
            );
        var telemetryClient =
            device.GetMicroservice<ITelemetryClientEx>()
            ?? throw new ArgumentException(
                $"Unable to load {nameof(TelemetryClientEx)} from {device.Id}"
            );
        var heartbeatClient =
            device.GetMicroservice<IHeartbeatClient>()
            ?? throw new ArgumentException(
                $"Unable to load {nameof(HeartbeatClient)} from {device.Id}"
            );
        IModeClient? modeClientRaw = device switch
        {
            ArduCopterClientDevice => device.GetMicroservice<ArduCopterModeClient>(),
            ArduPlaneClientDevice => device.GetMicroservice<ArduPlaneModeClient>(),
            _ => null,
        };
        var modeClient =
            modeClientRaw
            ?? throw new ArgumentException(
                $"Unable to load {nameof(PositionClientEx)} from {device.Id}"
            );
        TakeOff = new ReactiveCommand(
            async (_, ct) =>
            {
                using var vm = new SetAltitudeDialogViewModel(_altitudeUnit, loggerFactory);
                var dialog = new ContentDialog(vm, navigation)
                {
                    Title = RS.UavWidgetViewModel_SetAltitudeDialog_Title,
                    PrimaryButtonText =
                        RS.SetAltitudeDialogViewModel_ApplyDialog_PrimaryButton_TakeOff,
                    SecondaryButtonText =
                        RS.SetAltitudeDialogViewModel_ApplyDialog_SecondaryButton_Cancel,
                    IsSecondaryButtonEnabled = true,
                };

                var result = await dialog.ShowAsync();

                if (result == ContentDialogResult.Primary)
                {
                    await this.ExecuteCommand(
                        TakeOffCommand.Id,
                        new DoubleArg(
                            _altitudeUnit.CurrentUnitItem.Value.ParseToSi(vm.Altitude.Value)
                        ),
                        ct
                    );
                }
            }
        ).DisposeItWith(Disposable);
        Rtl = new BindableAsyncCommand(RTLCommand.Id, this);
        Land = new BindableAsyncCommand(LandCommand.Id, this);
        Guided = new BindableAsyncCommand(GuidedModeCommand.Id, this);
        AutoMode = new BindableAsyncCommand(AutoModeCommand.Id, this);
        StartMission = new BindableAsyncCommand(StartMissionCommand.Id, this);

        var batteryVoltage = new ReactiveProperty<double>().DisposeItWith(Disposable);

        var altitudeAgl = positionClientEx
            .Base.GlobalPosition.ObserveOnUIThreadDispatcher()
            .Select(pld => Math.Truncate((pld?.RelativeAlt ?? double.NaN) / 1000d));

        var altitudeMsl = positionClientEx
            .Base.GlobalPosition.ObserveOnUIThreadDispatcher()
            .Select(pld => Math.Truncate((pld?.Alt ?? double.NaN) / 1000d));

        var velocity = _gnssClient
            .Main.GroundVelocity.ObserveOnUIThreadDispatcher()
            .Select(Math.Truncate);

        var azimuth = positionClientEx
            .Yaw.ObserveOnUIThreadDispatcher()
            .Select(d => Math.Round(d, 2));
        var heading = positionClientEx.Yaw.ObserveOnUIThreadDispatcher().Select(Math.Truncate);

        var homeAzimuth = positionClientEx
            .Current.ObserveOnUIThreadDispatcher()
            .Where(_ => positionClientEx.Home.CurrentValue.HasValue)
            .ThrottleLast(TimeSpan.FromMilliseconds(200))
            .Select(p => p.Azimuth(positionClientEx.Home.CurrentValue ?? GeoPoint.NaN));

        var batteryAmperage = telemetryClient.BatteryCurrent.ObserveOnUIThreadDispatcher();

        var batteryCharge = telemetryClient.BatteryCharge.ObserveOnUIThreadDispatcher();

        var batteryConsumed = telemetryClient
            .BatteryCurrent.ObserveOnUIThreadDispatcher()
            .Select(d =>
                d == 0
                    ? double.NaN
                    : Math.Round(d * positionClientEx.ArmedTime.CurrentValue.TotalHours, 2)
            );
        var vdopCount = _gnssClient
            .Main.Info.ObserveOnUIThreadDispatcher()
            .Select(info => info.Vdop ?? double.NaN)
            .ToReadOnlyReactiveProperty()
            .DisposeItWith(Disposable);
        var hdopCount = _gnssClient
            .Main.Info.Select(info => info.Hdop ?? double.NaN)
            .ObserveOnUIThreadDispatcher()
            .ToReadOnlyReactiveProperty()
            .DisposeItWith(Disposable);
        var satelliteCount = _gnssClient
            .Main.Info.Select(info => info.SatellitesVisible)
            .ObserveOnUIThreadDispatcher()
            .ToReadOnlyReactiveProperty()
            .DisposeItWith(Disposable);
        var rtkMode = _gnssClient
            .Main.Info.Select(gpsInfo => gpsInfo.FixType)
            .ObserveOnUIThreadDispatcher()
            .ToReadOnlyReactiveProperty()
            .DisposeItWith(Disposable);
        heartbeatClient
            .Link.State.ObserveOnUIThreadDispatcher()
            .Skip(1)
            .Subscribe(ChangeLinkStatus)
            .DisposeItWith(Disposable);

        AltitudeAgl = altitudeAgl.ToReadOnlyBindableReactiveProperty().DisposeItWith(Disposable);
        AltitudeMsl = altitudeMsl.ToReadOnlyBindableReactiveProperty().DisposeItWith(Disposable);
        Azimuth = azimuth.ToReadOnlyBindableReactiveProperty().DisposeItWith(Disposable);
        Heading = heading.ToReadOnlyBindableReactiveProperty().DisposeItWith(Disposable);
        Velocity = velocity.ToReadOnlyBindableReactiveProperty().DisposeItWith(Disposable);
        HomeAzimuth = homeAzimuth.ToReadOnlyBindableReactiveProperty().DisposeItWith(Disposable);
        Roll = positionClientEx.Roll.ToBindableReactiveProperty().DisposeItWith(Disposable);
        Pitch = positionClientEx.Pitch.ToBindableReactiveProperty().DisposeItWith(Disposable);
        IsArmed = positionClientEx
            .IsArmed.DistinctUntilChanged()
            .ObserveOnUIThreadDispatcher()
            .Select(b => b)
            .ToBindableReactiveProperty()
            .DisposeItWith(Disposable);
        StatusText = positionClientEx
            .IsArmed.ObserveOnUIThreadDispatcher()
            .Select(b =>
                b
                    ? RS.UavWidgetViewModel_StatusText_Armed
                    : RS.UavWidgetViewModel_StatusText_DisArmed
            )
            .ToBindableReactiveProperty<string>()
            .DisposeItWith(Disposable);
        VibrationX = new BindableReactiveProperty<float>().DisposeItWith(Disposable);
        VibrationY = new BindableReactiveProperty<float>().DisposeItWith(Disposable);
        VibrationZ = new BindableReactiveProperty<float>().DisposeItWith(Disposable);
        Clipping0 = new BindableReactiveProperty<uint>().DisposeItWith(Disposable);
        Clipping1 = new BindableReactiveProperty<uint>().DisposeItWith(Disposable);
        Clipping2 = new BindableReactiveProperty<uint>().DisposeItWith(Disposable);
        ArmedTime = new BindableReactiveProperty<TimeSpan>().DisposeItWith(Disposable);

        Observable
            .Timer(TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(4))
            .Subscribe(_ => StatusText.Value = string.Empty)
            .DisposeItWith(Disposable);

        CurrentFlightModeRttBox = new SingleRttBoxViewModel<string>(
            nameof(CurrentFlightModeRttBox),
            loggerFactory,
            modeClient.CurrentMode.Select(mode => mode.Name),
            null
        )
        {
            Header = RS.UavRttItem_Mode,
            Icon = MaterialIconKind.FlightMode,
            UpdateAction = (model, mode) => model.ValueString = mode,
            Status = DefaultStatusColor,
        }
            .SetRoutableParent(this)
            .DisposeItWith(Disposable);

        AltitudeRttBox = new TwoColumnRttBoxViewModel<AltitudeRttBoxData>(
            nameof(AltitudeRttBox),
            loggerFactory,
            altitudeAgl
                .CombineLatest(
                    altitudeMsl,
                    _altitudeUnit.CurrentUnitItem,
                    (agl, msl, unit) => new AltitudeRttBoxData(agl, msl, unit)
                )
                .ObserveOnUIThreadDispatcher()
                .ThrottleLast(TimeSpan.FromMilliseconds(200)),
            null
        )
        {
            Header = RS.UavRttItem_Altitude,
            Icon = MaterialIconKind.Altimeter,
            UpdateAction = (model, changes) =>
            {
                model.Left.ValueString = changes.AltitudeUnit.PrintFromSi(
                    changes.AltitudeAgl,
                    "F2"
                );
                model.Right.ValueString = changes.AltitudeUnit.PrintFromSi(
                    changes.AltitudeMsl,
                    "F2"
                );

                model.Left.UnitSymbol = changes.AltitudeUnit.Symbol;
                model.Right.UnitSymbol = changes.AltitudeUnit.Symbol;

                if (positionClientEx.Base.GlobalPosition.CurrentValue is not null)
                {
                    SpeedAltitudeCheck(
                        positionClientEx.Base.GlobalPosition.CurrentValue.RelativeAlt / 1000,
                        Math.Round(_gnssClient.Main.GroundVelocity.CurrentValue)
                    );
                }
            },
            Status = DefaultStatusColor,
        }
            .SetRoutableParent(this)
            .DisposeItWith(Disposable);
        AltitudeRttBox.Left.Header = "AGL";
        AltitudeRttBox.Right.Header = "MSL";

        VelocityRttBox = new SplitDigitRttBoxViewModel(
            nameof(VelocityRttBox),
            loggerFactory,
            unitService,
            VelocityBase.Id,
            velocity,
            null
        )
        {
            Header = RS.UavRttItem_Velocity,
            ShortHeader = "GS",
            Icon = MaterialIconKind.Speedometer,
            Status = DefaultStatusColor,
        }
            .SetRoutableParent(this)
            .DisposeItWith(Disposable);

        AzimuthRttBox = new SplitDigitRttBoxViewModel(
            nameof(AzimuthRttBox),
            loggerFactory,
            unitService,
            AngleBase.Id,
            azimuth,
            null
        )
        {
            Header = RS.UavRttItem_Azimuth,
            Icon = MaterialIconKind.SunAzimuth,
            Status = DefaultStatusColor,
        }
            .SetRoutableParent(this)
            .DisposeItWith(Disposable);

        BatteryRttBox = new KeyValueRttBoxViewModel<BatteryRttBoxData>(
            nameof(BatteryRttBox),
            loggerFactory,
            batteryCharge
                .CombineLatest(
                    batteryAmperage,
                    batteryVoltage,
                    batteryConsumed,
                    _progressUnit.CurrentUnitItem,
                    _amperageUnit.CurrentUnitItem,
                    _capacityUnit.CurrentUnitItem,
                    _voltageUnit.CurrentUnitItem,
                    (bC, bA, bV, bCo, prog, amp, cap, vol) =>
                        new BatteryRttBoxData(bC, bA, bV, bCo, prog, amp, cap, vol)
                )
                .ObserveOnUIThreadDispatcher()
                .ThrottleLast(TimeSpan.FromMilliseconds(200)),
            null
        )
        {
            Header = RS.UavRttItem_Battery,
            Icon = MaterialIconKind.Battery10,
            UpdateAction = (model, changes) =>
            {
                model[0, "Charge", changes.ProgressUnit.Symbol].ValueString =
                    changes.ProgressUnit.PrintFromSi(changes.Charge, "F2");
                model[1, "Amperage", changes.AmperageUnit.Symbol].ValueString =
                    changes.AmperageUnit.PrintFromSi(changes.Amperage, "F2");
                model[2, "Voltage", changes.VoltageUnit.Symbol].ValueString =
                    changes.VoltageUnit.PrintFromSi(changes.Voltage, "F2");
                model[3, "Consumed", changes.CapacityUnit.Symbol].ValueString =
                    changes.CapacityUnit.PrintFromSi(changes.Consumed, "F2");

                ChangeBatteryStatus(changes.Charge);
            },
            Status = DefaultStatusColor,
        }
            .SetRoutableParent(this)
            .DisposeItWith(Disposable);

        GnssRttBox = new KeyValueRttBoxViewModel<GnssRttBoxData>(
            nameof(GnssRttBox),
            loggerFactory,
            satelliteCount
                .CombineLatest(
                    hdopCount,
                    vdopCount,
                    rtkMode,
                    (satellites, hdops, vdops, mode) =>
                        new GnssRttBoxData(satellites, hdops, vdops, mode)
                )
                .ObserveOnUIThreadDispatcher()
                .ThrottleLast(TimeSpan.FromMilliseconds(200)),
            null
        )
        {
            Header = RS.UavRttItem_GNSS,
            Icon = MaterialIconKind.GpsFixed,
            UpdateAction = (model, changes) =>
            {
                model[0, "Satellites", null].ValueString = changes.Sattelites.ToString();
                model[1, "Hdops", null].ValueString = changes.HdopCount.ToString("F2");
                model[2, "Vdops", null].ValueString = changes.VdopCount.ToString("F2");
                model[3, "Mode", null].ValueString = GpsFixTypeToString(changes.Mode);
                ChangeGnssStatus();
            },
            Status = DefaultStatusColor,
        }
            .SetRoutableParent(this)
            .DisposeItWith(Disposable);

        LinkQualityRttBox = new SplitDigitRttBoxViewModel(
            nameof(LinkQualityRttBox),
            loggerFactory,
            unitService,
            ProgressBase.Id,
            heartbeatClient.LinkQuality,
            null
        )
        {
            Header = RS.UavRttItem_Link,
            Icon = MaterialIconKind.Wifi,
            Status = DefaultStatusColor,
        }
            .SetRoutableParent(this)
            .DisposeItWith(Disposable);
    }

    public ReactiveCommand TakeOff { get; }
    public ICommand AutoMode { get; }
    public ICommand Rtl { get; }
    public ICommand Land { get; }
    public ICommand Guided { get; }
    public ICommand StartMission { get; }

    public MissionProgressViewModel MissionProgress { get; }

    public SingleRttBoxViewModel<string> CurrentFlightModeRttBox { get; }
    public TwoColumnRttBoxViewModel AltitudeRttBox { get; }
    public SplitDigitRttBoxViewModel VelocityRttBox { get; }
    public SplitDigitRttBoxViewModel AzimuthRttBox { get; }
    public SplitDigitRttBoxViewModel LinkQualityRttBox { get; }
    public KeyValueRttBoxViewModel BatteryRttBox { get; }
    public KeyValueRttBoxViewModel GnssRttBox { get; }

    public BindableReactiveProperty<float> VibrationX { get; }
    public BindableReactiveProperty<float> VibrationY { get; }
    public BindableReactiveProperty<float> VibrationZ { get; }
    public BindableReactiveProperty<uint> Clipping0 { get; }
    public BindableReactiveProperty<uint> Clipping1 { get; }
    public BindableReactiveProperty<uint> Clipping2 { get; }
    public BindableReactiveProperty<double> Roll { get; }
    public BindableReactiveProperty<double> Pitch { get; }
    public IReadOnlyBindableReactiveProperty<double> Velocity { get; }
    public IReadOnlyBindableReactiveProperty<double> AltitudeAgl { get; }
    public IReadOnlyBindableReactiveProperty<double> AltitudeMsl { get; }
    public IReadOnlyBindableReactiveProperty<double> Heading { get; }
    public IReadOnlyBindableReactiveProperty<double> HomeAzimuth { get; }
    public IReadOnlyBindableReactiveProperty<double> Azimuth { get; }
    public BindableReactiveProperty<string> StatusText { get; }
    public BindableReactiveProperty<bool> IsArmed { get; }
    public BindableReactiveProperty<TimeSpan> ArmedTime { get; }

    public IClientDevice Device { get; }

    public override IEnumerable<IRoutable> GetChildren()
    {
        yield return MissionProgress;
        yield return CurrentFlightModeRttBox;
        yield return AltitudeRttBox;
        yield return VelocityRttBox;
        yield return AzimuthRttBox;
        yield return LinkQualityRttBox;
        yield return BatteryRttBox;
        yield return GnssRttBox;
    }

    private void ChangeBatteryStatus(double percent)
    {
        BatteryRttBox.Status = percent switch
        {
            > 0.7d => DefaultStatusColor,
            > 0.5d => AsvColorKind.Warning,
            > 0.4d => AsvColorKind.Warning | AsvColorKind.Blink,
            < 0.3d => AsvColorKind.Error | AsvColorKind.Blink,
            _ => DefaultStatusColor,
        };
    }

    private void ChangeLinkStatus(LinkState state)
    {
        LinkQualityRttBox.Status = state switch
        {
            Common.LinkState.Connected => AsvColorKind.Success,
            Common.LinkState.Downgrade => AsvColorKind.Warning,
            Common.LinkState.Disconnected => AsvColorKind.Error,
            _ => AsvColorKind.None,
        };
    }

    private void SpeedAltitudeCheck(int alt, double gs)
    {
        if (gs > DangerHighSpeed && alt < CriticalAltitude)
        {
            AltitudeRttBox.StatusText = RS.UavWidgetViewModel_StatusText_PullUp;
            AltitudeRttBox.Status = AsvColorKind.Warning | AsvColorKind.Blink;
        }
        else
        {
            StatusText.Value = string.Empty;
            AltitudeRttBox.Status = DefaultStatusColor;
        }
    }

    private void ChangeGnssStatus()
    {
        if (_gnssClient is null)
        {
            return;
        }

        if (
            _gnssClient.Main.Info.CurrentValue.FixType
                == Mavlink.Common.GpsFixType.GpsFixTypeRtkFloat
            || _gnssClient.Main.Info.CurrentValue.SatellitesVisible
                > WarningSatelliteAmount.Start.Value
            || _gnssClient.Main.Info.CurrentValue.SatellitesVisible
                < WarningSatelliteAmount.End.Value
        )
        {
            GnssRttBox.Status = AsvColorKind.Warning;
            return;
        }

        if (
            (
                _gnssClient.Main.Info.CurrentValue.FixType
                    != Mavlink.Common.GpsFixType.GpsFixTypeRtkFloat
                && _gnssClient.Main.Info.CurrentValue.FixType
                    != Mavlink.Common.GpsFixType.GpsFixTypeRtkFixed
            )
            || _gnssClient.Main.Info.CurrentValue.SatellitesVisible < DangerSatelliteCount
        )
        {
            GnssRttBox.Status = AsvColorKind.Error;
            return;
        }

        GnssRttBox.Status = DefaultStatusColor;
    }

    private string GpsFixTypeToString(Mavlink.Common.GpsFixType type)
    {
        return type switch
        {
            Mavlink.Common.GpsFixType.GpsFixType2dFix => RS.GpsFixType_GpsFixType2dFix,
            Mavlink.Common.GpsFixType.GpsFixTypeRtkFloat => RS.GpsFixType_GpsFixTypeRtkFloat,
            Mavlink.Common.GpsFixType.GpsFixTypeRtkFixed => RS.GpsFixType_GpsFixTypeRtkFixed,
            Mavlink.Common.GpsFixType.GpsFixTypeDgps => RS.GpsFixType_GpsFixTypeDgps,
            Mavlink.Common.GpsFixType.GpsFixTypePpp => RS.GpsFixType_GpsFixTypePpp,
            Mavlink.Common.GpsFixType.GpsFixType3dFix => RS.GpsFixType_GpsFixType3dFix,
            Mavlink.Common.GpsFixType.GpsFixTypeStatic => RS.GpsFixType_GpsFixTypeStatic,
            Mavlink.Common.GpsFixType.GpsFixTypeNoGps => RS.GpsFixType_GpsFixTypeNoGps,
            _ => string.Empty,
        };
    }
}
