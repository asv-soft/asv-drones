using System.Composition;
using Asv.Avalonia;

namespace Asv.Drones;

[ExportCommand]
[method: ImportingConstructor]
public class OpenSetupCommand(INavigationService nav)
    : OpenPageCommandBase(SetupPageViewModel.PageId, nav)
{
    #region Static

    public const string Id = $"{BaseId}.open.{SetupPageViewModel.PageId}";

    public static readonly ICommandInfo StaticInfo = new CommandInfo
    {
        Id = Id,
        Name = RS.OpenSetupCommand_CommandInfo_Name,
        Description = RS.OpenSetupCommand_CommandInfo_Description,
        Icon = SetupPageViewModel.PageIcon,
        DefaultHotKey = null,
        Source = SystemModule.Instance,
    };

    #endregion

    public override ICommandInfo Info => StaticInfo;
}
