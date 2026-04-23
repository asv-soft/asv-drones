using Asv.Avalonia;

namespace Asv.Drones;

public class OpenFlightCommand(INavigationService nav)
    : OpenPageCommandBase(FlightModePageViewModel.PageId, nav)
{
    #region Static

    public const string Id = $"{BaseId}.open.{FlightModePageViewModel.PageId}";

    public static readonly ICommandInfo StaticInfo = new CommandInfo
    {
        Id = Id,
        Name = "Open Flight Mode (BETA)",
        Description = "Command opens Flight Mode (BETA)",
        Icon = FlightModePageViewModel.PageIcon,
        DefaultHotKey = null, // TODO: add after BETA
    };

    #endregion
    public override ICommandInfo Info => StaticInfo;
}
