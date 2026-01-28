using Asv.Avalonia;

namespace Asv.Drones.Api;

public static class Commands
{
    private static IFlightModeCommands? _flightMode;
    private static IMavlinkCommands? _mavlink;

    public static IMavlinkCommands Mavlink =>
        _mavlink ??= AppHost.Instance.GetService<IMavlinkCommands>();

    public static IFlightModeCommands FlightMode =>
        _flightMode ??= AppHost.Instance.GetService<IFlightModeCommands>();
}
