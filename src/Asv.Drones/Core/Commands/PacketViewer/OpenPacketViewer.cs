using Asv.Avalonia;

namespace Asv.Drones;

public class OpenPacketViewerCommand(INavigationService nav)
    : OpenPageCommandBase(PacketViewerViewModel.PageId, nav)
{
    #region Static
    public const string Id = $"{BaseId}.open.{PacketViewerViewModel.PageId}";

    public static readonly ICommandInfo StaticInfo = new CommandInfo
    {
        Id = Id,
        Name = RS.OpenPacketViewerCommand_CommandInfo_Name,
        Description = RS.OpenPacketViewerCommand_CommandInfo_Description,
        Icon = PacketViewerViewModel.PageIcon,
        DefaultHotKey = null,
    };
    #endregion

    public override ICommandInfo Info => StaticInfo;
}
