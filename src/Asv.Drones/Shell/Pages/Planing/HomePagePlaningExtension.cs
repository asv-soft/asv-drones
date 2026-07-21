using Asv.Avalonia;
using Asv.Common;
using Asv.Modeling;
using R3;

namespace Asv.Drones;

public class HomePagePlaningExtension : IExtensionFor<IHomePage>
{
    public const string StaticId = "ext.home.planing";

    string ISupportId<string>.Id => StaticId;

    public void Extend(IHomePage context, CompositeDisposable contextDispose)
    {
        context.Tools.Add(
            new ActionViewModel(FlightModePageViewModel.PageId)
            {
                Header = "Planing",
                Description = "Tool for create missions and planing flights",
                Icon = FlightModePageViewModel.PageIcon,
                Command = new ReactiveCommand(
                    async (_, cancel) =>
                        await context.GoTo(
                            new NavPath(new NavId(PlaningPageViewModel.PageId, NavArgs.Empty)),
                            cancel
                        )
                ),
            }.DisposeItWith(contextDispose)
        );
    }
}
