using Asv.Avalonia;
using Asv.Drones.Api;

namespace Asv.Drones;

public class OpenFlightCommand(INavigationService nav)
    : OpenPageCommandBase(FlightModePageViewModel.PageId, nav)
{
    #region Static

    public const string Id = $"{BaseId}.open.{FlightModePageViewModel.PageId}";

    public static readonly ICommandInfo StaticInfo = new CommandInfo
    {
        Id = Id,
        Name = RS.OpenFlightModeCommand_CommandInfo_Name,
        Description = RS.OpenFlightModeCommand_CommandInfo_Description,
        Icon = FlightMode.PageIcon,
        DefaultHotKey = "Ctrl+F2",
    };

    #endregion
    public override ICommandInfo Info => StaticInfo;
}
