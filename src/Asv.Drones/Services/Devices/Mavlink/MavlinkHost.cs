using System;
using System.Composition;
using Asv.Avalonia;
using Asv.Avalonia.IO;
using Asv.Cfg;
using Asv.IO;
using Asv.Mavlink;
using Asv.Mavlink.Minimal;
using Avalonia.Media;
using Material.Icons;
using R3;

namespace Asv.Drones;

public class MavlinkDeviceManagerExtensionConfig
{
    public byte SystemId { get; set; } = 255;
    public byte ComponentId { get; set; } = 255;
}

[Export(typeof(IDeviceManagerExtension))]
[Export(typeof(IMavlinkHost))]
[Export(typeof(IStartupTask))]
[Shared]
public class MavlinkHost : IDeviceManagerExtension, IMavlinkHost, IStartupTask
{
    private readonly IConfiguration _cfgSvc;
    private readonly IPacketSequenceCalculator _seq;
    private readonly MavlinkDeviceManagerExtensionConfig _cfg;

    [ImportingConstructor]
    public MavlinkHost(IConfiguration cfgSvc, IPacketSequenceCalculator seq)
    {
        _cfgSvc = cfgSvc;
        _seq = seq;
        _cfg = cfgSvc.Get<MavlinkDeviceManagerExtensionConfig>();
        Identity = new MavlinkIdentity(_cfg.SystemId, _cfg.ComponentId);
    }

    public void Configure(IProtocolBuilder builder)
    {
        builder.RegisterMavlinkV2Protocol();
        builder.Features.RegisterMavlinkV2WrapFeature();
        builder.Features.RegisterBroadcastFeature<MavlinkMessage>();
    }

    public void Configure(IDeviceExplorerBuilder builder)
    {
        builder.Factories.RegisterDefaultDevices(Identity, _seq, _cfgSvc);
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

    public void Run(IDeviceManager deviceManager)
    {
        var config = _cfgSvc.Get<MavlinkHeartbeatServerConfig>();
        var core = new CoreServices(_seq, deviceManager.Router, deviceManager.ProtocolFactory);

        Heartbeat = new HeartbeatServer(Identity, config, core);
        Heartbeat.Set(m =>
        {
            m.Autopilot = MavAutopilot.MavAutopilotInvalid;
            m.Type = MavType.MavTypeGcs;
            m.BaseMode = MavModeFlag.MavModeFlagCustomModeEnabled;
        });
        core.Connection.OnTxMessage.Subscribe(x => { });
        Heartbeat.Start();
    }

    public IHeartbeatServer? Heartbeat { get; set; }

    public void AppCtor()
    {
        // do nothing
    }

    public void OnFrameworkInitializationCompleted()
    {
        // do nothing
    }

    public void Initialize()
    {
        // do nothing
    }

    public MavlinkIdentity Identity { get; }
}
