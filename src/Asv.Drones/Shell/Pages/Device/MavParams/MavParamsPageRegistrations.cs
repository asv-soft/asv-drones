using Asv.Avalonia;
using Asv.Drones.Api;
using Microsoft.Extensions.DependencyInjection;

namespace Asv.Drones;

public static class MavParamsPageRegistrations
{
    extension(PagesRegistrations.Builder builder)
    {
        public PagesRegistrations.Builder RegisterMavParams()
        {
            builder.AppBuilder.RegisterMavParams();

            builder.AppBuilder.Shell.Pages.Register<MavParamsPageViewModel, MavParamsPageView>(
                MavParamsPageViewModel.PageId
            );
            builder.AppBuilder.Shell.Pages.Home.UseItemExtension<HomePageParamsDeviceItemAction>();
            builder.AppBuilder.ViewLocator.RegisterViewFor<ParamItemViewModel, ParamItemView>();
            return builder;
        }
    }
}
