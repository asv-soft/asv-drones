using System;
using System.Collections.Generic;
using System.Windows.Input;
using Asv.Avalonia;
using Asv.Avalonia.GeoMap;
using Asv.Avalonia.IO;
using Asv.Common;
using Asv.Drones.Api;
using Asv.IO;
using Asv.Mavlink;
using Material.Icons;
using Microsoft.Extensions.Logging;
using R3;
using Observable = R3.Observable;

namespace Asv.Drones;

public class UavWidgetViewModel : MapWidget, IUavFlightWidget
{
    private const string WidgetId = "widget-uav";
    private const AsvColorKind DefaultStatusColor = AsvColorKind.Info5;

    private readonly IUnit _altitudeUnit;
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
            new VelocityUnit(
                DesignTime.Configuration,
                [new VelocityMetersPerSecondUnitItem(), new VelocityMilesPerHourUnitItem()]
            )
        );
        NullUnitService.Instance.Extend(
            new ProgressUnit(
                DesignTime.Configuration,
                [new ProgressPercentUnitItem(), new ProgressInPartsUnitItem()]
            )
        );
        NullUnitService.Instance.Extend(
            new CapacityUnit(DesignTime.Configuration, [new CapacityMilliAmperePerHourUnitItem()])
        );
        NullUnitService.Instance.Extend(
            new AmperageUnit(
                DesignTime.Configuration,
                [new AmperageAmpereUnitItem(), new AmperageMilliAmpereUnitItem()]
            )
        );
        NullUnitService.Instance.Extend(
            new VoltageUnit(
                DesignTime.Configuration,
                [new VoltageVoltUnitItem(), new VoltageMilliVoltUnitItem()]
            )
        );

        var unitService = NullUnitService.Instance;

        Icon = MaterialIconKind.AccountFile;
        IconColor = AsvColorKind.Info5;

        _altitudeUnit = unitService.Units[AltitudeUnit.Id];
        _capacityUnit = unitService.Units[CapacityUnit.Id];
        _amperageUnit = unitService.Units[AmperageUnit.Id];
        _voltageUnit = unitService.Units[VoltageUnit.Id];
        _progressUnit = unitService.Units[ProgressUnit.Id];

        var linkQuality = new ReactiveProperty<double>(100).DisposeItWith(Disposable);
        var altitudeAgl = new ReactiveProperty<double>(10).DisposeItWith(Disposable);
        var altitudeMsl = new ReactiveProperty<double>(14).DisposeItWith(Disposable);
        var heading = new ReactiveProperty<double>(29).DisposeItWith(Disposable);
        var azimuth = new ReactiveProperty<double>(39).DisposeItWith(Disposable);
        var homeAzimuth = new ReactiveProperty<double>(30).DisposeItWith(Disposable);
        var satelliteCount = new ReactiveProperty<int>(10).DisposeItWith(Disposable);
        var hdopCount = new ReactiveProperty<double>(2).DisposeItWith(Disposable);
        var vdopCount = new ReactiveProperty<double>(4).DisposeItWith(Disposable);
        var gpsFixType = new ReactiveProperty<Mavlink.Common.GpsFixType>(
            Mavlink.Common.GpsFixType.GpsFixTypeDgps
        ).DisposeItWith(Disposable);
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
            Status = DefaultStatusColor,
        }
            .SetRoutableParent(this)
            .DisposeItWith(Disposable);

        AltitudeRttBox = new TwoColumnRttBoxViewModel<AltitudeRttBoxData>(
            nameof(AltitudeRttBox),
            DesignTime.LoggerFactory,
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
            },
            Status = DefaultStatusColor,
        }
            .SetRoutableParent(this)
            .DisposeItWith(Disposable);
        AltitudeRttBox.Left.Header = "AGL";
        AltitudeRttBox.Right.Header = "MSL";

        VelocityRttBox = new SplitDigitRttBoxViewModel(
            nameof(VelocityRttBox),
            DesignTime.LoggerFactory,
            unitService,
            VelocityUnit.Id,
            velocity,
            null
        )
        {
            Header = RS.UavRttItem_Velocity,
            ShortHeader = "GS",
            Icon = MaterialIconKind.Speedometer,
            Status = DefaultStatusColor,
            FormatString = "F2",
        }
            .SetRoutableParent(this)
            .DisposeItWith(Disposable);

        AzimuthRttBox = new SplitDigitRttBoxViewModel(
            nameof(AzimuthRttBox),
            DesignTime.LoggerFactory,
            unitService,
            AngleUnit.Id,
            azimuth,
            null
        )
        {
            Header = RS.UavRttItem_Azimuth,
            Icon = MaterialIconKind.SunAzimuth,
            Status = DefaultStatusColor,
            FormatString = "F2",
        }
            .SetRoutableParent(this)
            .DisposeItWith(Disposable);

        BatteryRttBox = new KeyValueRttBoxViewModel<BatteryRttBoxData>(
            nameof(BatteryRttBox),
            DesignTime.LoggerFactory,
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
                model[
                    0,
                    RS.UavWidgetViewModel_BatteryRttBox_BatteryCharge_Header,
                    changes.ProgressUnit.Symbol
                ].ValueString = changes.ProgressUnit.PrintFromSi(changes.Charge, "F2");
                model[
                    1,
                    RS.UavWidgetViewModel_BatteryRttBox_BatteryAmperage_Header,
                    changes.AmperageUnit.Symbol
                ].ValueString = changes.AmperageUnit.PrintFromSi(changes.Amperage, "F2");
                model[
                    2,
                    RS.UavWidgetViewModel_BatteryRttBox_BatteryVoltage_Header,
                    changes.VoltageUnit.Symbol
                ].ValueString = changes.VoltageUnit.PrintFromSi(changes.Voltage, "F2");
                model[
                    3,
                    RS.UavWidgetViewModel_BatteryRttBox_BatteryConsumed_Header,
                    changes.CapacityUnit.Symbol
                ].ValueString = changes.CapacityUnit.PrintFromSi(changes.Consumed, "F2");

                ChangeBatteryStatus(changes.Charge);
            },
            Status = DefaultStatusColor,
        }
            .SetRoutableParent(this)
            .DisposeItWith(Disposable);

        GnssRttBox = new KeyValueRttBoxViewModel<GnssRttBoxData>(
            nameof(GnssRttBox),
            DesignTime.LoggerFactory,
            satelliteCount
                .CombineLatest(
                    hdopCount,
                    vdopCount,
                    gpsFixType,
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
                model[
                    0,
                    RS.UavWidgetViewModel_GnssRttBox_SatellitesCount_Header,
                    null
                ].ValueString = changes.Sattelites.ToString();
                model[1, RS.UavWidgetViewModel_GnssRttBox_Hdop_Header, null].ValueString =
                    changes.HdopCount.ToString("F2");
                model[2, RS.UavWidgetViewModel_GnssRttBox_Vdop_Header, null].ValueString =
                    changes.VdopCount.ToString("F2");
                model[3, RS.UavWidgetViewModel_GnssRttBox_Mode_Header, null].ValueString =
                    GpsFixTypeToString(changes.Mode);
                ChangeGnssStatus(changes.Sattelites, changes.Mode);
            },
            Status = DefaultStatusColor,
        }
            .SetRoutableParent(this)
            .DisposeItWith(Disposable);

        LinkQualityRttBox = new SplitDigitRttBoxViewModel(
            nameof(LinkQualityRttBox),
            DesignTime.LoggerFactory,
            unitService,
            ProgressUnit.Id,
            linkQuality,
            null
        )
        {
            Header = RS.UavRttItem_Link,
            Icon = MaterialIconKind.Wifi,
            Status = DefaultStatusColor,
            FormatString = "F2",
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
        _altitudeUnit = unitService.Units[AltitudeUnit.Id];
        _capacityUnit = unitService.Units[CapacityUnit.Id];
        _amperageUnit = unitService.Units[AmperageUnit.Id];
        _voltageUnit = unitService.Units[VoltageUnit.Id];
        _progressUnit = unitService.Units[ProgressUnit.Id];
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
        var batteryVoltage = telemetryClient.BatteryVoltage.ObserveOnUIThreadDispatcher();
        var batteryConsumed = telemetryClient
            .BatteryCurrent.ObserveOnUIThreadDispatcher()
            .Select(d =>
                d == 0
                    ? double.NaN
                    : Math.Round(d * positionClientEx.ArmedTime.CurrentValue.TotalHours, 2)
            );
        var vdop = _gnssClient
            .Main.Info.ObserveOnUIThreadDispatcher()
            .Select(info => info.Vdop ?? double.NaN);
        var hdop = _gnssClient
            .Main.Info.Select(info => info.Hdop ?? double.NaN)
            .ObserveOnUIThreadDispatcher();
        var satelliteCount = _gnssClient
            .Main.Info.Select(info => info.SatellitesVisible)
            .ObserveOnUIThreadDispatcher();
        var rtkMode = _gnssClient
            .Main.Info.Select(gpsInfo => gpsInfo.FixType)
            .ObserveOnUIThreadDispatcher();

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
        heartbeatClient
            .Link.State.ObserveOnUIThreadDispatcher()
            .Skip(1)
            .Subscribe(ChangeLinkStatus)
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

                CheckSpeedAltitude(
                    changes.AltitudeAgl,
                    Math.Round(_gnssClient.Main.GroundVelocity.CurrentValue)
                );
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
            VelocityUnit.Id,
            velocity,
            null
        )
        {
            Header = RS.UavRttItem_Velocity,
            ShortHeader = "GS",
            Icon = MaterialIconKind.Speedometer,
            Status = DefaultStatusColor,
            FormatString = "F2",
        }
            .SetRoutableParent(this)
            .DisposeItWith(Disposable);

        AzimuthRttBox = new SplitDigitRttBoxViewModel(
            nameof(AzimuthRttBox),
            loggerFactory,
            unitService,
            AngleUnit.Id,
            azimuth,
            null
        )
        {
            Header = RS.UavRttItem_Azimuth,
            Icon = MaterialIconKind.SunAzimuth,
            Status = DefaultStatusColor,
            FormatString = "F2",
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
                model[
                    0,
                    RS.UavWidgetViewModel_BatteryRttBox_BatteryCharge_Header,
                    changes.ProgressUnit.Symbol
                ].ValueString = changes.ProgressUnit.PrintFromSi(changes.Charge, "F2");
                model[
                    1,
                    RS.UavWidgetViewModel_BatteryRttBox_BatteryAmperage_Header,
                    changes.AmperageUnit.Symbol
                ].ValueString = changes.AmperageUnit.PrintFromSi(changes.Amperage, "F2");
                model[
                    2,
                    RS.UavWidgetViewModel_BatteryRttBox_BatteryVoltage_Header,
                    changes.VoltageUnit.Symbol
                ].ValueString = changes.VoltageUnit.PrintFromSi(changes.Voltage, "F2");
                model[
                    3,
                    RS.UavWidgetViewModel_BatteryRttBox_BatteryConsumed_Header,
                    changes.CapacityUnit.Symbol
                ].ValueString = changes.CapacityUnit.PrintFromSi(changes.Consumed, "F2");

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
                    hdop,
                    vdop,
                    rtkMode,
                    (satellites, hdopValue, vdopValue, mode) =>
                        new GnssRttBoxData(satellites, hdopValue, vdopValue, mode)
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
                model[
                    0,
                    RS.UavWidgetViewModel_GnssRttBox_SatellitesCount_Header,
                    null
                ].ValueString = changes.Sattelites.ToString();
                model[1, RS.UavWidgetViewModel_GnssRttBox_Hdop_Header, null].ValueString =
                    changes.HdopCount.ToString("F2");
                model[2, RS.UavWidgetViewModel_GnssRttBox_Vdop_Header, null].ValueString =
                    changes.VdopCount.ToString("F2");
                model[3, RS.UavWidgetViewModel_GnssRttBox_Mode_Header, null].ValueString =
                    GpsFixTypeToString(changes.Mode);
                ChangeGnssStatus(changes.Sattelites, changes.Mode);
            },
            Status = DefaultStatusColor,
        }
            .SetRoutableParent(this)
            .DisposeItWith(Disposable);

        LinkQualityRttBox = new SplitDigitRttBoxViewModel(
            nameof(LinkQualityRttBox),
            loggerFactory,
            unitService,
            ProgressUnit.Id,
            heartbeatClient.LinkQuality,
            null
        )
        {
            Header = RS.UavRttItem_Link,
            Icon = MaterialIconKind.Wifi,
            Status = DefaultStatusColor,
            FormatString = "F2",
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

    private void CheckSpeedAltitude(double alt, double gs)
    {
        if (gs > DangerHighSpeed && alt < CriticalAltitude)
        {
            StatusText.Value = RS.UavWidgetViewModel_StatusText_PullUp;
            AltitudeRttBox.StatusText = RS.UavWidgetViewModel_StatusText_PullUp;
            AltitudeRttBox.Status = AsvColorKind.Warning | AsvColorKind.Blink;
        }
        else
        {
            StatusText.Value = string.Empty;
            AltitudeRttBox.StatusText = string.Empty;
            AltitudeRttBox.Status = DefaultStatusColor;
        }
    }

    private void ChangeGnssStatus(int satellitesCount, Mavlink.Common.GpsFixType mode)
    {
        if (_gnssClient is null)
        {
            return;
        }

        if (
            mode == Mavlink.Common.GpsFixType.GpsFixTypeRtkFloat
            || satellitesCount > WarningSatelliteAmount.Start.Value
            || satellitesCount < WarningSatelliteAmount.End.Value
        )
        {
            GnssRttBox.Status = AsvColorKind.Warning;
            return;
        }

        if (
            mode != Mavlink.Common.GpsFixType.GpsFixTypeRtkFixed
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
