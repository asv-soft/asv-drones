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
        BatteryConsumed.Value = "0.04mAh";
        BatteryAmperage.Value = "2A";
        BatteryVoltage.Value = "12V";
        BatteryCharge.Value = "10%";
        GnssStatusBrush.Color = OrangeColor;
        SatelliteCount.Value = 10;
        RtkMode.Value = "RTK Fixed";
        Velocity.Value = "12";
        AltitudeMsl.Value = "1200m";
        AltitudeAgl.Value = "200m";
        CurrentFlightMode.Value = "Auto";
        LinkQuality.Value = "100%";
        LinkState.Value = "Connected";
        LinkQualityStatusBrush.Color = GreenColor;
        GnssStatusBrush.Color = GreenColor;
        HomeAzimuth.Value = -115;
        MissionProgress.Value = new MissionProgressViewModel();
        MissionProgress.Value.PathProgress.Value = 0.7;
        MissionProgress.Value.IsDownloaded.Value = true;
        MissionProgress.Value.MissionDistance.Value = "1000m";
        MissionProgress.Value.MissionFlightTime.Value = "15 min";
        MissionProgress.Value.TotalDistance.Value = "1200m";
        MissionProgress.Value.TargetDistance.Value = "300m";
        MissionProgress.Value.HomeDistance.Value = "100m";
        MissionProgress.Value.PathProgress.Value = 0.7;
        Azimuth.Value = "80";
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
        AltitudeUnit.Value = unitService.Units[AltitudeBase.Id];
        VelocityUnit.Value = unitService.Units[VelocityBase.Id];
        AngleUnit.Value = unitService.Units[AngleBase.Id];
        BearingUnit.Value = unitService.Units[BearingBase.Id];
        CapacityUnit.Value = unitService.Units[CapacityBase.Id];

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
                $"Unable to load {nameof(PositionClientEx)} from {device.Id}"
            );
        var telemetryClient =
            device.GetMicroservice<TelemetryClientEx>()
            ?? throw new ArgumentException(
                $"Unable to load {nameof(PositionClientEx)} from {device.Id}"
            );
        var heartbeatClient =
            device.GetMicroservice<HeartbeatClient>()
            ?? throw new ArgumentException(
                $"Unable to load {nameof(PositionClientEx)} from {device.Id}"
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
        LinkQuality = heartbeatClient
            .LinkQuality.Select(x => $"{(int)x * 100} %")
            .ToBindableReactiveProperty<string>();

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
        AltitudeAgl = positionClientEx
            .Base.GlobalPosition.Select(payload =>
                payload?.RelativeAlt is not null
                    ? AltitudeUnit.Value.Current.Value.Print(
                        Math.Truncate(payload.RelativeAlt / 1000d)
                    )
                    : RS.Not_Available
            )
            .ToBindableReactiveProperty<string>();
        AltitudeMsl = positionClientEx
            .Base.GlobalPosition.Select(payload =>
                payload?.Alt is not null
                    ? AltitudeUnit.Value.Current.Value.Print(Math.Truncate(payload.Alt / 1000d))
                    : RS.Not_Available
            )
            .ToBindableReactiveProperty<string>();
        Roll = positionClientEx.Roll.ToBindableReactiveProperty();
        Pitch = positionClientEx.Roll.ToBindableReactiveProperty();
        Heading = positionClientEx.Yaw.ToBindableReactiveProperty();
        Heading
            .Subscribe(_ =>
                Azimuth.Value = AngleUnit.Value.Current.Value.Print(Heading.Value, "N2")
            )
            .DisposeItWith(Disposable);

        Velocity = _gnssClient
            .Main.GroundVelocity.Select(d =>
                VelocityUnit.Value.Current.Value.PrintFromSi(Math.Truncate(d))
            )
            .ToBindableReactiveProperty<string>();
        Velocity
            .Subscribe(_ =>
            {
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
        VdopCount = _gnssClient
            .Main.Info.Select(info =>
                info.Vdop is null ? RS.Not_Available : $"{info.Vdop.Value} VDOP"
            )
            .ToBindableReactiveProperty<string>();
        HdopCount = _gnssClient
            .Main.Info.Select(info =>
                info.Hdop is null ? RS.Not_Available : $"{info.Hdop.Value} HDOP"
            )
            .ToBindableReactiveProperty<string>();
        _gnssClient
            .Main.Info.Subscribe(_ =>
                GnssStatus().SafeFireAndForget(ex => _log.LogError(ex, "Gnss status error"))
            )
            .DisposeItWith(Disposable);
        RtkMode = _gnssClient
            .Main.Info.Select(gpsInfo => GpsFixTypeToString(gpsInfo.FixType))
            .ToBindableReactiveProperty<string>();
        IsArmed = positionClientEx
            .IsArmed.DistinctUntilChanged()
            .Select(b => b)
            .ToBindableReactiveProperty();
        BatteryAmperage = telemetryClient
            .BatteryCurrent.Select(d => $"{(int)d} A")
            .ToBindableReactiveProperty<string>();
        BatteryVoltage = telemetryClient
            .BatteryVoltage.Select(d => $"{(int)d} V")
            .ToBindableReactiveProperty<string>();
        BatteryCharge = telemetryClient
            .BatteryCharge.Select(d => $"{(int)(d * 100)} %")
            .ToBindableReactiveProperty<string>();
        BatteryCharge.Subscribe(_ =>
            BatteryStatus(telemetryClient.BatteryCharge.CurrentValue * 100)
                .SafeFireAndForget(ex => _log.LogError(ex, "Battery charge error"))
        );
        SatelliteCount = _gnssClient
            .Main.Info.Select(info => info.SatellitesVisible)
            .ToBindableReactiveProperty();
        BatteryAmperage
            .Subscribe(_ =>
            {
                BatteryConsumed.Value =
                    telemetryClient.BatteryCurrent.CurrentValue == 0
                        ? RS.Not_Available
                        : $"{CapacityUnit.CurrentValue.Current.Value.PrintWithUnits(telemetryClient.BatteryCurrent.CurrentValue * positionClientEx.ArmedTime.CurrentValue.TotalHours, "N2")}";
            })
            .DisposeItWith(Disposable);

        StatusText = positionClientEx
            .IsArmed.Select(_ =>
                _
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

    public BindableReactiveProperty<string> BatteryConsumed { get; } = new();
    public BindableReactiveProperty<string> BatteryAmperage { get; } = new();
    public BindableReactiveProperty<string> BatteryCharge { get; } = new();
    public BindableReactiveProperty<string> BatteryVoltage { get; } = new();

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
    public BindableReactiveProperty<string> LinkQuality { get; } = new();

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
    public BindableReactiveProperty<string> Velocity { get; } = new();
    public BindableReactiveProperty<string> AltitudeAgl { get; } = new();
    public BindableReactiveProperty<string> AltitudeMsl { get; } = new();
    public BindableReactiveProperty<double> Heading { get; } = new();
    public BindableReactiveProperty<int> HomeAzimuth { get; } = new();
    public BindableReactiveProperty<string> Azimuth { get; } = new();
    public BindableReactiveProperty<string> StatusText { get; } = new();
    public BindableReactiveProperty<bool> IsArmed { get; } = new();
    public BindableReactiveProperty<TimeSpan> ArmedTime { get; } = new();
    public BindableReactiveProperty<IUnit> VelocityUnit { get; } = new();
    public BindableReactiveProperty<IUnit> AltitudeUnit { get; } = new();
    public BindableReactiveProperty<IUnit> BearingUnit { get; } = new();
    public BindableReactiveProperty<IUnit> AngleUnit { get; } = new();
    public BindableReactiveProperty<IUnit> CapacityUnit { get; } = new();

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
            Azimuth.Dispose();
            RtkMode.Dispose();
            Heading.Dispose();
            Velocity.Dispose();
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
            LinkQuality.Dispose();
            BearingUnit.Dispose();
            AltitudeAgl.Dispose();
            AltitudeMsl.Dispose();
            HomeAzimuth.Dispose();
            VelocityUnit.Dispose();
            AltitudeUnit.Dispose();
            CapacityUnit.Dispose();
            BatteryCharge.Dispose();
            BatteryVoltage.Dispose();
            SatelliteCount.Dispose();
            MissionProgress.Dispose();
            BatteryConsumed.Dispose();
            BatteryAmperage.Dispose();
            CurrentFlightMode.Dispose();
        }

        base.Dispose(disposing);
    }

    #endregion
}
