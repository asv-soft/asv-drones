using Asv.Avalonia;
using Microsoft.Extensions.DependencyInjection;

namespace Asv.Drones.Api;

public static class Commands
{
    private static IFlightModeCommands? _flightMode;
    private static IMavlinkCommands? _mavlink;

    public static IMavlinkCommands Mavlink =>
        _mavlink ??= AppHost.Instance.Services.GetRequiredService<IMavlinkCommands>();

    public static IFlightModeCommands FlightMode =>
        _flightMode ??= AppHost.Instance.Services.GetRequiredService<IFlightModeCommands>();
}
