using System.Composition;
using Asv.Avalonia.IO;
using Asv.Cfg;
using Asv.IO;
using Asv.Mavlink;
using Avalonia.Media;
using Material.Icons;

namespace Asv.Drones.Api;

public class MavlinkDeviceManagerExtensionConfig
{
    public byte SystemId { get; set; } = 255;
    public byte ComponentId { get; set; } = 255;
}

[Export(typeof(IDeviceManagerExtension))]
[Shared]
public class MavlinkDeviceManagerExtension : IDeviceManagerExtension
{
    private readonly IConfiguration _cfgSvc;
    private readonly IPacketSequenceCalculator _seq;
    private readonly MavlinkDeviceManagerExtensionConfig _cfg;

    [ImportingConstructor]
    public MavlinkDeviceManagerExtension(IConfiguration cfgSvc, IPacketSequenceCalculator seq)
    {
        _cfgSvc = cfgSvc;
        _seq = seq;
        _cfg = cfgSvc.Get<MavlinkDeviceManagerExtensionConfig>();
    }
    
    public void Configure(IProtocolBuilder builder)
    {
        builder.Features.RegisterMavlinkV2WrapFeature();
        builder.RegisterMavlinkV2Protocol();
    }

    public void Configure(IDeviceExplorerBuilder builder)
    {
        builder.Factories.RegisterDefaultDevices(new MavlinkIdentity(_cfg.SystemId, _cfg.ComponentId),_seq, _cfgSvc);
    }

    public bool TryGetIcon(DeviceId id, out MaterialIconKind? icon)
    {
        icon = DeviceIconMixin.GetIcon(id);
        return icon != null;
    }

    public bool TryGetDeviceBrush(DeviceId id, out IBrush? brush)
    {
        brush = null;
        return false;
    }
}