using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Asv.Avalonia;
using Asv.Common;
using Asv.Drones.Api;
using Asv.IO;
using Asv.Mavlink;
using Avalonia.Media;
using Avalonia.Threading;
using Microsoft.Extensions.Logging;
using R3;
using Observable = R3.Observable;

namespace Asv.Drones;

public class UavWidgetViewModel : ExtendableHeadlinedViewModel<IUavFlightWidget>, IUavFlightWidget
{
    private WorkspaceDock _position;

    private readonly ILogger _log;
    private readonly GnssClientEx? _gnssClient;
    private const string WidgetId = "widget-uav";
    private const int CriticalAltitude = 40;
    private const int DangerHighSpeed = 10;
    private const int DangerSatelliteCount = 10;
    private static readonly Range WarningSatelliteAmount = 15..20;

    private static readonly Color GreenColor = Color.Parse("#21c088");
    private static readonly Color OrangeColor = Color.Parse("#e48f4d");
    private static readonly Color RedColor = Color.Parse("#cc5058");
    private static readonly Color YellowColor = Color.Parse("#dfc34a");

    public UavWidgetViewModel()
        : base(SystemModule.Name)
    {
        DesignTime.ThrowIfNotDesignMode();
        InitArgs("1");
        AltitudeStatusBrush.Color = GreenColor;
        BatteryStatusBrush.Color = RedColor;
        GnssStatusBrush.Color = OrangeColor;
        SatelliteCount.Value = 10;
        RtkMode.Value = "RTK Fixed";
        CurrentFlightMode.Value = "Auto";
        LinkState.Value = "Connected";
        LinkQualityStatusBrush.Color = GreenColor;
        GnssStatusBrush.Color = GreenColor;
        MissionProgress.Value = new MissionProgressViewModel();
        MissionProgress.Value.PathProgress.Value = 0.7;
        MissionProgress.Value.IsDownloaded.Value = true;
        MissionProgress.Value.MissionDistance.Value = "1000m";
        MissionProgress.Value.MissionFlightTime.Value = "15 min";
        MissionProgress.Value.TotalDistance.Value = "1200m";
        MissionProgress.Value.TargetDistance.Value = "300m";
        MissionProgress.Value.HomeDistance.Value = "100m";
        MissionProgress.Value.PathProgress.Value = 0.7;
    }

