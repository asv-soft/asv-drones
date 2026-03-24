using Asv.Avalonia;
using Asv.Avalonia.IO;
using Asv.Cfg;
using Asv.IO;
using Asv.Mavlink;
using Asv.Mavlink.Ardupilotmega;
using Asv.Mavlink.AsvAudio;
using Asv.Mavlink.AsvChart;
using Asv.Mavlink.Avssuas;
using Asv.Mavlink.Common;
using Asv.Mavlink.Csairlink;
using Asv.Mavlink.Cubepilot;
using Asv.Mavlink.Icarous;
using Asv.Mavlink.Minimal;
using Asv.Mavlink.Storm32;
using Asv.Mavlink.Ualberta;
using Asv.Mavlink.Uavionix;
using Asv.Mavlink.AsvGbs;
using Asv.Mavlink.AsvSdr;
using Asv.Mavlink.AsvAudio;
using Asv.Mavlink.AsvChart;
using Asv.Mavlink.AsvRadio;
using Asv.Mavlink.AsvRsga;
using Asv.Mavlink.AsvRfsa;
using Material.Icons;
using Microsoft.Extensions.Hosting;
using R3;
using MavType = Asv.Mavlink.Minimal.MavType;

namespace Asv.Drones.Api;

public class MavlinkDeviceManagerExtensionConfig
{
    public byte SystemId { get; set; } = 255;
    public byte ComponentId { get; set; } = 255;
}

public class MavlinkHost : IDeviceManagerExtension, IMavlinkHost, IHostedService
{
    private readonly IConfiguration _cfgSvc;
    private readonly IPacketSequenceCalculator _seq;
    private readonly MavlinkDeviceManagerExtensionConfig _cfg;
    private readonly IProtocolMessageFactory<MavlinkMessage, int> _messageFactory;

    public MavlinkHost(IConfiguration cfgSvc, IPacketSequenceCalculator seq, IEnumerable<IMavlinkMessagesExtension> extensions)
    {
        _cfgSvc = cfgSvc;
        _seq = seq;
        _cfg = cfgSvc.Get<MavlinkDeviceManagerExtensionConfig>();
        Identity = new MavlinkIdentity(_cfg.SystemId, _cfg.ComponentId);
        _messageFactory = MavlinkV2Protocol.CreateMessageFactory(builder =>
        {
            // TODO: replace with extension in the future: RegisterDefault
            builder.RegisterMinimalDialect();
                builder.RegisterCommonDialect();
                builder.RegisterArdupilotmegaDialect();
                builder.RegisterIcarousDialect();
                builder.RegisterUalbertaDialect();
                builder.RegisterStorm32Dialect();
                builder.RegisterAvssuasDialect();
                builder.RegisterUavionixDialect();
                builder.RegisterCubepilotDialect();
                builder.RegisterCsairlinkDialect();
                builder.RegisterAsvGbsDialect();
                builder.RegisterAsvSdrDialect();
                builder.RegisterAsvAudioDialect();
                builder.RegisterAsvRadioDialect();
                builder.RegisterAsvRfsaDialect();
                builder.RegisterAsvChartDialect();
                builder.RegisterAsvRsgaDialect();
            foreach (var ext in extensions)
            {
                ext.Extend(builder);
            }
        });
    }

    public void Configure(IProtocolBuilder builder)
    {
        builder.RegisterMavlinkV2Protocol(_messageFactory);
        builder.Features.RegisterMavlinkV2WrapFeature(_messageFactory);
        builder.Features.RegisterBroadcastFeature<MavlinkMessage>();
    }

    public void Configure(IDeviceExplorerBuilder builder)
    {
        builder.Factories.RegisterDefaultDevices(Identity, _seq, _cfgSvc, _messageFactory);
    }

    public bool TryGetIcon(DeviceId id, out MaterialIconKind? icon)
    {
        icon = DeviceIconMixin.GetIcon(id);
        return icon != null;
    }

    public bool TryGetDeviceBrush(DeviceId id, out AsvColorKind brush)
    {
        brush = AsvColorKind.None;
        return false;
    }

    public void Run(IDeviceManager deviceManager)
    {
        var config = _cfgSvc.Get<MavlinkHeartbeatServerConfig>();
        Context = new CoreServices(_seq, deviceManager.Router, deviceManager.ProtocolFactory, _messageFactory);

        Heartbeat = new HeartbeatServer(Identity, config, Context);
        Heartbeat.Set(m =>
        {
            m.Autopilot = MavAutopilot.MavAutopilotInvalid;
            m.Type = MavType.MavTypeGcs;
            m.BaseMode = MavModeFlag.MavModeFlagCustomModeEnabled;
        });
        Heartbeat.Start();
    }

    public IHeartbeatServer? Heartbeat { get; private set; }

    public IProtocolMessageFactory<MavlinkMessage, int> MessageFactory => _messageFactory;
    public IMavlinkContext Context { get; private set; }
    public MavlinkIdentity Identity { get; }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
