using Asv.Avalonia;
using Asv.Drones.Api;
using Microsoft.Extensions.DependencyInjection;

namespace Asv.Drones;

public static class PlaningRegistrations
{
    extension(PagesRegistrations.Builder builder)
    {
        public PagesRegistrations.Builder RegisterPlaning()
        {
            builder.AppBuilder.Shell.Pages.Register<PlaningPageViewModel, PlaningPageView>(
                PlaningPageViewModel.PageId
            );
            builder.AppBuilder.Shell.Pages.Home.UseExtension<HomePagePlaningExtension>();
            return builder;
        }
    }
}
