#nullable enable
using System.ComponentModel.Composition;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using Asv.Cfg;
using Asv.Common;
using Asv.Drones.Gui.Uav;
using Asv.Mavlink;
using Asv.Mavlink.V2.Ardupilotmega;
using Asv.Mavlink.V2.AsvGbs;
using Asv.Mavlink.V2.Common;
using Asv.Mavlink.V2.Icarous;
using Asv.Mavlink.V2.Uavionix;
using DynamicData;
using DynamicData.Binding;
using ReactiveUI;
using MavType = Asv.Mavlink.V2.Common.MavType;

namespace Asv.Drones.Gui.Core
{
    public class MavlinkDeviceServiceConfig
    {
        public MavlinkPortConfig[] Ports { get; set; } = {
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
    }

    [Export(typeof(IMavlinkDevicesService))]
    [PartCreationPolicy(CreationPolicy.Shared)]
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
        public MavlinkDevicesService(IConfiguration config,IPacketSequenceCalculator sequenceCalculator,ILogService log):base(config)
        {
            _sequenceCalculator = sequenceCalculator ?? throw new ArgumentNullException(nameof(sequenceCalculator));
            _log = log ?? throw new ArgumentNullException(nameof(log));
            
            #region InitUriHost mavlink router

            _mavlinkRouter = new MavlinkRouter(MavlinkV2Connection.RegisterDefaultDialects,publishScheduler: RxApp.MainThreadScheduler).DisposeItWith(Disposable);
            _mavlinkRouter.WrapToV2ExtensionEnabled = InternalGetConfig(_ => _.WrapToV2ExtensionEnabled);
            foreach (var port in InternalGetConfig(_ => _.Ports))
            {
                _mavlinkRouter.AddPort(port);
            }
            _mavlinkRouter.OnConfigChanged
                .Throttle(TimeSpan.FromSeconds(1))
                .Subscribe(_ => InternalSaveConfig(serviceConfig=>serviceConfig.Ports = _mavlinkRouter.GetConfig()))
                .DisposeItWith(Disposable);

            #endregion

            #region InitUriHost mavlink heartbeat

            var serverIdentity = InternalGetConfig(_ =>new MavlinkServerIdentity { SystemId = _.SystemId, ComponentId = _.ComponentId } );
            var serverConfig = InternalGetConfig(_ => new ServerDeviceConfig{Heartbeat = new MavlinkHeartbeatServerConfig{HeartbeatRateMs = _.HeartbeatRateMs}} );
            var serverDevice = new ServerDevice(Router, sequenceCalculator,serverIdentity,serverConfig, Scheduler.Default).DisposeItWith(Disposable);
            serverDevice.Heartbeat.Set(_ =>
            {
                _.Autopilot = MavAutopilot.MavAutopilotInvalid;
                _.BaseMode = 0;
                _.CustomMode = 0;
                _.MavlinkVersion = 3;
                _.SystemStatus = MavState.MavStateActive;
                _.Type = MavType.MavTypeGcs;
            });
            _systemId = new RxValue<byte>(serverIdentity.SystemId).DisposeItWith(Disposable);
            _systemId
                .Throttle(TimeSpan.FromSeconds(1))
                .DistinctUntilChanged()
                .Skip(1)
                .Do(_=> _needReloadToApplyConfig.Value = true)
                .Subscribe(_=>InternalSaveConfig(cfg=>cfg.SystemId = _))
                .DisposeItWith(Disposable);
            _componentId = new RxValue<byte>(serverIdentity.ComponentId).DisposeItWith(Disposable);
            _componentId
                .Throttle(TimeSpan.FromSeconds(1))
                .DistinctUntilChanged()
                .Skip(1)
                .Do(_ => _needReloadToApplyConfig.Value = true)
                .Subscribe(_ => InternalSaveConfig(cfg => cfg.ComponentId = _))
                .DisposeItWith(Disposable);
            var heartbeatRateMs = InternalGetConfig(_ => TimeSpan.FromMilliseconds(_.HeartbeatRateMs));
            _heartBeatRate = new RxValue<TimeSpan>(heartbeatRateMs).DisposeItWith(Disposable);
            _heartBeatRate
                .Throttle(TimeSpan.FromSeconds(1))
                .DistinctUntilChanged()
                .Skip(1)
                .Do(_ => _needReloadToApplyConfig.Value = true)
                .Subscribe(_ =>
                {
                    InternalSaveConfig(cfg => cfg.HeartbeatRateMs = (int)_.TotalMilliseconds);
                })
                .DisposeItWith(Disposable);
            serverDevice.Heartbeat.Start();

            #endregion

            #region Mavlink devices

            var deviceTimeout = InternalGetConfig(_ => TimeSpan.FromMilliseconds(_.DeviceHeartbeatTimeoutMs));
            _deviceBrowser = new MavlinkDeviceBrowser(_mavlinkRouter, deviceTimeout, RxApp.MainThreadScheduler).DisposeItWith(Disposable);
            _deviceBrowser.DeviceTimeout
                .Throttle(TimeSpan.FromSeconds(1))
                .DistinctUntilChanged()
                .Skip(1)
                .Subscribe(_ => InternalSaveConfig(cfg => cfg.DeviceHeartbeatTimeoutMs = (int)_.TotalMilliseconds))
                .DisposeItWith(Disposable);

            #endregion

            #region Mavlink vehicles

            
            
            Vehicles = Devices
                .Filter(_=> _.Autopilot is MavAutopilot.MavAutopilotArdupilotmega)
                .Transform(CreateVehicle)
                .Filter(_=>_!=null)
                .DisposeMany()
                .RefCount();
            
            BaseStations = Devices
                .Filter(_=> _.Type == (MavType)Mavlink.V2.AsvGbs.MavType.MavTypeAsvGbs)
                .Transform(CreateBaseStation)
                .DisposeMany()
                .RefCount();
            Payloads = Devices
                .Filter(_=> _.Type == (MavType)Mavlink.V2.AsvSdr.MavType.MavTypeAsvSdrPayload)
                .Transform(CreateSdrDevice)
                .DisposeMany()
                .RefCount();
            AdsbDevices = Devices
                .Filter(_ => _.Type == MavType.MavTypeAdsb)
                .Transform(CreateAdsbDevice)
                .DisposeMany()
                .RefCount();

            #endregion

            #region Logs

            _logNames = Vehicles
                .AutoRefreshOnObservable(_ => _.Name)
                .Filter(_=>_.Name.Value!=null)
                .Transform(_ => _.Name.Value,true)
                .AsObservableCache();
            _mavlinkRouter
                .Filter<StatustextPacket>()
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(SaveToLog)
                .DisposeItWith(Disposable);

            #endregion
           
        }

