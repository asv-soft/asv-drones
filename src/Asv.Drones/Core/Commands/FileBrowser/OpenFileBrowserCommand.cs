using Asv.Avalonia;

namespace Asv.Drones;

public class OpenFileBrowserCommand(INavigationService nav)
    : OpenPageCommandBase(FileBrowserViewModel.PageId, nav)
{
    #region Static

    public const string Id = $"{BaseId}.open.{FileBrowserViewModel.PageId}";

    public static readonly ICommandInfo StaticInfo = new CommandInfo
    {
        Id = Id,
        Name = RS.OpenFileBrowserCommand_CommandInfo_Name,
        Description = RS.OpenFileBrowserCommand_CommandInfo_Description,
        Icon = FileBrowserViewModel.PageIcon,
        DefaultHotKey = null,
    };

    #endregion

    public override ICommandInfo Info => StaticInfo;
}
