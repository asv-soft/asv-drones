using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Input;
using Asv.Avalonia;
using Asv.Avalonia.IO;
using Asv.Common;
using Asv.Drones.Api;
using Asv.IO;
using Asv.Mavlink;
using Avalonia.Media;
using Avalonia.Threading;
using Material.Icons;
using Microsoft.Extensions.Logging;
using R3;
using Observable = R3.Observable;

namespace Asv.Drones;

public class UavWidgetViewModel : ExtendableHeadlinedViewModel<IUavFlightWidget>, IUavFlightWidget
{
    private const string WidgetId = "widget-uav";
    private WorkspaceDock _position;

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

    private static readonly Color GreenColor = Color.Parse("#21c088");
    private static readonly Color OrangeColor = Color.Parse("#e48f4d");
    private static readonly Color RedColor = Color.Parse("#cc5058");
    private static readonly Color YellowColor = Color.Parse("#dfc34a");

    public UavWidgetViewModel()
        : base(WidgetId, DesignTime.LoggerFactory)
    {
        DesignTime.ThrowIfNotDesignMode();
        Dispatcher.UIThread.InvokeAsync(() =>
        {
            GnssStatusBrush = new SolidColorBrush();
            LinkQualityStatusBrush = new SolidColorBrush();
            BatteryStatusBrush = new SolidColorBrush();
            AltitudeStatusBrush = new SolidColorBrush();

            AltitudeStatusBrush.Color = GreenColor;
            BatteryStatusBrush.Color = RedColor;
            GnssStatusBrush.Color = OrangeColor;
            LinkQualityStatusBrush.Color = GreenColor;
            GnssStatusBrush.Color = GreenColor;
        });
        TakeOff = new ReactiveCommand();
        AutoMode = new ReactiveCommand();
        Rtl = new ReactiveCommand();
        Guided = new ReactiveCommand();
        StartMission = new ReactiveCommand();
        InitArgs("1");
        MissionProgress = new MissionProgressViewModel().DisposeItWith(Disposable);

        var unitService = NullUnitService.Instance;

        Icon = MaterialIconKind.AccountFile;
        IconBrush = Brush.Parse("Blue");
        var unitItem = unitService.Units[NullUnitBase.Id];
        _altitudeUnit = unitService.Units[NullUnitBase.Id];
        _velocityUnit = unitService.Units[NullUnitBase.Id];
        _angleUnit = unitService.Units[NullUnitBase.Id];
        _capacityUnit = unitService.Units[NullUnitBase.Id];
        _amperageUnit = unitService.Units[NullUnitBase.Id];
        _voltageUnit = unitService.Units[NullUnitBase.Id];
        _progressUnit = unitService.Units[NullUnitBase.Id];

        var linkQuality = new ReactiveProperty<double>(100).DisposeItWith(Disposable);
        var altitudeAgl = new ReactiveProperty<double>(10).DisposeItWith(Disposable);
        var altitudeMsl = new ReactiveProperty<double>(14).DisposeItWith(Disposable);
        var heading = new ReactiveProperty<double>(29).DisposeItWith(Disposable);
        var azimuth = new ReactiveProperty<double>(39).DisposeItWith(Disposable);
        var homeAzimuth = new ReactiveProperty<double>(30).DisposeItWith(Disposable);
        var velocity = new ReactiveProperty<double>(199).DisposeItWith(Disposable);
        var batteryAmperage = new ReactiveProperty<double>(39).DisposeItWith(Disposable);
        var batteryVoltage = new ReactiveProperty<double>(34).DisposeItWith(Disposable);
        var batteryCharge = new ReactiveProperty<double>(123).DisposeItWith(Disposable);
        var batteryConsumed = new ReactiveProperty<double>(39).DisposeItWith(Disposable);

        LinkQuality = new HistoricalUnitProperty(
            nameof(LinkQuality),
            linkQuality,
            unitItem,
            DesignTime.LoggerFactory,
            this
        ).DisposeItWith(Disposable);
        LinkQuality.ForceValidate();
        AltitudeAgl = new HistoricalUnitProperty(
            nameof(AltitudeAgl),
            altitudeAgl,
            unitItem,
            DesignTime.LoggerFactory,
            this
        ).DisposeItWith(Disposable);
        AltitudeAgl.ForceValidate();
        AltitudeMsl = new HistoricalUnitProperty(
            nameof(AltitudeMsl),
            altitudeMsl,
            unitItem,
            DesignTime.LoggerFactory,
            this
        ).DisposeItWith(Disposable);
        AltitudeMsl.ForceValidate();
        Azimuth = new HistoricalUnitProperty(
            nameof(Azimuth),
            azimuth,
            unitItem,
            DesignTime.LoggerFactory,
            this
        ).DisposeItWith(Disposable);
        Azimuth.ForceValidate();
        Heading = new HistoricalUnitProperty(
            nameof(Heading),
            heading,
            unitItem,
            DesignTime.LoggerFactory,
            this
        ).DisposeItWith(Disposable);
        Heading.ForceValidate();
        HomeAzimuth = new HistoricalUnitProperty(
            nameof(HomeAzimuth),
            homeAzimuth,
            unitItem,
            DesignTime.LoggerFactory,
            this
        ).DisposeItWith(Disposable);
        HomeAzimuth.ForceValidate();
        Velocity = new HistoricalUnitProperty(
            nameof(Velocity),
            velocity,
            unitItem,
            DesignTime.LoggerFactory,
            this
        ).DisposeItWith(Disposable);
        Velocity.ForceValidate();
        BatteryAmperage = new HistoricalUnitProperty(
            nameof(BatteryAmperage),
            batteryAmperage,
            unitItem,
            DesignTime.LoggerFactory,
            this,
            "N2"
        ).DisposeItWith(Disposable);
        BatteryAmperage.ForceValidate();
        BatteryVoltage = new HistoricalUnitProperty(
            nameof(BatteryVoltage),
            batteryVoltage,
            unitItem,
            DesignTime.LoggerFactory,
            this,
            "N2"
        ).DisposeItWith(Disposable);
        BatteryVoltage.ForceValidate();
        BatteryCharge = new HistoricalUnitProperty(
            nameof(BatteryCharge),
            batteryCharge,
            unitItem,
            DesignTime.LoggerFactory,
            this
        ).DisposeItWith(Disposable);
        BatteryCharge.ForceValidate();
        BatteryConsumed = new HistoricalUnitProperty(
            nameof(BatteryConsumed),
            batteryConsumed,
            unitItem,
            DesignTime.LoggerFactory,
            this
        ).DisposeItWith(Disposable);
        BatteryConsumed.ForceValidate();

        LinkState = new BindableReactiveProperty<string>("Connected").DisposeItWith(Disposable);
        CurrentFlightMode = new BindableReactiveProperty<string>("Auto").DisposeItWith(Disposable);
        Roll = new BindableReactiveProperty<double>().DisposeItWith(Disposable);
        Pitch = new BindableReactiveProperty<double>().DisposeItWith(Disposable);
        VdopCount = new BindableReactiveProperty<string>().DisposeItWith(Disposable);
        HdopCount = new BindableReactiveProperty<string>().DisposeItWith(Disposable);
        SatelliteCount = new BindableReactiveProperty<int>(10).DisposeItWith(Disposable);
        RtkMode = new BindableReactiveProperty<string>("RTK Fixed").DisposeItWith(Disposable);
        IsArmed = new BindableReactiveProperty<bool>().DisposeItWith(Disposable);
        StatusText = new BindableReactiveProperty<string>().DisposeItWith(Disposable);
        VibrationX = new BindableReactiveProperty<float>().DisposeItWith(Disposable);
        VibrationY = new BindableReactiveProperty<float>().DisposeItWith(Disposable);
        VibrationZ = new BindableReactiveProperty<float>().DisposeItWith(Disposable);
        Clipping0 = new BindableReactiveProperty<uint>().DisposeItWith(Disposable);
        Clipping1 = new BindableReactiveProperty<uint>().DisposeItWith(Disposable);
        Clipping2 = new BindableReactiveProperty<uint>().DisposeItWith(Disposable);
        ArmedTime = new BindableReactiveProperty<TimeSpan>().DisposeItWith(Disposable);

        BatteryConsumedSymbol = BatteryConsumed
            .Unit.CurrentUnitItem.Select(item => item.Symbol)
            .ToBindableReactiveProperty<string>()
            .DisposeItWith(Disposable);
        BatteryAmperageSymbol = BatteryAmperage
            .Unit.CurrentUnitItem.Select(item => item.Symbol)
            .ToBindableReactiveProperty<string>()
            .DisposeItWith(Disposable);
        BatteryChargeSymbol = BatteryCharge
            .Unit.CurrentUnitItem.Select(item => item.Symbol)
            .ToBindableReactiveProperty<string>()
            .DisposeItWith(Disposable);
        BatteryVoltageSymbol = BatteryVoltage
            .Unit.CurrentUnitItem.Select(item => item.Symbol)
            .ToBindableReactiveProperty<string>()
            .DisposeItWith(Disposable);
        LinkQualitySymbol = LinkQuality
            .Unit.CurrentUnitItem.Select(item => item.Symbol)
            .ToBindableReactiveProperty<string>()
            .DisposeItWith(Disposable);
        VelocitySymbol = Velocity
            .Unit.CurrentUnitItem.Select(item => item.Symbol)
            .ToBindableReactiveProperty<string>()
            .DisposeItWith(Disposable);
        AltitudeSymbol = _altitudeUnit
            .CurrentUnitItem.Select(item => item.Symbol)
            .ToBindableReactiveProperty<string>()
            .DisposeItWith(Disposable);
        AzimuthSymbol = Azimuth
            .Unit.CurrentUnitItem.Select(item => item.Symbol)
            .ToBindableReactiveProperty<string>()
            .DisposeItWith(Disposable);
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
        Dispatcher.UIThread.InvokeAsync(() =>
        {
            GnssStatusBrush = new SolidColorBrush();
            LinkQualityStatusBrush = new SolidColorBrush();
            BatteryStatusBrush = new SolidColorBrush();
            AltitudeStatusBrush = new SolidColorBrush();
        });
        Device = device;
        Position = WorkspaceDock.Left;
        Icon = dev.GetIcon(device.Id);
        IconBrush = dev.GetDeviceBrush(device.Id);
        _altitudeUnit = unitService.Units[AltitudeBase.Id];
        _velocityUnit = unitService.Units[VelocityBase.Id];
        _angleUnit = unitService.Units[AngleBase.Id];
        _capacityUnit = unitService.Units[CapacityBase.Id];
        _amperageUnit = unitService.Units[AmperageBase.Id];
        _voltageUnit = unitService.Units[VoltageBase.Id];
        _progressUnit = unitService.Units[ProgressBase.Id];
        device.Name.Subscribe(x => Header = x).DisposeItWith(Disposable);
        InitArgs(device.Id.AsString());
        MissionProgress = new MissionProgressViewModel(
            device,
            unitService,
            loggerFactory,
            this
        ).DisposeItWith(Disposable);
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

        var linkQuality = new ReactiveProperty<double>().DisposeItWith(Disposable);
        var altitudeAgl = new ReactiveProperty<double>().DisposeItWith(Disposable);
        var altitudeMsl = new ReactiveProperty<double>().DisposeItWith(Disposable);
        var heading = new ReactiveProperty<double>().DisposeItWith(Disposable);
        var azimuth = new ReactiveProperty<double>().DisposeItWith(Disposable);
        var homeAzimuth = new ReactiveProperty<double>().DisposeItWith(Disposable);
        var velocity = new ReactiveProperty<double>().DisposeItWith(Disposable);
        var batteryAmperage = new ReactiveProperty<double>().DisposeItWith(Disposable);
        var batteryVoltage = new ReactiveProperty<double>().DisposeItWith(Disposable);
        var batteryCharge = new ReactiveProperty<double>().DisposeItWith(Disposable);
        var batteryConsumed = new ReactiveProperty<double>().DisposeItWith(Disposable);

        heartbeatClient.LinkQuality.Subscribe(d => linkQuality.Value = d).DisposeItWith(Disposable);
        positionClientEx
            .Base.GlobalPosition.Subscribe(pld =>
            {
                altitudeAgl.Value = Math.Truncate((pld?.RelativeAlt ?? double.NaN) / 1000d);
                altitudeMsl.Value = Math.Truncate((pld?.Alt ?? double.NaN) / 1000d);
            })
            .DisposeItWith(Disposable);
        positionClientEx
            .Yaw.Subscribe(d =>
            {
                azimuth.Value = Math.Round(d, 2);
                heading.Value = Math.Truncate(d);
            })
            .DisposeItWith(Disposable);
        positionClientEx
            .Current.Where(_ => positionClientEx.Home.CurrentValue.HasValue)
            .ThrottleLast(TimeSpan.FromMilliseconds(200))
            .Subscribe(p =>
                homeAzimuth.Value = p.Azimuth(positionClientEx.Home.CurrentValue ?? GeoPoint.NaN)
            )
            .DisposeItWith(Disposable);
        _gnssClient
            .Main.GroundVelocity.Subscribe(d =>
            {
                velocity.Value = Math.Truncate(d);
                if (positionClientEx.Base.GlobalPosition.CurrentValue != null)
                {
                    SpeedAltitudeCheck(
                            positionClientEx.Base.GlobalPosition.CurrentValue.RelativeAlt / 1000,
                            Math.Round(_gnssClient.Main.GroundVelocity.CurrentValue)
                        )
                        .SafeFireAndForget(ex => Logger.LogError(ex, "Velocity error"));
                }
            })
            .DisposeItWith(Disposable);
        telemetryClient
            .BatteryCurrent.Subscribe(d => batteryAmperage.Value = d)
            .DisposeItWith(Disposable);
        telemetryClient
            .BatteryCharge.Subscribe(d =>
            {
                batteryCharge.Value = d;
                BatteryStatus(d * 100)
                    .SafeFireAndForget(ex => Logger.LogError(ex, "Battery charge error"));
            })
            .DisposeItWith(Disposable);
        batteryAmperage
            .Subscribe(_ =>
            {
                batteryConsumed.Value =
                    telemetryClient.BatteryCurrent.CurrentValue == 0
                        ? double.NaN
                        : Math.Round(
                            telemetryClient.BatteryCurrent.CurrentValue
                                * positionClientEx.ArmedTime.CurrentValue.TotalHours,
                            2
                        );
            })
            .DisposeItWith(Disposable);
        _gnssClient
            .Main.Info.Subscribe(_ =>
                GnssStatus().SafeFireAndForget(ex => Logger.LogError(ex, "Gnss status error"))
            )
            .DisposeItWith(Disposable);

        LinkQuality = new HistoricalUnitProperty(
            nameof(LinkQuality),
            linkQuality,
            _progressUnit,
            loggerFactory,
            this
        ).DisposeItWith(Disposable);
        AltitudeAgl = new HistoricalUnitProperty(
            nameof(AltitudeAgl),
            altitudeAgl,
            _altitudeUnit,
            loggerFactory,
            this
        ).DisposeItWith(Disposable);
        AltitudeMsl = new HistoricalUnitProperty(
            nameof(AltitudeMsl),
            altitudeMsl,
            _altitudeUnit,
            loggerFactory,
            this
        ).DisposeItWith(Disposable);
        Azimuth = new HistoricalUnitProperty(
            nameof(Azimuth),
            azimuth,
            _angleUnit,
            loggerFactory,
            this
        ).DisposeItWith(Disposable);
        Heading = new HistoricalUnitProperty(
            nameof(Heading),
            heading,
            _angleUnit,
            loggerFactory,
            this
        ).DisposeItWith(Disposable);
        HomeAzimuth = new HistoricalUnitProperty(
            nameof(HomeAzimuth),
            homeAzimuth,
            _angleUnit,
            loggerFactory,
            this
        ).DisposeItWith(Disposable);
        Velocity = new HistoricalUnitProperty(
            nameof(Velocity),
            velocity,
            _velocityUnit,
            loggerFactory,
            this
        ).DisposeItWith(Disposable);
        BatteryAmperage = new HistoricalUnitProperty(
            nameof(BatteryAmperage),
            batteryAmperage,
            _amperageUnit,
            loggerFactory,
            this,
            "N2"
        ).DisposeItWith(Disposable);
        BatteryVoltage = new HistoricalUnitProperty(
            nameof(BatteryVoltage),
            batteryVoltage,
            _voltageUnit,
            loggerFactory,
            this,
            "N2"
        ).DisposeItWith(Disposable);
        BatteryCharge = new HistoricalUnitProperty(
            nameof(BatteryCharge),
            batteryCharge,
            _progressUnit,
            loggerFactory,
            this
        ).DisposeItWith(Disposable);
        BatteryConsumed = new HistoricalUnitProperty(
            nameof(BatteryConsumed),
            batteryConsumed,
            _capacityUnit,
            loggerFactory,
            this
        ).DisposeItWith(Disposable);

        BatteryConsumedSymbol = BatteryConsumed
            .Unit.CurrentUnitItem.Select(item => item.Symbol)
            .ToBindableReactiveProperty<string>()
            .DisposeItWith(Disposable);
        BatteryAmperageSymbol = BatteryAmperage
            .Unit.CurrentUnitItem.Select(item => item.Symbol)
            .ToBindableReactiveProperty<string>()
            .DisposeItWith(Disposable);
        BatteryChargeSymbol = BatteryCharge
            .Unit.CurrentUnitItem.Select(item => item.Symbol)
            .ToBindableReactiveProperty<string>()
            .DisposeItWith(Disposable);
        BatteryVoltageSymbol = BatteryVoltage
            .Unit.CurrentUnitItem.Select(item => item.Symbol)
            .ToBindableReactiveProperty<string>()
            .DisposeItWith(Disposable);
        LinkQualitySymbol = LinkQuality
            .Unit.CurrentUnitItem.Select(item => item.Symbol)
            .ToBindableReactiveProperty<string>()
            .DisposeItWith(Disposable);
        VelocitySymbol = Velocity
            .Unit.CurrentUnitItem.Select(item => item.Symbol)
            .ToBindableReactiveProperty<string>()
            .DisposeItWith(Disposable);
        AltitudeSymbol = _altitudeUnit
            .CurrentUnitItem.Select(item => item.Symbol)
            .ToBindableReactiveProperty<string>()
            .DisposeItWith(Disposable);
        AzimuthSymbol = Azimuth
            .Unit.CurrentUnitItem.Select(item => item.Symbol)
            .ToBindableReactiveProperty<string>()
            .DisposeItWith(Disposable);

        LinkState = heartbeatClient
            .Link.State.Select(state =>
            {
                LinkStatus(state).SafeFireAndForget(ex => Logger.LogError(ex, "Link status error"));
                return state.ToString();
            })
            .ToBindableReactiveProperty<string>()
            .DisposeItWith(Disposable);
        CurrentFlightMode = modeClient
            .CurrentMode.Select(mode => mode.Name)
            .ToBindableReactiveProperty<string>()
            .DisposeItWith(Disposable);
        Roll = positionClientEx.Roll.ToBindableReactiveProperty().DisposeItWith(Disposable);
        Pitch = positionClientEx.Pitch.ToBindableReactiveProperty().DisposeItWith(Disposable);
        VdopCount = _gnssClient
            .Main.Info.Select(info => info.Vdop is null ? RS.NotANumber : $"{info.Vdop.Value} VDOP")
            .ToBindableReactiveProperty<string>()
            .DisposeItWith(Disposable);
        HdopCount = _gnssClient
            .Main.Info.Select(info => info.Hdop is null ? RS.NotANumber : $"{info.Hdop.Value} HDOP")
            .ToBindableReactiveProperty<string>()
            .DisposeItWith(Disposable);
        SatelliteCount = _gnssClient
            .Main.Info.Select(info => info.SatellitesVisible)
            .ToBindableReactiveProperty()
            .DisposeItWith(Disposable);
        RtkMode = _gnssClient
            .Main.Info.Select(gpsInfo => GpsFixTypeToString(gpsInfo.FixType))
            .ToBindableReactiveProperty<string>()
            .DisposeItWith(Disposable);
        IsArmed = positionClientEx
            .IsArmed.DistinctUntilChanged()
            .Select(b => b)
            .ToBindableReactiveProperty()
            .DisposeItWith(Disposable);
        StatusText = positionClientEx
            .IsArmed.Select(b =>
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
    }

    private async Task BatteryStatus(double percent)
    {
        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            BatteryStatusBrush.Color = percent switch
            {
                > 70 => GreenColor,
                > 50 => YellowColor,
                > 40 => OrangeColor,
                < 30 => RedColor,
                _ => BatteryStatusBrush.Color,
            };
        });
    }

    private async Task LinkStatus(LinkState state)
    {
        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            LinkQualityStatusBrush.Color = state switch
            {
                Common.LinkState.Connected => GreenColor,
                Common.LinkState.Disconnected => RedColor,
                Common.LinkState.Downgrade => OrangeColor,
                _ => LinkQualityStatusBrush.Color,
            };
        });
    }

    private async Task SpeedAltitudeCheck(int alt, double gs)
    {
        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            if (gs > DangerHighSpeed && alt < CriticalAltitude)
            {
                StatusText.Value = RS.UavWidgetViewModel_StatusText_PullUp;
                AltitudeStatusBrush.Color = YellowColor;
            }
            else
            {
                StatusText.Value = string.Empty;
                AltitudeStatusBrush.Color = GreenColor;
            }
        });
    }

    private async Task GnssStatus()
    {
        await Dispatcher.UIThread.InvokeAsync(() =>
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
                GnssStatusBrush.Color = OrangeColor;
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
                GnssStatusBrush.Color = RedColor;
                return;
            }

            GnssStatusBrush.Color = GreenColor;
        });
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

    public override IEnumerable<IRoutable> GetRoutableChildren()
    {
        yield return LinkQuality;
        yield return AltitudeAgl;
        yield return AltitudeMsl;
        yield return Azimuth;
        yield return Heading;
        yield return HomeAzimuth;
        yield return Velocity;
        yield return BatteryAmperage;
        yield return BatteryVoltage;
        yield return BatteryCharge;
        yield return BatteryConsumed;
        yield return MissionProgress;
    }

    protected override void AfterLoadExtensions()
    {
        // do nothing
    }

    public ReactiveCommand TakeOff { get; }
    public ICommand AutoMode { get; }
    public ICommand Rtl { get; }
    public ICommand Land { get; }
    public ICommand Guided { get; }
    public ICommand StartMission { get; }

    public MissionProgressViewModel MissionProgress { get; }

    #region BatteryRtt

    public HistoricalUnitProperty BatteryConsumed { get; }
    public HistoricalUnitProperty BatteryAmperage { get; }
    public HistoricalUnitProperty BatteryCharge { get; }
    public HistoricalUnitProperty BatteryVoltage { get; }
    public BindableReactiveProperty<string> BatteryConsumedSymbol { get; }
    public BindableReactiveProperty<string> BatteryAmperageSymbol { get; }
    public BindableReactiveProperty<string> BatteryChargeSymbol { get; }
    public BindableReactiveProperty<string> BatteryVoltageSymbol { get; }

    public SolidColorBrush BatteryStatusBrush
    {
        get;
        set => SetField(ref field, value);
    }

    #endregion

    #region Gnss

    public BindableReactiveProperty<int> SatelliteCount { get; }
    public BindableReactiveProperty<string> HdopCount { get; }
    public BindableReactiveProperty<string> VdopCount { get; }
    public BindableReactiveProperty<string> RtkMode { get; }

    public SolidColorBrush GnssStatusBrush
    {
        get;
        set => SetField(ref field, value);
    }

    #endregion

    public BindableReactiveProperty<string> LinkState { get; }
    public HistoricalUnitProperty LinkQuality { get; }
    public BindableReactiveProperty<string> LinkQualitySymbol { get; }

    public SolidColorBrush AltitudeStatusBrush
    {
        get;
        set => SetField(ref field, value);
    }

    public SolidColorBrush LinkQualityStatusBrush
    {
        get;
        set => SetField(ref field, value);
    }

    public BindableReactiveProperty<string> CurrentFlightMode { get; }
    public BindableReactiveProperty<float> VibrationX { get; }
    public BindableReactiveProperty<float> VibrationY { get; }
    public BindableReactiveProperty<float> VibrationZ { get; }
    public BindableReactiveProperty<uint> Clipping0 { get; }
    public BindableReactiveProperty<uint> Clipping1 { get; }
    public BindableReactiveProperty<uint> Clipping2 { get; }
    public BindableReactiveProperty<double> Roll { get; }
    public BindableReactiveProperty<double> Pitch { get; }
    public HistoricalUnitProperty Velocity { get; }
    public HistoricalUnitProperty AltitudeAgl { get; }
    public HistoricalUnitProperty AltitudeMsl { get; }
    public HistoricalUnitProperty Heading { get; }
    public HistoricalUnitProperty HomeAzimuth { get; }
    public HistoricalUnitProperty Azimuth { get; }
    public BindableReactiveProperty<string> VelocitySymbol { get; }
    public BindableReactiveProperty<string> AltitudeSymbol { get; }
    public BindableReactiveProperty<string> AzimuthSymbol { get; }
    public BindableReactiveProperty<string> StatusText { get; }
    public BindableReactiveProperty<bool> IsArmed { get; }
    public BindableReactiveProperty<TimeSpan> ArmedTime { get; }

    public IClientDevice Device { get; }

    public WorkspaceDock Position
    {
        get => _position;
        private init => SetField(ref _position, value);
    }
}
