using System;
using Asv.Avalonia;
using Asv.Common;
using Asv.Modeling;
using Microsoft.Extensions.Hosting;
using R3;

namespace Asv.Drones;

public class HomePageFlightModeExtension : IExtensionFor<IHomePage>
{
    public void Extend(IHomePage context, CompositeDisposable contextDispose)
    {
        context.Tools.Add(
            new ActionViewModel(FlightModePageViewModel.PageId)
            {
                Header = "Open Flight Mode (BETA)",
                Description = "Command opens Flight Mode (BETA)",
                Icon = FlightModePageViewModel.PageIcon,
                Command = new ReactiveCommand(
                    async (_, _) =>
                        await context.GoTo(
                            new NavPath(new NavId(FlightModePageViewModel.PageId, NavArgs.Empty))
                        )
                ),
            }.DisposeItWith(contextDispose)
        );
    }
}

public static class FlightPageMixin
{
    extension(PageMixin.Builder builder)
    {
        public PageMixin.Builder WithFlightPage(Action<Builder>? configure = null)
        {
            builder.Register<FlightModePageViewModel, FlightModePageView>(
                FlightModePageViewModel.PageId
            );
            builder.Parent.Parent.Extensions.Register<IHomePage, HomePageFlightModeExtension>();
            configure ??= b => b.UseDefault();
            configure(new Builder(builder));
            return builder;
        }

        public Builder FlightPage => new(builder);
    }

    public class Builder(PageMixin.Builder builder)
    {
        public PageMixin.Builder Parent => builder;

        public void UseDefault() { }
    }
}
