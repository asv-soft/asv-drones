using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Asv.Avalonia;
using Asv.Avalonia.GeoMap;
using Asv.Avalonia.IO;
using Asv.Common;
using Asv.IO;
using Asv.Mavlink;
using Microsoft.Extensions.Logging;
using R3;

namespace Asv.Drones;

public class TelemetryViewModel : RoutableViewModel, IDashboardWidget
{
    private const string WidgetId = "telemetry-widget-dashboard-item";

    private readonly IDeviceManager _deviceManager;
    private readonly ILoggerFactory _loggerFactory;
    private readonly IUnitService _unitService;
    private const AsvColorKind DefaultStatusColor = AsvColorKind.Info5;

    public TelemetryViewModel()
        : base(WidgetId, DesignTime.LoggerFactory)
    {
        DesignTime.ThrowIfNotDesignMode();

        InitArgs("1");
    }

    public TelemetryViewModel(
        IDeviceManager deviceManager,
        ILoggerFactory loggerFactory,
        IUnitService unitService
    )
        : base(WidgetId, loggerFactory)
    {
        _deviceManager = deviceManager;
        _loggerFactory = loggerFactory;
        _unitService = unitService;
    }

    public AltitudeUavIndicatorViewModel AltitudeUavIndicator { get; private set; }
    public BatteryUavIndicatorViewModel BatteryUavIndicator { get; private set; }
    public VelocityUavIndicatorViewModel VelocityUavIndicator { get; private set; }
    public AngleUavRttIndicatorViewModel AngleUavRttIndicator { get; private set; }

    public override IEnumerable<IRoutable> GetChildren()
    {
        yield return AltitudeUavIndicator;
        yield return BatteryUavIndicator;
        yield return VelocityUavIndicator;
        yield return AngleUavRttIndicator;
    }

    public DeviceId DeviceId { get; }

    public void Attach(DeviceId deviceId)
    {
        if (deviceId == null)
        {
            return;
        }

        InitArgs(deviceId.AsString());

        var device = _deviceManager.Explorer.Devices[deviceId];

        var positionClientEx =
            device.GetMicroservice<IPositionClientEx>()
            ?? throw new ArgumentException(
                $"Unable to load {nameof(PositionClientEx)} from {device.Id}"
            );

        var gnssClient =
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
        var modeClient =
            device.GetMicroservice<IModeClient>()
            ?? throw new ArgumentException($"Unable to load {nameof(IModeClient)}");

        var altitudeAgl = new ReactiveProperty<double>(10).DisposeItWith(Disposable);
        var altitudeMsl = new ReactiveProperty<double>(14).DisposeItWith(Disposable);

        var velocity = new ReactiveProperty<double>(199).DisposeItWith(Disposable);
        var batteryAmperage = new ReactiveProperty<double>(39).DisposeItWith(Disposable);
        var batteryVoltage = new ReactiveProperty<double>(34).DisposeItWith(Disposable);
        var batteryCharge = new ReactiveProperty<double>(123).DisposeItWith(Disposable);
        var batteryConsumed = new ReactiveProperty<double>(39).DisposeItWith(Disposable);

        var roll = new ReactiveProperty<double>(10).DisposeItWith(Disposable);
        var pitch = new ReactiveProperty<double>(30).DisposeItWith(Disposable);

        positionClientEx
            .Base.GlobalPosition.ObserveOnUIThreadDispatcher()
            .Select(pld => Math.Truncate((pld?.RelativeAlt ?? double.NaN) / 1000d))
            .Subscribe(x => altitudeAgl.Value = x)
            .DisposeItWith(Disposable);

        positionClientEx
            .Base.GlobalPosition.ObserveOnUIThreadDispatcher()
            .Select(pld => Math.Truncate((pld?.Alt ?? double.NaN) / 1000d))
            .Subscribe(x => altitudeMsl.Value = x)
            .DisposeItWith(Disposable);

        gnssClient
            .Main.GroundVelocity.ObserveOnUIThreadDispatcher()
            .Select(Math.Truncate)
            .Subscribe(x => velocity.Value = x)
            .DisposeItWith(Disposable);

        telemetryClient
            .BatteryCurrent.ObserveOnUIThreadDispatcher()
            .Subscribe(x => batteryAmperage.Value = x)
            .DisposeItWith(Disposable);

        telemetryClient
            .BatteryVoltage.ObserveOnUIThreadDispatcher()
            .Subscribe(x => batteryVoltage.Value = x)
            .DisposeItWith(Disposable);

        telemetryClient
            .BatteryCharge.ObserveOnUIThreadDispatcher()
            .Subscribe(x => batteryCharge.Value = x)
            .DisposeItWith(Disposable);

        telemetryClient
            .BatteryCurrent.ObserveOnUIThreadDispatcher()
            .Select(d =>
                d == 0
                    ? double.NaN
                    : Math.Round(d * positionClientEx.ArmedTime.CurrentValue.TotalHours, 2)
            )
            .Subscribe(x => batteryConsumed.Value = x)
            .DisposeItWith(Disposable);

        positionClientEx.Roll.Subscribe(x => roll.Value = x).DisposeItWith(Disposable);
        positionClientEx.Pitch.Subscribe(x => pitch.Value = x).DisposeItWith(Disposable);

        var altitudeUnit = _unitService.Units[AltitudeUnit.Id];
        var capacityUnit = _unitService.Units[CapacityUnit.Id];
        var amperageUnit = _unitService.Units[AmperageUnit.Id];
        var voltageUnit = _unitService.Units[VoltageUnit.Id];
        var progressUnit = _unitService.Units[ProgressUnit.Id];
        var angleUnit = _unitService.Units[AngleUnit.Id];

        AltitudeUavIndicator = new AltitudeUavIndicatorViewModel(
            nameof(AltitudeUavIndicator),
            _loggerFactory,
            altitudeAgl,
            altitudeMsl,
            altitudeUnit.CurrentUnitItem,
            DefaultStatusColor
        )
            .SetRoutableParent(this)
            .DisposeItWith(Disposable);

        BatteryUavIndicator = new BatteryUavIndicatorViewModel(
            nameof(BatteryUavIndicator),
            _loggerFactory,
            batteryCharge,
            batteryAmperage,
            batteryVoltage,
            batteryConsumed,
            progressUnit.CurrentUnitItem,
            amperageUnit.CurrentUnitItem,
            capacityUnit.CurrentUnitItem,
            voltageUnit.CurrentUnitItem,
            DefaultStatusColor
        )
            .SetRoutableParent(this)
            .DisposeItWith(Disposable);

        VelocityUavIndicator = new VelocityUavIndicatorViewModel(
            nameof(VelocityUavIndicator),
            _loggerFactory,
            _unitService,
            velocity,
            DefaultStatusColor
        )
            .SetRoutableParent(this)
            .DisposeItWith(Disposable);

        AngleUavRttIndicator = new AngleUavRttIndicatorViewModel(
            nameof(AngleUavRttIndicator),
            _loggerFactory,
            pitch,
            roll,
            angleUnit.CurrentUnitItem,
            DefaultStatusColor
        )
            .SetRoutableParent(this)
            .DisposeItWith(Disposable);
    }
}
