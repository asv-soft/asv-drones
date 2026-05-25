using Asv.Avalonia;
using Asv.Common;
using Asv.Drones.Api;
using Asv.Modeling;
using R3;

namespace Asv.Drones;

public class HomePageFlightExtension : IExtensionFor<IHomePage>
{
    public void Extend(IHomePage context, CompositeDisposable contextDispose)
    {
        context.Tools.Add(
            new ActionViewModel(FlightMode.PageId)
            {
                Header = RS.OpenFlightModeCommand_CommandInfo_Name,
                Description = RS.OpenFlightModeCommand_CommandInfo_Description,
                Icon = FlightMode.PageIcon,
                Command = new ReactiveCommand(
                    async (_, _) =>
                        await context.GoTo(new NavPath(new NavId(FlightMode.PageId, NavArgs.Empty)))
                ),
            }.DisposeItWith(contextDispose)
        );
    }
}
