using System.Composition;
using Asv.Avalonia;
using Asv.Common;
using Microsoft.Extensions.Logging;
using R3;

namespace Asv.Drones;

[ExportExtensionFor<IHomePage>]
[method: ImportingConstructor]
public class HomePacketViewerExtension(ILoggerFactory loggerFactory) : IExtensionFor<IHomePage>
{
    public void Extend(IHomePage context, CompositeDisposable contextDispose)
    {
        context.Tools.Add(
            OpenPacketViewerCommand
                .StaticInfo.CreateAction(loggerFactory)
                .DisposeItWith(contextDispose)
        );
    }
}
