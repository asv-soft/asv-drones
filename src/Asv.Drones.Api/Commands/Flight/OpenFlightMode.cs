using System.Composition;
using Asv.Avalonia;
using Avalonia.Input;

namespace Asv.Drones.Api;

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
        Name = "Flight mode",
        Description = "Open flight mode map",
        Icon = FlightMode.PageIcon,
        HotKeyInfo = new HotKeyInfo { DefaultHotKey = KeyGesture.Parse("Ctrl+P") },
        Source = SystemModule.Instance,
    };

    #endregion
    public override ICommandInfo Info => StaticInfo;
}
