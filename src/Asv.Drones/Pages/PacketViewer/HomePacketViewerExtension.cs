using Asv.Avalonia;
using Asv.Common;
using R3;

namespace Asv.Drones;

[ExportExtensionFor<IHomePage>]
public class HomePacketViewerExtension : IExtensionFor<IHomePage>
{
    public void Extend(IHomePage context, CompositeDisposable contextDispose)
    {
        context.Tools.Add(
            OpenPacketViewerCommand.StaticInfo.CreateAction().DisposeItWith(contextDispose)
        );
    }
}
