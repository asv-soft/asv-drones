#nullable enable
using System;
using System.Composition;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using Asv.Cfg;
using Asv.Common;
using Asv.Drones.Gui.Api;
using Asv.Mavlink;
using Asv.Mavlink.V2.Common;
using Asv.Mavlink.V2.Minimal;
using DynamicData;
using DynamicData.Binding;
using ReactiveUI;

namespace Asv.Drones.Gui
{
    public class MavlinkDeviceServiceConfig
    {
        public MavlinkPortConfig[] Ports { get; set; } =
        {
            new() { ConnectionString = "tcp://127.0.0.1:5762", IsEnabled = true, Name = "SITL connection" },
            new() { ConnectionString = "tcp://172.16.0.1:7341", IsEnabled = true, Name = "Base station" },
        };

        public byte SystemId { get; set; } = 254;
        public byte ComponentId { get; set; } = 254;
        public int HeartbeatRateMs { get; set; } = 1000;
        public int DeviceHeartbeatTimeoutMs { get; set; } = 60_000;
        public VehicleClientConfig Vehicle { get; set; } = new();
        public GbsClientDeviceConfig Gbs { get; set; } = new();
        public SdrClientDeviceConfig Sdr { get; set; } = new();
        public AdsbClientDeviceConfig Adsb { get; set; } = new();
        public bool WrapToV2ExtensionEnabled { get; set; } = true;
        public RfsaClientDeviceConfig Rfsa { get; set; } = new();
    }

    [Export(typeof(IMavlinkDevicesService))]
    [Shared]
    public class MavlinkDevicesService : ServiceWithConfigBase<MavlinkDeviceServiceConfig>, IMavlinkDevicesService
    {
        private readonly IPacketSequenceCalculator _sequenceCalculator;
        private readonly ILogService _log;
        private readonly MavlinkRouter _mavlinkRouter;
        private readonly RxValue<byte> _systemId;
        private readonly RxValue<byte> _componentId;
        private readonly RxValue<TimeSpan> _heartBeatRate;
        private readonly RxValue<bool> _needReloadToApplyConfig = new(false);
        private readonly MavlinkDeviceBrowser _deviceBrowser;
        private readonly IObservableCache<string, ushort> _logNames;

