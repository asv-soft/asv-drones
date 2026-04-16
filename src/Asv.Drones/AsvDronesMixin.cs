using System;
using Asv.Avalonia;
using Asv.Avalonia.IO;
using Asv.Drones.Api;
using Asv.Mavlink;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Asv.Drones;

public static class AsvDronesMixin
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
        public IHostApplicationBuilder Parent => builder;

        public Builder UseDefault()
        {
            builder.Services.AddSingleton<IPacketSequenceCalculator, PacketSequenceCalculator>();
            builder.Services.AddSingleton<IDeviceManagerExtension, GnssDeviceManagerExtension>();
            return UseMavlinkHost()
                .UseMavParams()
                .UseOptionalPacketViewer()
                .UseFlightMode()
                .UseExtendableFlightMode()
                .UseCommands()
                .UseFileBrowser()
                .UseSetupPage();
        }

        public Builder UseControls()
        {
            builder.ViewLocator.RegisterViewFor<UavWidgetViewModel, UavWidgetView>();
            builder.ViewLocator.RegisterViewFor<
                VelocityUavIndicatorViewModel,
                VelocityUavIndicator
            >();
            builder.ViewLocator.RegisterViewFor<
                BatteryUavIndicatorViewModel,
                BatteryUavIndicator
            >();
            return this;
        }

        public Builder UseCommands()
        {
            builder
                .RegisterMavlinkCommands()
                .Commands.Register<OpenSetupCommand>()
                .Register<ChangeFrameTypeCommand>()
                .Register<OpenPacketViewerCommand>()
                .Register<ExportPacketsToCsvCommand>()
                .Register<ClearAllPacketsCommand>()
                .Register<OpenMavParamsCommand>()
                .Register<UpdateParamsCommand>()
                .Register<StopUpdateParamsCommand>()
                .Register<RemoveAllPinsCommand>()
                .Register<MavlinkParamsWriteCommand>()
                .Register<MavlinkParamReadCommand>()
                .Register<OpenFlightModeCommand>()
                .Register<OpenFlightCommand>()
                .Register<TakeOffCommand>()
                .Register<StartMissionCommand>()
                .Register<RTLCommand>()
                .Register<LandCommand>()
                .Register<GuidedModeCommand>()
                .Register<AutoModeCommand>()
                .Register<UpdateMissionCommand>()
                .Register<OpenFileBrowserCommand>()
                .Register<UploadItemCommand>()
                .Register<DownloadItemCommand>()
                .Register<BurstDownloadItemCommand>()
                .Register<CreateDirectoryCommand>()
                .Register<CalculateCrc32Command>()
                .Register<FindFileCommand>()
                .Register<CommitRenameCommand>()
                .Register<RemoveItemCommand>();
            return this;
        }

        public Builder UseMavlinkHost()
        {
            builder.Services.AddSingleton<MavlinkHost>();
            builder.Services.AddHostedService(svc => svc.GetRequiredService<MavlinkHost>());
            builder.Services.AddSingleton<IMavlinkHost>(svc =>
                svc.GetRequiredService<MavlinkHost>()
            );
            builder.Services.AddSingleton<IDeviceManagerExtension>(svc =>
                svc.GetRequiredService<MavlinkHost>()
            );
            return this;
        }

        public Builder UseMavParams()
        {
            builder.ViewLocator.RegisterViewFor<MavParamTextBoxViewModel, MavParamTextBoxView>();
            builder.ViewLocator.RegisterViewFor<MavParamButtonViewModel, MavParamButtonView>();
            builder.ViewLocator.RegisterViewFor<MavParamComboBoxViewModel, MavParamComboBoxView>();
            builder.ViewLocator.RegisterViewFor<
                MavParamAltitudeTextBoxViewModel,
                MavParamAltitudeTextBoxView
            >();
            builder.ViewLocator.RegisterViewFor<
                MavParamLatLonTextBoxViewModel,
                MavParamLatLonTextBoxView
            >();
            builder.ViewLocator.RegisterViewFor<
                MavParamAsciiCharViewModel,
                MavParamAsciiCharView
            >();
            builder.ViewLocator.RegisterViewFor<MavParamButtonViewModel, MavParamButtonView>();

            builder.Shell.Pages.Register<MavParamsPageViewModel, MavParamsPageView>(
                MavParamsPageViewModel.PageId
            );
            builder.Shell.Pages.Home.UseItemExtension<HomePageParamsDeviceItemAction>();
            builder.ViewLocator.RegisterViewFor<ParamItemViewModel, ParamItemView>();
            builder.ViewLocator.RegisterViewFor<
                TryCloseWithApprovalDialogViewModel,
                TryCloseWithApprovalDialogView
            >();
            return this;
        }

        public Builder UseFileBrowser()
        {
            builder.Shell.Pages.Register<FileBrowserViewModel, FileBrowserView>(
                FileBrowserViewModel.PageId
            );
            builder.Shell.Pages.Home.UseItemExtension<HomePageFileBrowserDeviceItemAction>();
            builder.ViewLocator.RegisterViewFor<
                BurstDownloadDialogViewModel,
                BurstDownloadDialogView
            >();
            return this;
        }

        public Builder UseFlightMode()
        {
            builder.Shell.Pages.Register<FlightPageViewModel, FlightPageView>(
                FlightPageViewModel.PageId
            );
            builder.Shell.Pages.Home.UseExtension<HomePageFlightExtension>();
            builder.Extensions.Register<IFlightMode, FlightUavAnchorsExtension>();
            builder.Extensions.Register<IFlightMode, FlightWidgetsExtension>();
            builder.ViewLocator.RegisterViewFor<UavWidgetViewModel, UavWidgetView>();
            builder.ViewLocator.RegisterViewFor<MissionProgressViewModel, MissionProgressView>();
            builder.ViewLocator.RegisterViewFor<
                SetAltitudeDialogViewModel,
                SetAltitudeDialogView
            >();
            return this;
        }

        public Builder UseExtendableFlightMode()
        {
            builder.Shell.Pages.Register<FlightModePageViewModel, FlightModePageView>(
                FlightModePageViewModel.PageId
            );
            builder.Shell.Pages.Home.UseExtension<HomePageFlightModeExtension>();
            builder.Extensions.Register<IFlightModePage, FlightWidgetExtension>();

            builder.Services.AddTransient<IDroneFlightWidget, DroneFlightWidgetViewModel>();
            builder.ViewLocator.RegisterViewFor<
                DroneFlightWidgetViewModel,
                DroneFlightWidgetView
            >();

            builder.Extensions.Register<IDroneFlightWidget, TelemetryDroneFlightWidgetExtension>();
            builder.ViewLocator.RegisterViewFor<TelemetryViewModel, TelemetryView>();

            builder.Extensions.Register<IFlightModePage, FlightModeAnchorsExtension>();

            builder.Services.AddKeyedTransient<IDashboardWidget, AttitudeIndicatorViewModel>(
                AttitudeIndicatorViewModel.WidgetId
            );
            builder.Extensions.Register<
                IDroneFlightWidget,
                AttitudeIndicatorDroneFlightWidgetExtension
            >();
            builder.ViewLocator.RegisterViewFor<
                AttitudeIndicatorViewModel,
                AttitudeIndicatorView
            >();

            builder.Extensions.Register<
                IDroneFlightWidget,
                FlightControlDroneFlightWidgetExtension
            >();
            builder.ViewLocator.RegisterViewFor<FlightControlViewModel, FlightControlView>();

            // Plugin extends flight mode
            builder.ViewLocator.RegisterViewFor<PluginFlightItemViewModel, PluginFlightItemView>();
            builder.Extensions.Register<IFlightModePage, PluginFlightItemWidgetExtension>();
            return this;
        }

        public Builder UseOptionalPacketViewer()
        {
            builder.Shell.Pages.Register<PacketViewerViewModel, PacketViewerView>(
                PacketViewerViewModel.PageId
            );
            builder.Shell.Pages.Home.UseExtension<HomePacketViewerExtension>();
            builder.ViewLocator.RegisterViewFor<PacketMessageViewModel, PacketMessageView>();
            builder.Services.AddSingleton<IPacketConverter, DefaultMavlinkPacketConverter>();
            builder.ViewLocator.RegisterViewFor<
                SavePacketMessagesDialogViewModel,
                SavePacketMessagesDialogView
            >();
            return this;
        }
    }
}
