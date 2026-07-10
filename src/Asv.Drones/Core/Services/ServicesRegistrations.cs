using Asv.Avalonia;
using Asv.Avalonia.IO;
using Asv.Drones.Api;
using Asv.Mavlink;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Asv.Drones;

public static class ServicesRegistrations
{
    extension(CoreRegistrations.Builder builder)
    {
        public Builder Services => new(builder);

        public CoreRegistrations.Builder RegisterServices(Action<Builder>? configure = null)
        {
            configure ??= b => b.RegisterDefault();
            configure.Invoke(new Builder(builder));
            return builder;
        }
    }

    public class Builder(CoreRegistrations.Builder builder) : IDependencyBuilder
    {
        public IHostApplicationBuilder AppBuilder => builder.AppBuilder;

        public Builder RegisterDefault()
        {
            RegisterPacketConverters();
            RegisterPacketSequenceCalculator();
            RegisterGnssDeviceManagerExtensions();
            RegisterMavlinkHost();
            return this;
        }

        public Builder RegisterPacketConverters()
        {
            AppBuilder.Services.AddSingleton<IPacketConverter, DefaultMavlinkPacketConverter>();
            return this;
        }

        public Builder RegisterGnssDeviceManagerExtensions()
        {
            AppBuilder.Services.AddSingleton<IDeviceManagerExtension, GnssDeviceManagerExtension>();
            return this;
        }

        public Builder RegisterPacketSequenceCalculator()
        {
            AppBuilder.Services.AddSingleton<IPacketSequenceCalculator, PacketSequenceCalculator>();
            return this;
        }

        public Builder RegisterMavlinkHost()
        {
            AppBuilder.Services.AddSingleton<MavlinkHost>();
            AppBuilder.Services.AddHostedService(svc => svc.GetRequiredService<MavlinkHost>());
            AppBuilder.Services.AddSingleton<IMavlinkHost>(svc =>
                svc.GetRequiredService<MavlinkHost>()
            );
            AppBuilder.Services.AddSingleton<IDeviceManagerExtension>(svc =>
                svc.GetRequiredService<MavlinkHost>()
            );
            return this;
        }
    }
}