    public UavWidgetViewModel(
        IClientDevice device,
        INavigationService navigation,
        IUnitService unitService,
        ILoggerFactory loggerFactory
    )
        : base(WidgetId)
    {
        ArgumentNullException.ThrowIfNull(device);
        _log = loggerFactory.CreateLogger<UavWidgetViewModel>();
        GnssStatusBrush = new SolidColorBrush();
        LinkQualityStatusBrush = new SolidColorBrush();
        BatteryStatusBrush = new SolidColorBrush();
        AltitudeStatusBrush = new SolidColorBrush();
        Device = device;
        Position = WorkspaceDock.Left;
        Icon = DeviceIconMixin.GetIcon(device.Id);
        IconBrush = DeviceIconMixin.GetIconBrush(device.Id);
        AltitudeUnit = unitService.Units[AltitudeBase.Id];
        VelocityUnit = unitService.Units[VelocityBase.Id];
        AngleUnit = unitService.Units[AngleBase.Id];
        BearingUnit = unitService.Units[BearingBase.Id];
        CapacityUnit = unitService.Units[CapacityBase.Id];
        AmperageUnit = unitService.Units[AmperageBase.Id];
        VoltageUnit = unitService.Units[VoltageBase.Id];
        ProgressUnit = unitService.Units[ProgressBase.Id];
        device.Name.Subscribe(x => Header = x).DisposeItWith(Disposable);
        InitArgs(device.Id.AsString());
        MissionProgress.Value = new MissionProgressViewModel(
            WidgetId,
            device,
            unitService,
            loggerFactory
        )
        {
            Parent = this,
        };
        var positionClientEx =
            device.GetMicroservice<PositionClientEx>()
            ?? throw new ArgumentException(
                $"Unable to load {nameof(PositionClientEx)} from {device.Id}"
            );
        _gnssClient =
            device.GetMicroservice<GnssClientEx>()
            ?? throw new ArgumentException(
                $"Unable to load {nameof(GnssClientEx)} from {device.Id}"
            );
        var telemetryClient =
            device.GetMicroservice<TelemetryClientEx>()
            ?? throw new ArgumentException(
                $"Unable to load {nameof(TelemetryClientEx)} from {device.Id}"
            );
        var heartbeatClient =
            device.GetMicroservice<HeartbeatClient>()
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
            async (_, _) =>
            {
                using var dialog = new SetAltitudeDialogViewModel(navigation, unitService);
                var result = await dialog.ApplyDialog();
                if (result == ContentDialogResult.Primary)
                {
                    await this.ExecuteCommand(
                        TakeOffCommand.Id,
                        new DoubleCommandArg(dialog.AltitudeResult.Value)
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
                        .SafeFireAndForget(ex => _log.LogError(ex, "Velocity error"));
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
                    .SafeFireAndForget(ex => _log.LogError(ex, "Battery charge error"));
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
                GnssStatus().SafeFireAndForget(ex => _log.LogError(ex, "Gnss status error"))
            )
            .DisposeItWith(Disposable);

        LinkQuality = new HistoricalUnitProperty(
            $"{WidgetId}.{nameof(LinkQuality)}",
            linkQuality,
            ProgressUnit
        )
        {
            Parent = this,
        }.DisposeItWith(Disposable);
        AltitudeAgl = new HistoricalUnitProperty(
            $"{WidgetId}.{nameof(AltitudeAgl)}",
            altitudeAgl,
            AltitudeUnit
        )
        {
            Parent = this,
        }.DisposeItWith(Disposable);
        AltitudeMsl = new HistoricalUnitProperty(
            $"{WidgetId}.{nameof(AltitudeMsl)}",
            altitudeMsl,
            AltitudeUnit
        )
        {
            Parent = this,
        }.DisposeItWith(Disposable);
        Azimuth = new HistoricalUnitProperty($"{WidgetId}.{nameof(Azimuth)}", azimuth, AngleUnit)
        {
            Parent = this,
        }.DisposeItWith(Disposable);
        Heading = new HistoricalUnitProperty($"{WidgetId}.{nameof(Heading)}", heading, AngleUnit)
        {
            Parent = this,
        }.DisposeItWith(Disposable);
        HomeAzimuth = new HistoricalUnitProperty(
            $"{WidgetId}.{nameof(HomeAzimuth)}",
            homeAzimuth,
            AngleUnit
        )
        {
            Parent = this,
        }.DisposeItWith(Disposable);
        Velocity = new HistoricalUnitProperty(
            $"{WidgetId}.{nameof(Velocity)}",
            velocity,
            VelocityUnit
        )
        {
            Parent = this,
        }.DisposeItWith(Disposable);
        BatteryAmperage = new HistoricalUnitProperty(
            $"{WidgetId}.{nameof(BatteryAmperage)}",
            batteryAmperage,
            AmperageUnit,
            "N2"
        )
        {
            Parent = this,
        }.DisposeItWith(Disposable);
        BatteryVoltage = new HistoricalUnitProperty(
            $"{WidgetId}.{nameof(BatteryVoltage)}",
            batteryVoltage,
            VoltageUnit,
            "N2"
        )
        {
            Parent = this,
        }.DisposeItWith(Disposable);
        BatteryCharge = new HistoricalUnitProperty(
            $"{WidgetId}.{nameof(BatteryCharge)}",
            batteryCharge,
            ProgressUnit
        )
        {
            Parent = this,
        }.DisposeItWith(Disposable);
        BatteryConsumed = new HistoricalUnitProperty(
            $"{WidgetId}.{nameof(BatteryConsumed)}",
            batteryConsumed,
            CapacityUnit
        )
        {
            Parent = this,
        }.DisposeItWith(Disposable);

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
        AltitudeMslSymbol = AltitudeMsl
            .Unit.CurrentUnitItem.Select(item => item.Symbol)
            .ToBindableReactiveProperty<string>()
            .DisposeItWith(Disposable);
        AltitudeAglSymbol = AltitudeAgl
            .Unit.CurrentUnitItem.Select(item => item.Symbol)
            .ToBindableReactiveProperty<string>()
            .DisposeItWith(Disposable);
        HeadingSymbol = Heading
            .Unit.CurrentUnitItem.Select(item => item.Symbol)
            .ToBindableReactiveProperty<string>()
            .DisposeItWith(Disposable);
        HomeAzimuthSymbol = HomeAzimuth
            .Unit.CurrentUnitItem.Select(item => item.Symbol)
            .ToBindableReactiveProperty<string>()
            .DisposeItWith(Disposable);
        AzimuthSymbol = Azimuth
            .Unit.CurrentUnitItem.Select(item => item.Symbol)
            .ToBindableReactiveProperty<string>()
            .DisposeItWith(Disposable);

        LinkState = heartbeatClient
            .Link.State.Select(state =>
            {
                LinkStatus(state).SafeFireAndForget(ex => _log.LogError(ex, "Link status error"));
                return state.ToString();
            })
            .ToBindableReactiveProperty<string>();
        CurrentFlightMode = modeClient
            .CurrentMode.Select(mode => mode.Name)
            .ToBindableReactiveProperty<string>();
        Roll = positionClientEx.Roll.ToBindableReactiveProperty();
        Pitch = positionClientEx.Pitch.ToBindableReactiveProperty();
        VdopCount = _gnssClient
            .Main.Info.Select(info => info.Vdop is null ? RS.NotANumber : $"{info.Vdop.Value} VDOP")
            .ToBindableReactiveProperty<string>();
        HdopCount = _gnssClient
            .Main.Info.Select(info => info.Hdop is null ? RS.NotANumber : $"{info.Hdop.Value} HDOP")
            .ToBindableReactiveProperty<string>();
        SatelliteCount = _gnssClient
            .Main.Info.Select(info => info.SatellitesVisible)
            .ToBindableReactiveProperty();
        RtkMode = _gnssClient
            .Main.Info.Select(gpsInfo => GpsFixTypeToString(gpsInfo.FixType))
            .ToBindableReactiveProperty<string>();
        IsArmed = positionClientEx
            .IsArmed.DistinctUntilChanged()
            .Select(b => b)
            .ToBindableReactiveProperty();
        StatusText = positionClientEx
            .IsArmed.Select(b =>
                b
                    ? RS.UavWidgetViewModel_StatusText_Armed
                    : RS.UavWidgetViewModel_StatusText_DisArmed
            )
            .ToBindableReactiveProperty<string>();
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
        yield return MissionProgress.Value;
    }

    protected override void AfterLoadExtensions()
    {
        // do nothing
    }

    public ReactiveCommand TakeOff { get; }
    public BindableAsyncCommand AutoMode { get; }
    public BindableAsyncCommand Rtl { get; }
    public BindableAsyncCommand Land { get; }
    public BindableAsyncCommand Guided { get; }
    public BindableAsyncCommand StartMission { get; }

    public BindableReactiveProperty<MissionProgressViewModel> MissionProgress { get; } = new();

    #region BatteryRtt

    public HistoricalUnitProperty BatteryConsumed { get; }
    public HistoricalUnitProperty BatteryAmperage { get; }
    public HistoricalUnitProperty BatteryCharge { get; }
    public HistoricalUnitProperty BatteryVoltage { get; }
    public BindableReactiveProperty<string> BatteryConsumedSymbol { get; }
    public BindableReactiveProperty<string> BatteryAmperageSymbol { get; }
    public BindableReactiveProperty<string> BatteryChargeSymbol { get; }
    public BindableReactiveProperty<string> BatteryVoltageSymbol { get; }

    private SolidColorBrush _batteryStatusBrush;
    public SolidColorBrush BatteryStatusBrush
    {
        get => _batteryStatusBrush;
        set => SetField(ref _batteryStatusBrush, value);
    }
    #endregion

    #region Gnss

    public BindableReactiveProperty<int> SatelliteCount { get; } = new();
    public BindableReactiveProperty<string> HdopCount { get; } = new();
    public BindableReactiveProperty<string> VdopCount { get; } = new();
    public BindableReactiveProperty<string> RtkMode { get; } = new();

    private SolidColorBrush _gnssStatusBrush;

    public SolidColorBrush GnssStatusBrush
    {
        get => _gnssStatusBrush;
        set => SetField(ref _gnssStatusBrush, value);
    }
    #endregion

    public BindableReactiveProperty<string> LinkState { get; } = new();
    public HistoricalUnitProperty LinkQuality { get; }
    public BindableReactiveProperty<string> LinkQualitySymbol { get; }

    private SolidColorBrush _altitudeStatusBrush;
    public SolidColorBrush AltitudeStatusBrush
    {
        get => _altitudeStatusBrush;
        set => SetField(ref _altitudeStatusBrush, value);
    }

    private SolidColorBrush _linkQualityStatusBrush;
    public SolidColorBrush LinkQualityStatusBrush
    {
        get => _linkQualityStatusBrush;
        set => SetField(ref _linkQualityStatusBrush, value);
    }

    public BindableReactiveProperty<string> CurrentFlightMode { get; } = new();
    public BindableReactiveProperty<float> VibrationX { get; } = new();
    public BindableReactiveProperty<float> VibrationY { get; } = new();
    public BindableReactiveProperty<float> VibrationZ { get; } = new();
    public BindableReactiveProperty<uint> Clipping0 { get; } = new();
    public BindableReactiveProperty<uint> Clipping1 { get; } = new();
    public BindableReactiveProperty<uint> Clipping2 { get; } = new();
    public BindableReactiveProperty<double> Roll { get; } = new();
    public BindableReactiveProperty<double> Pitch { get; } = new();
    public HistoricalUnitProperty Velocity { get; }
    public HistoricalUnitProperty AltitudeAgl { get; }
    public HistoricalUnitProperty AltitudeMsl { get; }
    public HistoricalUnitProperty Heading { get; }
    public HistoricalUnitProperty HomeAzimuth { get; }
    public HistoricalUnitProperty Azimuth { get; }
    public BindableReactiveProperty<string> VelocitySymbol { get; }
    public BindableReactiveProperty<string> AltitudeAglSymbol { get; }
    public BindableReactiveProperty<string> AltitudeMslSymbol { get; }
    public BindableReactiveProperty<string> HeadingSymbol { get; }
    public BindableReactiveProperty<string> HomeAzimuthSymbol { get; }
    public BindableReactiveProperty<string> AzimuthSymbol { get; }
    public BindableReactiveProperty<string> StatusText { get; } = new();
    public BindableReactiveProperty<bool> IsArmed { get; } = new();
    public BindableReactiveProperty<TimeSpan> ArmedTime { get; } = new();
    public IUnit VelocityUnit { get; }
    public IUnit AltitudeUnit { get; }
    public IUnit BearingUnit { get; }
    public IUnit AngleUnit { get; }
    public IUnit CapacityUnit { get; }
    public IUnit AmperageUnit { get; }
    public IUnit VoltageUnit { get; }
    public IUnit ProgressUnit { get; }

    public IClientDevice Device { get; }

    public WorkspaceDock Position
    {
        get => _position;
        private init => SetField(ref _position, value);
    }

    #region Dispose

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            Roll.Dispose();
            Pitch.Dispose();
            IsArmed.Dispose();
            RtkMode.Dispose();
            AngleUnit.Dispose();
            LinkState.Dispose();
            VdopCount.Dispose();
            HdopCount.Dispose();
            Clipping0.Dispose();
            Clipping1.Dispose();
            Clipping2.Dispose();
            ArmedTime.Dispose();
            VibrationY.Dispose();
            VibrationZ.Dispose();
            VibrationX.Dispose();
            StatusText.Dispose();
            BearingUnit.Dispose();
            VelocityUnit.Dispose();
            AltitudeUnit.Dispose();
            CapacityUnit.Dispose();
            SatelliteCount.Dispose();
            MissionProgress.Dispose();
            CurrentFlightMode.Dispose();
        }

        base.Dispose(disposing);
    }

    #endregion
}
