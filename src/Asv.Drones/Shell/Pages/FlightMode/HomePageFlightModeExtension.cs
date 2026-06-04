using Asv.Avalonia;
using Asv.Common;
using Asv.Modeling;
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
