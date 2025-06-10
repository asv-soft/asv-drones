using System.Composition;
using Asv.Avalonia;

namespace Asv.Drones;

[ExportCommand]
[method: ImportingConstructor]
public class OpenMavParamsCommand(INavigationService nav)
    : OpenPageCommandBase(MavParamsPageViewModel.PageId, nav)
{
    #region Static

    public const string Id = $"{BaseId}.open.{MavParamsPageViewModel.PageId}";

    public static readonly ICommandInfo StaticInfo = new CommandInfo
    {
        Id = Id,
        Name = RS.OpenMavParamsCommand_CommandInfo_Name,
        Description = RS.OpenMavParamsCommand_CommandInfo_Description,
        Icon = MavParamsPageViewModel.PageIcon,
        DefaultHotKey = null,
        Source = SystemModule.Instance,
    };

    #endregion
    public override ICommandInfo Info => StaticInfo;
}
