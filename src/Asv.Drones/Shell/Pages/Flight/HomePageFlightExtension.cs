using Asv.Avalonia;
using Asv.Common;
using Microsoft.Extensions.Logging;
using R3;

namespace Asv.Drones;

public class HomePageFlightExtension(ILoggerFactory loggerFactory) : IExtensionFor<IHomePage>
{
    public void Extend(IHomePage context, CompositeDisposable contextDispose)
    {
        context.Tools.Add(
            OpenFlightModeCommand
                .StaticInfo.CreateAction(loggerFactory)
                .DisposeItWith(contextDispose)
        );
    }
}
