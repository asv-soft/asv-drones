using System.Composition;
using Asv.Avalonia;
using Asv.Drones.Api;

namespace Asv.Drones;

[ExportCommand]
[method: ImportingConstructor]
public class OpenFlightModeCommand(INavigationService nav)
    : OpenPageCommandBase(FlightMode.PageId, nav)
{
    #region Static

    public const string Id = $"{BaseId}.open.flight";

    public static readonly ICommandInfo StaticInfo = new CommandInfo
    {
        Id = Id,
        Name = RS.OpenFlightModeCommand_CommandInfo_Name,
        Description = RS.OpenFlightModeCommand_CommandInfo_Description,
        Icon = FlightMode.PageIcon,
        Source = SystemModule.Instance,
        DefaultHotKey = "Ctrl+F2",
    };

    #endregion
    public override ICommandInfo Info => StaticInfo;
}
