using System;
using Asv.Avalonia;
using Asv.Avalonia.IO;
using Asv.Drones.Api;
using Asv.Drones.Plane;
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
                .UseExtendableFlightMode()
                .UseActions()
                .UseFileBrowser()
                .UseSetupPage();
        }

        public Builder UseControls()
        {
            builder.ViewLocator.RegisterViewFor<
                VelocityTelemetryItemViewModel,
                SplitDigitRttBoxView
            >();
            builder.ViewLocator.RegisterViewFor<
                BatteryTelemetryItemViewModel,
                KeyValueRttBoxView
            >();
            return this;
        }

        public Builder UseActions()
        {
            /*builder.Register<ChangeFrameTypeCommand>()
                .Register<ExportPacketsToCsvCommand>()
                .Register<ClearAllPacketsCommand>()
                .Register<UpdateParamsCommand>()
                .Register<StopUpdateParamsCommand>()
                .Register<RemoveAllPinsCommand>()
                .Register<MavlinkParamsWriteCommand>()
                .Register<MavlinkParamReadCommand>()
                .Register<TakeOffCommand>()
                .Register<StartMissionCommand>()
                .Register<RTLCommand>()
                .Register<LandCommand>()
                .Register<GuidedModeCommand>()
                .Register<AutoModeCommand>()
                .Register<UpdateMissionCommand>()
                .Register<UploadItemCommand>()
                .Register<DownloadItemCommand>()
                .Register<BurstDownloadItemCommand>()
                .Register<CreateDirectoryCommand>()
                .Register<CalculateCrc32Command>()
                .Register<FindFileCommand>()
                .Register<CommitRenameCommand>()
                .Register<RemoveItemCommand>();*/
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
            builder.ViewLocator.RegisterPropertyEditor();

            builder.UseMavParam();

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

        public Builder UseExtendableFlightMode()
        {
            // FlightMode
            builder.Shell.Pages.Register<FlightModePageViewModel, FlightModePageView>(
                FlightModePageViewModel.PageId
            );
            builder.Shell.Pages.Home.UseExtension<HomePageFlightModeExtension>();

            // Anchors
            builder.Extensions.Register<IFlightModePage, FlightModeAnchorsExtension>();

            // Factory for client device widgets
            builder.Services.AddSingleton<IClientDeviceWidgetFactory, ClientDeviceWidgetFactory>();

            // Create widgets for client devices
            builder.Extensions.Register<IFlightModePage, FlightModeClientDeviceWidgetExtension>();

            // Widget for all drones
            builder.Services.AddSingleton<
                IClientDeviceWidgetCreationHandler,
                DroneWidgetCreationHandler
            >();
            builder.ViewLocator.RegisterViewFor<DroneFlightWidgetViewModel, FlightWidgetView>();

            // Sections for the drone Widget
            builder.Extensions.Register<
                IDroneFlightWidget,
                DroneFlightWidgetTelemetrySectionExtension
            >();
            builder.ViewLocator.RegisterViewFor<ITelemetrySection, TelemetrySectionView>();
            builder.ViewModel.RegisterWithArgs<
                ITelemetrySection,
                TelemetrySectionViewModel,
                TelemetrySectionArgs
            >();
            builder.ViewLocator.RegisterViewFor<
                TelemetryDisplayItemViewModel,
                TelemetryDisplayItemView
            >();
            builder.ViewLocator.RegisterViewFor<
                AddTelemetryDisplayItemViewModel,
                AddTelemetryDisplayItemView
            >();
            builder.ViewLocator.RegisterViewFor<
                ConfigureTelemetryDialogViewModel,
                ConfigureTelemetryDialogView
            >();
            builder.ViewLocator.RegisterViewFor<
                CurrentFlightModeTelemetryItemViewModel,
                SingleRttBoxView
            >();
            builder.ViewLocator.RegisterViewFor<
                AzimuthTelemetryItemViewModel,
                SplitDigitRttBoxView
            >();
            builder.ViewLocator.RegisterViewFor<
                AltitudeTelemetryItemViewModel,
                TwoColumnRttBoxView
            >();
            builder.ViewLocator.RegisterViewFor<AngleTelemetryItemViewModel, TwoColumnRttBoxView>();
            builder.ViewLocator.RegisterViewFor<
                BatteryTelemetryItemViewModel,
                KeyValueRttBoxView
            >();
            builder.ViewLocator.RegisterViewFor<
                HeadingTelemetryItemViewModel,
                SplitDigitRttBoxView
            >();
            builder.ViewLocator.RegisterViewFor<
                HomeAzimuthTelemetryItemViewModel,
                SplitDigitRttBoxView
            >();
            builder.ViewLocator.RegisterViewFor<
                VelocityTelemetryItemViewModel,
                SplitDigitRttBoxView
            >();
            builder.ViewLocator.RegisterViewFor<
                LinkQualityTelemetryItemViewModel,
                SplitDigitRttBoxView
            >();
            builder.ViewLocator.RegisterViewFor<GnssTelemetryItemViewModel, KeyValueRttBoxView>();
            builder.ViewLocator.RegisterViewFor<
                DistanceTelemetryItemViewModel,
                SplitDigitRttBoxView
            >();
            builder.ViewLocator.RegisterViewFor<
                MissionDistanceTelemetryItemViewModel,
                KeyValueRttBoxView
            >();
            builder.ViewLocator.RegisterViewFor<
                MissionProgressTelemetryItemViewModel,
                KeyValueRttBoxView
            >();
            builder.ViewLocator.RegisterViewFor<
                MissionTargetTelemetryItemViewModel,
                KeyValueRttBoxView
            >();

            builder.Services.AddSingleton<ITelemetryItemFactory, AltitudeTelemetryItemFactory>();
            builder.Services.AddSingleton<ITelemetryItemFactory, BatteryTelemetryItemFactory>();
            builder.Services.AddSingleton<ITelemetryItemFactory, VelocityTelemetryItemFactory>();
            builder.Services.AddSingleton<ITelemetryItemFactory, AngleTelemetryItemFactory>();
            builder.Services.AddSingleton<ITelemetryItemFactory, HeadingTelemetryItemFactory>();
            builder.Services.AddSingleton<ITelemetryItemFactory, HomeAzimuthTelemetryItemFactory>();
            builder.Services.AddSingleton<
                ITelemetryItemFactory,
                CurrentFlightModeTelemetryItemFactory
            >();
            builder.Services.AddSingleton<ITelemetryItemFactory, AzimuthTelemetryItemFactory>();
            builder.Services.AddSingleton<ITelemetryItemFactory, LinkQualityTelemetryItemFactory>();
            builder.Services.AddSingleton<ITelemetryItemFactory, GnssTelemetryItemFactory>();
            builder.Services.AddSingleton<
                ITelemetryItemFactory,
                MissionDistanceTelemetryItemFactory
            >();
            builder.Services.AddSingleton<
                ITelemetryItemFactory,
                MissionProgressTelemetryItemFactory
            >();
            builder.Services.AddSingleton<
                ITelemetryItemFactory,
                HomeDistanceTelemetryItemFactory
            >();
            builder.Services.AddSingleton<
                ITelemetryItemFactory,
                MissionTargetTelemetryItemFactory
            >();

            builder.Extensions.Register<
                IDroneFlightWidget,
                DroneFlightWidgetAttitudeIndicatorSectionExtension
            >();
            builder.ViewLocator.RegisterViewFor<
                AttitudeIndicatorSectionViewModel,
                AttitudeIndicatorSectionView
            >();

            // Actions for flight widgets
            builder.Extensions.Register<IDroneFlightWidget, AutoModeAction<IDroneFlightWidget>>();
            builder.Extensions.Register<IDroneFlightWidget, GuidedAction<IDroneFlightWidget>>();
            builder.Extensions.Register<IDroneFlightWidget, TakeOffAction<IDroneFlightWidget>>();
            builder.Extensions.Register<IDroneFlightWidget, LandAction<IDroneFlightWidget>>();
            builder.Extensions.Register<IDroneFlightWidget, RtlAction<IDroneFlightWidget>>();
            builder.Extensions.Register<IDroneFlightWidget, GotoAction<IDroneFlightWidget>>();
            builder.Extensions.Register<IDroneFlightWidget, RoiAction<IDroneFlightWidget>>();
            builder.Extensions.Register<IDroneFlightWidget, FindDroneAction<IDroneFlightWidget>>();

            builder.Extensions.Register<IPlaneWidget, AutoModeAction<IPlaneWidget>>();
            builder.Extensions.Register<IPlaneWidget, GuidedAction<IPlaneWidget>>();
            builder.Extensions.Register<IPlaneWidget, TakeOffAction<IPlaneWidget>>();
            builder.Extensions.Register<IPlaneWidget, RtlAction<IPlaneWidget>>();
            builder.Extensions.Register<IPlaneWidget, GotoAction<IPlaneWidget>>();
            builder.Extensions.Register<IPlaneWidget, RoiAction<IPlaneWidget>>();
            builder.Extensions.Register<IPlaneWidget, FindDroneAction<IPlaneWidget>>();

            builder.ViewLocator.RegisterViewFor<
                SetAltitudeDialogViewModel,
                SetAltitudeDialogView
            >();
            builder.ViewLocator.RegisterViewFor<
                SetAltitudeDialogViewModel,
                SetAltitudeDialogView
            >();

            // Test plugin widget
            builder.ViewLocator.RegisterViewFor<PluginFlightItemViewModel, PluginFlightItemView>();
            builder.Extensions.Register<IFlightModePage, PluginFlightItemWidgetExtension>();

            // Test plane widget
            builder.Services.AddSingleton<
                IClientDeviceWidgetCreationHandler,
                PlaneWidgetCreationHandler
            >();
            builder.ViewLocator.RegisterViewFor<PlaneWidgetViewModel, FlightWidgetView>();

            // Test plane section
            builder.Extensions.Register<IPlaneWidget, PlaneSectionExtension>();
            builder.ViewLocator.RegisterViewFor<PlaneSectionViewModel, PlaneSectionView>();

            builder.Extensions.Register<IPlaneWidget, PlaneFlightWidgetTelemetrySectionExtension>();

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