        [ImportingConstructor]
        public MavlinkDevicesService(IConfiguration config, IPacketSequenceCalculator sequenceCalculator,
            ILogService log) : base(config)
        {
            _sequenceCalculator = sequenceCalculator ?? throw new ArgumentNullException(nameof(sequenceCalculator));
            _log = log ?? throw new ArgumentNullException(nameof(log));

            #region InitUriHost mavlink router

            _mavlinkRouter =
                new MavlinkRouter(MavlinkV2Connection.RegisterDefaultDialects,
                    publishScheduler: RxApp.MainThreadScheduler).DisposeItWith(Disposable);
            _mavlinkRouter.WrapToV2ExtensionEnabled = InternalGetConfig(s => s.WrapToV2ExtensionEnabled);
            foreach (var port in InternalGetConfig(s => s.Ports))
            {
                _mavlinkRouter.AddPort(port);
            }

            _mavlinkRouter.OnConfigChanged
                .Throttle(TimeSpan.FromSeconds(1))
                .Subscribe(_ => InternalSaveConfig(serviceConfig => serviceConfig.Ports = _mavlinkRouter.GetConfig()))
                .DisposeItWith(Disposable);

            #endregion

            #region InitUriHost mavlink heartbeat

            var serverIdentity = InternalGetConfig(s => new MavlinkIdentity(s.SystemId, s.ComponentId));
            var serverConfig = InternalGetConfig(s => new ServerDeviceConfig
                { Heartbeat = new MavlinkHeartbeatServerConfig { HeartbeatRateMs = s.HeartbeatRateMs } });
            var serverDevice =
                new ServerDevice(Router, sequenceCalculator, serverIdentity, serverConfig, Scheduler.Default)
                    .DisposeItWith(Disposable);
            serverDevice.Heartbeat.Set(p =>
            {
                p.Autopilot = MavAutopilot.MavAutopilotInvalid;
                p.BaseMode = 0;
                p.CustomMode = 0;
                p.MavlinkVersion = 3;
                p.SystemStatus = MavState.MavStateActive;
                p.Type = MavType.MavTypeGcs;
            });
            _systemId = new RxValue<byte>(serverIdentity.SystemId).DisposeItWith(Disposable);
            _systemId
                .Throttle(TimeSpan.FromSeconds(1))
                .DistinctUntilChanged()
                .Skip(1)
                .Do(_ => _needReloadToApplyConfig.Value = true)
                .Subscribe(b => InternalSaveConfig(cfg => cfg.SystemId = b))
                .DisposeItWith(Disposable);
            _componentId = new RxValue<byte>(serverIdentity.ComponentId).DisposeItWith(Disposable);
            _componentId
                .Throttle(TimeSpan.FromSeconds(1))
                .DistinctUntilChanged()
                .Skip(1)
                .Do(_ => _needReloadToApplyConfig.Value = true)
                .Subscribe(b => InternalSaveConfig(cfg => cfg.ComponentId = b))
                .DisposeItWith(Disposable);
            var heartbeatRateMs = InternalGetConfig(s => TimeSpan.FromMilliseconds(s.HeartbeatRateMs));
            _heartBeatRate = new RxValue<TimeSpan>(heartbeatRateMs).DisposeItWith(Disposable);
            _heartBeatRate
                .Throttle(TimeSpan.FromSeconds(1))
                .DistinctUntilChanged()
                .Skip(1)
                .Do(_ => _needReloadToApplyConfig.Value = true)
                .Subscribe(s => { InternalSaveConfig(cfg => cfg.HeartbeatRateMs = (int)s.TotalMilliseconds); })
                .DisposeItWith(Disposable);
            serverDevice.Heartbeat.Start();

            #endregion

            #region Mavlink devices

            var deviceTimeout = InternalGetConfig(s => TimeSpan.FromMilliseconds(s.DeviceHeartbeatTimeoutMs));
            _deviceBrowser = new MavlinkDeviceBrowser(_mavlinkRouter, deviceTimeout, RxApp.MainThreadScheduler)
                .DisposeItWith(Disposable);
            _deviceBrowser.DeviceTimeout
                .Throttle(TimeSpan.FromSeconds(1))
                .DistinctUntilChanged()
                .Skip(1)
                .Subscribe(s => InternalSaveConfig(cfg => cfg.DeviceHeartbeatTimeoutMs = (int)s.TotalMilliseconds))
                .DisposeItWith(Disposable);

            #endregion

            #region Mavlink vehicles

            Vehicles = Devices
                .Transform(CreateVehicle)
                .Filter(c => c != null)
                .DisposeMany()
                .RefCount();

            BaseStations = Devices
                .Filter(d => d.Type == (MavType)Mavlink.V2.AsvGbs.MavType.MavTypeAsvGbs)
                .Transform(CreateBaseStation)
                .DisposeMany()
                .RefCount();
            Payloads = Devices
                .Filter(d => d.Type == (MavType)Mavlink.V2.AsvSdr.MavType.MavTypeAsvSdrPayload)
                .Transform(CreateSdrDevice)
                .DisposeMany()
                .RefCount();
            AdsbDevices = Devices
                .Filter(d => d.Type == MavType.MavTypeAdsb)
                .Transform(CreateAdsbDevice)
                .DisposeMany()
                .RefCount();
            RfsaDevices = Devices
                .Filter(d => d.Type == (MavType)Mavlink.V2.AsvRfsa.MavType.MavTypeAsvRfsa)
                .Transform(CreateRfsaDevice)
                .DisposeMany()
                .RefCount();

            AllDevices = Vehicles.Transform(x => (IClientDevice)x)
                .MergeChangeSets(BaseStations.Transform(x => (IClientDevice)x))
                .MergeChangeSets(Payloads.Transform(x => (IClientDevice)x))
                .MergeChangeSets(AdsbDevices.Transform(x => (IClientDevice)x));

            #endregion

            #region Logs

            _logNames = Vehicles.Transform(x => (IClientDevice)x)
                .Merge(BaseStations.Transform(x => (IClientDevice)x))
                .Merge(Payloads.Transform(x => (IClientDevice)x))
                .Merge(AdsbDevices.Transform(x => (IClientDevice)x))
                .AutoRefreshOnObservable(d => d.Name)
                .Filter(d => d.Name.Value != null)
                .Transform(d => d.Name.Value, true)
                .AsObservableCache().DisposeItWith(Disposable);
            _mavlinkRouter
                .Filter<StatustextPacket>()
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(SaveToLog)
                .DisposeItWith(Disposable);

            #endregion
        }

