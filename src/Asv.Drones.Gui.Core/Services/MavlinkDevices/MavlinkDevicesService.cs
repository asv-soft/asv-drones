using System.ComponentModel.Composition;
using System.Reactive.Linq;
using Asv.Cfg;
using Asv.Common;
using Asv.Drones.Gui.Core;
using Asv.Mavlink;
using Asv.Mavlink.Client;
using Asv.Mavlink.V2.Ardupilotmega;
using Asv.Mavlink.V2.Common;
using Asv.Mavlink.V2.Icarous;
using Asv.Mavlink.V2.Uavionix;
using Asv.Mavlink.V2.AsvGbs;
using Avalonia.Controls.Shapes;
using DynamicData;
using DynamicData.Binding;
using ReactiveUI;
using MavType = Asv.Mavlink.V2.Common.MavType;

namespace Asv.Drones.Gui.Uav
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
    }

    [Export(typeof(IMavlinkDevicesService))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class MavlinkDevicesService : ServiceWithConfigBase<MavlinkDeviceServiceConfig>, IMavlinkDevicesService
    {
        private readonly IPacketSequenceCalculator _sequenceCalculator;
        private readonly ILogService _log;
        private readonly SourceCache<IMavlinkDevice, ushort> _devices = new(_ => _.FullId);
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

            _mavlinkRouter = new MavlinkRouter(_ =>
            {
                _.RegisterCommonDialect();
                _.RegisterArdupilotmegaDialect();
                _.RegisterIcarousDialect();
                _.RegisterUavionixDialect();
                _.RegisterAsvGbsDialect();

            }).DisposeItWith(Disposable);
            foreach (var port in InternalGetConfig(_ => _.Ports))
            {
                _mavlinkRouter.AddPort(port);
            }
            _mavlinkRouter.OnConfigChanged.Throttle(TimeSpan.FromSeconds(1)).Subscribe(_ => InternalSaveConfig(serviceConfig=>serviceConfig.Ports = _mavlinkRouter.GetPorts().Select(_=>_mavlinkRouter.GetConfig(_)).ToArray())).DisposeItWith(Disposable);

            #endregion

            #region InitUriHost mavlink heartbeat

            var serverIdentity = InternalGetConfig(_ => new MavlinkServerIdentity { SystemId = _.SystemId, ComponentId = _.ComponentId });
            var transponder = new MavlinkPacketTransponder<HeartbeatPacket, HeartbeatPayload>(_mavlinkRouter, serverIdentity, sequenceCalculator)
                .DisposeItWith(Disposable);
            transponder.Set(_ =>
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
                .Subscribe(_ =>
                {
                    transponder.Start(_);
                    InternalSaveConfig(cfg => cfg.HeartbeatRateMs = (int)_.TotalMilliseconds);
                })
                .DisposeItWith(Disposable);
            transponder.Start(heartbeatRateMs);

            #endregion

            #region Mavlink devices

            var deviceTimeout = InternalGetConfig(_ => TimeSpan.FromMilliseconds(_.DeviceHeartbeatTimeoutMs));
            _deviceBrowser = new MavlinkDeviceBrowser(_mavlinkRouter, deviceTimeout).DisposeItWith(Disposable);
            _deviceBrowser.OnFoundDevice.Subscribe(_devices.AddOrUpdate).DisposeItWith(Disposable);
            _deviceBrowser.OnLostDevice.Subscribe(_devices.Remove).DisposeItWith(Disposable);
            _deviceBrowser.Devices.ForEach(_devices.AddOrUpdate);

            _deviceBrowser.DeviceTimeout
                .Throttle(TimeSpan.FromSeconds(1))
                .DistinctUntilChanged()
                .Skip(1)
                .Subscribe(_ => InternalSaveConfig(cfg => cfg.DeviceHeartbeatTimeoutMs = (int)_.TotalMilliseconds))
                .DisposeItWith(Disposable);

            #endregion

            #region Mavlink vehicles

            Vehicles = _devices
            .Connect()
                .Filter(_=> _.Autopilot is MavAutopilot.MavAutopilotArdupilotmega)
                .Transform(CreateVehicle).Where(_ => _ != null).DisposeMany().RefCount();

            #endregion

            #region Logs

            _logNames = Vehicles
                .AutoRefreshOnObservable(_ => _.Name)
                .Filter(_=>_.Name.Value!=null)
                .Transform(_ => _.Name.Value,true)
                .AsObservableCache();
            _mavlinkRouter.Filter<StatustextPacket>().Subscribe(SaveToLog).DisposeItWith(Disposable);

            #endregion
           
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
            return name.HasValue ? name.Value : $"[{pkt.SystemId},{pkt.ComponenId}]";
        }
        
        #endregion

        public IObservable<IChangeSet<IMavlinkDevice, ushort>> Devices => _devices.Connect().RefCount();
        public IMavlinkRouter Router => _mavlinkRouter;
        public IRxValue<bool> NeedReloadToApplyConfig => _needReloadToApplyConfig;
        public IRxEditableValue<byte> SystemId => _systemId;
        public IRxEditableValue<byte> ComponentId => _componentId;
        public IRxEditableValue<TimeSpan> HeartbeatRate => _heartBeatRate;
        public IObservable<IChangeSet<IVehicle, ushort>> Vehicles { get; }
        public IRxEditableValue<TimeSpan> DeviceTimeout => _deviceBrowser.DeviceTimeout;

        private IVehicle? CreateVehicle(IMavlinkDevice device)
        {
            var proto = new MavlinkClient(Router, new MavlinkClientIdentity
            {
                TargetSystemId = device.SystemId,
                TargetComponentId = device.ComponentId,
                SystemId = _systemId.Value,
                ComponentId = _componentId.Value,
            }, new MavlinkClientConfig(), _sequenceCalculator, false, RxApp.MainThreadScheduler); // TODO: MavlinkClientConfig - add to settings 

            IVehicle dev = default;

            if (device.Autopilot == MavAutopilot.MavAutopilotArdupilotmega)
            {
                switch (device.Type)
                {
                    case MavType.MavTypeQuadrotor:
                    case MavType.MavTypeTricopter:
                    case MavType.MavTypeHexarotor:
                        dev = new VehicleArdupilotCopter(proto, new VehicleBaseConfig(),true);// TODO: VehicleBaseConfig - add to settings 
                        break;
                    case MavType.MavTypeFixedWing:
                        dev = new VehicleArdupilotPlane(proto, new VehicleBaseConfig(),true); // TODO: VehicleBaseConfig - add to settings 
                        break;
                }
            }

            if (dev == null)
            {
                proto.Dispose();
            }
            else
            {
                dev.StartListen();    
            }

            return dev;
            
            
        }

    }
}