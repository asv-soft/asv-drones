using Asv.Drones.Api;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Asv.Drones;

public static class MavlinkCommandsMixin
{
    public static IHostApplicationBuilder RegisterMavlinkCommands(
        this IHostApplicationBuilder builder
    )
    {
        builder.Services.AddSingleton<IMavlinkCommands, MavlinkCommands>();
        return builder;
    }
}