        private IRfsaClientDevice CreateRfsaDevice(IMavlinkDevice device)
        {
            return new RfsaClientDevice(Router, new MavlinkClientIdentity
            {
                TargetSystemId = device.SystemId,
                TargetComponentId = device.ComponentId,
                SystemId = _systemId.Value,
                ComponentId = _componentId.Value,
            }, InternalGetConfig(c => c.Rfsa), _sequenceCalculator, RxApp.MainThreadScheduler);
        }

        private ISdrClientDevice CreateSdrDevice(IMavlinkDevice device)
        {
            var dev = new SdrClientDevice(Router, new MavlinkClientIdentity
            {
                TargetSystemId = device.SystemId,
                TargetComponentId = device.ComponentId,
                SystemId = _systemId.Value,
                ComponentId = _componentId.Value,
            }, InternalGetConfig(c => c.Sdr), _sequenceCalculator, RxApp.MainThreadScheduler);
            ((ParamsClientEx)dev.Params).Init(new MavParamByteWiseEncoding(), ArraySegment<ParamDescription>.Empty);
            return dev;
        }

        private IGbsClientDevice CreateBaseStation(IMavlinkDevice device)
        {
            return new GbsClientDevice(Router, new MavlinkClientIdentity
            {
                TargetSystemId = device.SystemId,
                TargetComponentId = device.ComponentId,
                SystemId = _systemId.Value,
                ComponentId = _componentId.Value,
            }, _sequenceCalculator, InternalGetConfig(c => c.Gbs));
        }

        private IAdsbClientDevice CreateAdsbDevice(IMavlinkDevice device)
        {
            return new AdsbClientDevice(Router, new MavlinkClientIdentity
            {
                TargetSystemId = device.SystemId,
                TargetComponentId = device.ComponentId,
                SystemId = _systemId.Value,
                ComponentId = _componentId.Value,
            }, _sequenceCalculator, InternalGetConfig(c => c.Adsb));
        }

        #region Logs

