using Asv.Drones.Gui.Api;

namespace Asv.Drones.Gui;

public class AppPathInfo : IAppPathInfo
{
    public required string AppDataFolder { get; set; }
}