using Asv.Avalonia;
using Asv.Common;
using Asv.Modeling;
using R3;

namespace Asv.Drones;

public class HomePacketViewerExtension : IExtensionFor<IHomePage>
{
    public const string StaticId = "ext.home.packet-viewer";

    string ISupportId<string>.Id => StaticId;

    public void Extend(IHomePage context, CompositeDisposable contextDispose)
    {
        context.Tools.Add(
            new ActionViewModel(PacketViewerViewModel.PageId)
            {
                Header = RS.OpenPacketViewerCommand_CommandInfo_Name,
                Description = RS.OpenPacketViewerCommand_CommandInfo_Description,
                Icon = PacketViewerViewModel.PageIcon,
                Command = new ReactiveCommand(
                    async (_, cancel) =>
                        await context.GoTo(
                            new NavPath(new NavId(PacketViewerViewModel.PageId, NavArgs.Empty)),
                            cancel
                        )
                ),
            }.DisposeItWith(contextDispose)
        );
    }
}
