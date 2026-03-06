using System;
using Asv.Avalonia;
using Asv.Avalonia.IO;
using Asv.Drones.Api;
using Asv.Mavlink;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Asv.Drones;
public static class ApiMixin
{
    extension(IHostApplicationBuilder builder)
    {
        public IHostApplicationBuilder UseDronesApp(Action<Builder>? configure = null)
        {
            configure ??= b => b.UseDefault();
            configure(new Builder(builder));
            return builder;
        }
    }
    
    public class Builder(IHostApplicationBuilder builder)
    {
        public Builder UseDefault()
        {
            builder.Services.AddSingleton<IPacketSequenceCalculator, PacketSequenceCalculator>();
            builder.Services.AddSingleton<IDeviceManagerExtension, GnssDeviceManagerExtension>();
            builder.UseApiControls();
            return UseMavlinkHost()
                .UseOptionalPacketViewer();
        }
        
        public Builder UseMavlinkHost()
        {
            builder.Services.AddSingleton<MavlinkHost>();
            builder.Services.AddHostedService(svc => svc.GetRequiredService<MavlinkHost>());
            builder.Services.AddSingleton<IMavlinkHost>(svc => svc.GetRequiredService<MavlinkHost>());
            builder.Services.AddSingleton<IDeviceManagerExtension>(svc => svc.GetRequiredService<MavlinkHost>());
            return this;
        }

        public Builder UseOptionalFlightMode()
        {
            builder.Shell.Pages.Register<FlightPageViewModel, FlightPageView>(FlightPageViewModel.PageId);
            builder.Shell.Pages.Home.UseExtension<HomePageFlightExtension>();
            builder.Extensions.Register<IFlightMode, FlightUavAnchorsExtension>();
            builder.Extensions.Register<IFlightMode, FlightWidgetsExtension>();
            builder.ViewLocator.RegisterViewFor<UavWidgetViewModel, UavWidgetView>();
            builder.ViewLocator.RegisterViewFor<MissionProgressViewModel, MissionProgressView>();
            builder.ViewLocator.RegisterViewFor<SetAltitudeDialogViewModel, SetAltitudeDialogView>();
            return this;
        }

        public Builder UseOptionalPacketViewer()
        {
            builder.Shell.Pages.Register<PacketViewerViewModel, PacketViewerView>(PacketViewerViewModel.PageId);
            builder.Shell.Pages.Home.UseExtension<HomePacketViewerExtension>();
            builder.ViewLocator.RegisterViewFor<PacketMessageViewModel, PacketMessageView>();
            builder.Services.AddSingleton<IPacketConverter, DefaultMavlinkPacketConverter>();
            builder.ViewLocator.RegisterViewFor<SavePacketMessagesDialogViewModel, SavePacketMessagesDialogView>();
            return this;
        }
    }
}