        private ISdrClientDevice CreateSdrDevice(IMavlinkDevice device)
        {
            return new SdrClientDevice(Router, new MavlinkClientIdentity
            {
                TargetSystemId = device.SystemId,
                TargetComponentId = device.ComponentId,
                SystemId = _systemId.Value,
                ComponentId = _componentId.Value,
            },InternalGetConfig(_ => _.Sdr), _sequenceCalculator, RxApp.MainThreadScheduler);
        }

        private IGbsClientDevice CreateBaseStation(IMavlinkDevice device)
        {
            return new GbsClientDevice(Router, new MavlinkClientIdentity
            {
                TargetSystemId = device.SystemId,
                TargetComponentId = device.ComponentId,
                SystemId = _systemId.Value,
                ComponentId = _componentId.Value,
            }, _sequenceCalculator, InternalGetConfig(_ => _.Gbs));
        }

        private IAdsbClientDevice CreateAdsbDevice(IMavlinkDevice device)
        {
            return new AdsbClientDevice(Router, new MavlinkClientIdentity
            {
                TargetSystemId = device.SystemId,
                TargetComponentId = device.ComponentId,
                SystemId = _systemId.Value,
                ComponentId = _componentId.Value,
            }, _sequenceCalculator, InternalGetConfig(_ => _.Adsb));
        }

        #region Logs

        private void SaveToLog(StatustextPacket pkt)
        {
            var indexOfNullSymbol = pkt.Payload.Text.IndexOf('\0');
            var txt = new string(pkt.Payload.Text,0, indexOfNullSymbol < 0 ? pkt.Payload.Text.Length : indexOfNullSymbol);
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
        public IObservable<IChangeSet<IVehicleClient, ushort>> Vehicles { get; }
        public IRxEditableValue<TimeSpan> DeviceTimeout => _deviceBrowser.DeviceTimeout;

        public IVehicleClient? GetVehicleByFullId(ushort id)
        {
            using var a = Vehicles.BindToObservableList(out var list).Subscribe();
            return list.Items.FirstOrDefault(_=>_.Heartbeat.FullId == id);
        }

        public IObservable<IChangeSet<IGbsClientDevice, ushort>> BaseStations { get; }
        public IGbsClientDevice? GetGbsByFullId(ushort id)
        {
            BaseStations.BindToObservableList(out var list).Subscribe();
            return list.Items.FirstOrDefault(_=>_.Heartbeat.FullId == id);
        }

        public IObservable<IChangeSet<ISdrClientDevice, ushort>> Payloads { get; }
        public ISdrClientDevice? GetPayloadsByFullId(ushort id)
        {
            using var a = Payloads.BindToObservableList(out var list).Subscribe();
            return list.Items.FirstOrDefault(_=>_.Heartbeat.FullId == id);
        }
        
        public IObservable<IChangeSet<IAdsbClientDevice, ushort>> AdsbDevices { get; }

        public IAdsbClientDevice? GetAdsbVehicleByFullId(ushort id)
        {
            AdsbDevices.BindToObservableList(out var list).Subscribe();
            return list.Items.FirstOrDefault(_ => _.FullId == id);
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
                        return new ArduCopterClient(Router,new MavlinkClientIdentity
                        {
                            TargetSystemId = device.SystemId,
                            TargetComponentId = device.ComponentId,
                            SystemId = _systemId.Value,
                            ComponentId = _componentId.Value,
                        },InternalGetConfig(_ => _.Vehicle), _sequenceCalculator);
                    case MavType.MavTypeFixedWing:
                        return new ArduPlaneClient(Router,new MavlinkClientIdentity
                        {
                            TargetSystemId = device.SystemId,
                            TargetComponentId = device.ComponentId,
                            SystemId = _systemId.Value,
                            ComponentId = _componentId.Value,
                        },InternalGetConfig(_ => _.Vehicle), _sequenceCalculator,null);
                    default:
                        return null;
                }
            }
            return null;
            
            
        }

    }
}