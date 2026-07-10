using Asv.Avalonia;
using Asv.Drones.Api;
using Microsoft.Extensions.DependencyInjection;

namespace Asv.Drones;

public static class FileBrowserRegistrations
{
    extension(PagesRegistrations.Builder builder)
    {
        public PagesRegistrations.Builder RegisterFileBrowser()
        {
            builder.AppBuilder.Shell.Pages.Register<FileBrowserViewModel, FileBrowserView>(
                FileBrowserViewModel.PageId
            );
            builder.AppBuilder.Shell.Pages.Home.UseItemExtension<HomePageFileBrowserDeviceItemAction>();
            builder.AppBuilder.ViewLocator.RegisterViewFor<
                BurstDownloadDialogViewModel,
                BurstDownloadDialogView
            >();
            return builder;
        }
    }
}
