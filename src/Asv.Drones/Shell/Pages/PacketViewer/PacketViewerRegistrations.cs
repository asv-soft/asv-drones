using Asv.Avalonia;

namespace Asv.Drones;

public static class PacketViewerRegistrations
{
    extension(PagesRegistrations.Builder builder)
    {
        public PagesRegistrations.Builder RegisterPacketViewer()
        {
            builder.AppBuilder.Shell.Pages.Register<PacketViewerViewModel, PacketViewerView>(
                PacketViewerViewModel.PageId
            );
            builder.AppBuilder.Shell.Pages.Home.UseExtension<HomePacketViewerExtension>();
            builder.AppBuilder.ViewLocator.RegisterViewFor<
                PacketMessageViewModel,
                PacketMessageView
            >();
            builder.AppBuilder.ViewLocator.RegisterViewFor<
                SavePacketMessagesDialogViewModel,
                SavePacketMessagesDialogView
            >();
            return builder;
        }
    }
}
