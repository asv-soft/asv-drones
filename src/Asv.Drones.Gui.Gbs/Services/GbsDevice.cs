using Asv.Cfg;
using Asv.Common;
using Asv.Drones.Gui.Uav;
using Asv.Mavlink;
using Asv.Mavlink.Client;
using ReactiveUI;

namespace Asv.Drones.Gui.Gbs;

public interface IGbsDevice
{
    IMavlinkClient Client { get; }
    ushort FullId { get; }
}

public class GbsDeviceConfig
{
    public MavlinkClientConfig Mavlink { get; set; } = new();
}

public class GbsDevice:DisposableOnceWithCancel,IGbsDevice
{
    private readonly MavlinkClient _client;

    public GbsDevice(IMavlinkDevicesService mavlink, IConfiguration cfg, IPacketSequenceCalculator pkt, IMavlinkDevice info)
    {
        var config = cfg.Get<GbsDeviceConfig>();
        FullId = (ushort) (info.ComponentId | (uint) info.SystemId << 8);
        _client = new MavlinkClient(mavlink.Router, new MavlinkClientIdentity
        {
            SystemId = mavlink.SystemId.Value,
            ComponentId = mavlink.ComponentId.Value,
            TargetSystemId = info.SystemId,
            TargetComponentId = info.ComponentId,
        }, config.Mavlink, pkt, false, RxApp.MainThreadScheduler)
            .DisposeItWith(Disposable);
        
        
    }

    public IMavlinkClient Client => _client;
    public ushort FullId { get; }
}