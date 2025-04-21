using Asv.Avalonia;
using Asv.Common;
using Asv.Drones.Api;
using R3;

namespace Asv.Drones;

[ExportExtensionFor<IHomePage>]
public class HomePageFlightExtension : IExtensionFor<IHomePage>
{
    public void Extend(IHomePage context, CompositeDisposable contextDispose)
    {
        context.Tools.Add(
            OpenFlightModeCommand.StaticInfo.CreateAction().DisposeItWith(contextDispose)
        );
    }
}
