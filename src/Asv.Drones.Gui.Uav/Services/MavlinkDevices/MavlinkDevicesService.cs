using System.ComponentModel.Composition;
using System.Reactive.Linq;
using System.Security.Principal;
using Asv.Cfg;
using Asv.Common;
using Asv.Drones.Gui.Core;
using Asv.Mavlink;
using Asv.Mavlink.Client;
using Asv.Mavlink.V2.Ardupilotmega;
using Asv.Mavlink.V2.Common;
using Asv.Mavlink.V2.Icarous;
using Asv.Mavlink.V2.Uavionix;
using DynamicData;
using Material.Icons;
using ObservableExtensions = System.ObservableExtensions;

namespace Asv.Drones.Gui.Uav
{
    public class MavlinkDeviceServiceConfig
    {
        public MavlinkPortConfig[] Ports { get; set; } = {
            new() { ConnectionString = "tcp://127.0.0.1:5762", IsEnabled = true, Name = "" },
            new() { ConnectionString = "tcp://127.0.0.1:7341?srv=true", IsEnabled = true, Name = "Base station" },
        };

        public byte SystemId { get; set; } = 254;
        public byte ComponentId { get; set; } = 254;
        public int HeartbeatRateMs { get; set; } = 1000;
        public int DeviceHeartbeatTimeoutMs { get; set; } = 30_000;
    }

    [Export(typeof(IMavlinkDevicesService))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class MavlinkDevicesService : ServiceWithConfigBase<MavlinkDeviceServiceConfig>, IMavlinkDevicesService
    {
        private readonly SourceCache<IMavlinkDevice, ushort> _devices = new(_ => _.FullId);
        private readonly MavlinkRouter _mavlinkRouter;
        private readonly RxValue<byte> _systemId;
        private readonly RxValue<byte> _componentId;
        private readonly RxValue<TimeSpan> _heartBeatRate;
        private readonly RxValue<bool> _needReloadToApplyConfig = new(false);
        private readonly MavlinkDeviceBrowser _deviceBrowser;

        [ImportingConstructor]
        public MavlinkDevicesService(IConfiguration config,IPacketSequenceCalculator sequenceCalculator):base(config)
        {
            #region Init mavlink router

            _mavlinkRouter = new MavlinkRouter(_ =>
            {
                _.RegisterCommonDialect();
                _.RegisterArdupilotmegaDialect();
                _.RegisterIcarousDialect();
                _.RegisterUavionixDialect();

            }).DisposeItWith(Disposable);
            foreach (var port in InternalGetConfig(_ => _.Ports))
            {
                _mavlinkRouter.AddPort(port);
            }
            _mavlinkRouter.OnConfigChanged.Throttle(TimeSpan.FromSeconds(1)).Subscribe(_ => InternalSaveConfig(serviceConfig=>serviceConfig.Ports = _mavlinkRouter.GetPorts().Select(_=>_mavlinkRouter.GetConfig(_)).ToArray())).DisposeItWith(Disposable);

            #endregion

            #region Init mavlink heartbeat

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
            }).Wait();
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
                    transponder.Start(heartbeatRateMs);
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
        }

        public IObservable<IChangeSet<IMavlinkDevice, ushort>> Devices => _devices.Connect().RefCount();
        public IMavlinkRouter Router => _mavlinkRouter;
        public IRxValue<bool> NeedReloadToApplyConfig => _needReloadToApplyConfig;
        public IRxEditableValue<byte> SystemId => _systemId;
        public IRxEditableValue<byte> ComponentId => _componentId;
        public IRxEditableValue<TimeSpan> HeartbeatRate => _heartBeatRate;
        public IRxEditableValue<TimeSpan> DeviceTimeout => _deviceBrowser.DeviceTimeout;


    }

    public static class MavlinkIconHelper
    {
        public static MaterialIconKind GetIcon(MavType type)
        {
            switch (type)
            {
                case MavType.MavTypeFixedWing:
                    return MaterialIconKind.Airplane;
                case MavType.MavTypeGeneric:
                case MavType.MavTypeQuadrotor:
                case MavType.MavTypeHexarotor:
                case MavType.MavTypeOctorotor:
                case MavType.MavTypeTricopter:
                    return MaterialIconKind.Quadcopter;
                case MavType.MavTypeHelicopter:
                    return MaterialIconKind.Helicopter;
                case MavType.MavTypeAntennaTracker:
                    return MaterialIconKind.Antenna;
                case MavType.MavTypeGcs:
                    return MaterialIconKind.Computer;
                default:
                    return MaterialIconKind.HelpNetworkOutline;
            }
        }

        public static string GetTypeName(MavType type)
        {
            switch (type)
            {
                case MavType.MavTypeFixedWing:
                    return "Fixed wing";
                case MavType.MavTypeGeneric:
                case MavType.MavTypeQuadrotor:
                    return "Quadrotor";
                case MavType.MavTypeHexarotor:
                    return "Hexarotor";
                case MavType.MavTypeOctorotor:
                    return "Octorotor";
                case MavType.MavTypeTricopter:
                    return "Tricopter";
                case MavType.MavTypeHelicopter:
                    return "Helicopter";
                default:
                    return "Unknown type";
            }
        }
    }
}