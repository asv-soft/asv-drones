using System.Composition;
using Asv.Avalonia;
using Asv.Common;
using Microsoft.Extensions.Logging;
using R3;

namespace Asv.Drones;

[ExportExtensionFor<IHomePage>]
[method: ImportingConstructor]
public class HomePageFlightExtension(ILayoutService layoutService, ILoggerFactory loggerFactory)
    : IExtensionFor<IHomePage>
{
    public void Extend(IHomePage context, CompositeDisposable contextDispose)
    {
        context.Tools.Add(
            OpenFlightModeCommand
                .StaticInfo.CreateAction(layoutService, loggerFactory)
                .DisposeItWith(contextDispose)
        );
    }
}