        private void SaveToLog(StatustextPacket pkt)
        {
            var indexOfNullSymbol = pkt.Payload.Text.IndexOf('\0');
            var txt = new string(pkt.Payload.Text, 0,
                indexOfNullSymbol < 0 ? pkt.Payload.Text.Length : indexOfNullSymbol);
            switch (pkt.Payload.Severity)
            {
                case MavSeverity.MavSeverityEmergency:
                case MavSeverity.MavSeverityAlert:
                case MavSeverity.MavSeverityCritical:
                case MavSeverity.MavSeverityError:
                    _log.Error(TryGetName(pkt), txt, null);
                    break;
                case MavSeverity.MavSeverityWarning:
                    _log.Warning(TryGetName(pkt), txt);
                    break;
                case MavSeverity.MavSeverityNotice:
                    _log.Trace(TryGetName(pkt), txt);
                    break;
                case MavSeverity.MavSeverityInfo:
                    _log.Trace(TryGetName(pkt), txt);
                    break;
                case MavSeverity.MavSeverityDebug:
                    _log.Trace(TryGetName(pkt), txt);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private string TryGetName(StatustextPacket pkt)
        {
            var name = _logNames.Lookup(pkt.FullId);
            return name.HasValue ? name.Value : $"[{pkt.SystemId},{pkt.ComponentId}]";
        }

        #endregion

        public IObservable<IChangeSet<IMavlinkDevice, ushort>> Devices => _deviceBrowser.Devices;
        public IMavlinkRouter Router => _mavlinkRouter;
        public IRxValue<bool> NeedReloadToApplyConfig => _needReloadToApplyConfig;
        public IRxEditableValue<byte> SystemId => _systemId;
        public IRxEditableValue<byte> ComponentId => _componentId;
        public IRxEditableValue<TimeSpan> HeartbeatRate => _heartBeatRate;
        public IObservable<IChangeSet<IClientDevice, ushort>> AllDevices { get; }
        public IObservable<IChangeSet<IVehicleClient, ushort>> Vehicles { get; }
        public IRxEditableValue<TimeSpan> DeviceTimeout => _deviceBrowser.DeviceTimeout;

        public IVehicleClient? GetVehicleByFullId(ushort id)
        {
            using var autoDispose = Vehicles.BindToObservableList(out var list).Subscribe();
            return list.Items.FirstOrDefault(c => c.Heartbeat.FullId == id);
        }

        public IObservable<IChangeSet<IGbsClientDevice, ushort>> BaseStations { get; }

        public IGbsClientDevice? GetGbsByFullId(ushort id)
        {
            using var autoDispose = BaseStations.BindToObservableList(out var list).Subscribe();
            return list.Items.FirstOrDefault(d => d.Heartbeat.FullId == id);
        }

        public IObservable<IChangeSet<ISdrClientDevice, ushort>> Payloads { get; }

        public ISdrClientDevice? GetPayloadsByFullId(ushort id)
        {
            using var autoDispose = Payloads.BindToObservableList(out var list).Subscribe();
            return list.Items.FirstOrDefault(d => d.Heartbeat.FullId == id);
        }

        public IObservable<IChangeSet<IAdsbClientDevice, ushort>> AdsbDevices { get; }

        public IAdsbClientDevice? GetAdsbVehicleByFullId(ushort id)
        {
            using var autoDispose =AdsbDevices.BindToObservableList(out var list).Subscribe();
            return list.Items.FirstOrDefault(d => d.FullId == id);
        }

        public IObservable<IChangeSet<IRfsaClientDevice, ushort>> RfsaDevices { get; }
        public IAdsbClientDevice? GetRfsaByFullId(ushort id)
        {
            using var autoDispose =AdsbDevices.BindToObservableList(out var list).Subscribe();
            return list.Items.FirstOrDefault(d => d.FullId == id);
        }

        private IVehicleClient? CreateVehicle(IMavlinkDevice device)
        {
            if (device.Autopilot == MavAutopilot.MavAutopilotArdupilotmega)
            {
                switch (device.Type)
                {
                    case MavType.MavTypeQuadrotor:
                    case MavType.MavTypeTricopter:
                    case MavType.MavTypeHexarotor:
                        return new ArduCopterClient(Router, new MavlinkClientIdentity
                        {
                            TargetSystemId = device.SystemId,
                            TargetComponentId = device.ComponentId,
                            SystemId = _systemId.Value,
                            ComponentId = _componentId.Value,
                        }, InternalGetConfig(c => c.Vehicle), _sequenceCalculator, RxApp.TaskpoolScheduler);
                    case MavType.MavTypeFixedWing:
                        return new ArduPlaneClient(Router, new MavlinkClientIdentity
                        {
                            TargetSystemId = device.SystemId,
                            TargetComponentId = device.ComponentId,
                            SystemId = _systemId.Value,
                            ComponentId = _componentId.Value,
                        }, InternalGetConfig(c => c.Vehicle), _sequenceCalculator, RxApp.TaskpoolScheduler);
                    default:
                        return null;
                }
            }

            return null;
        }
    }
}