using System.Composition.Hosting;
using Asv.Avalonia;

namespace Asv.Drones.Api;

public class ApiModule : IExportInfo
{
    public const string Name = "Asv.Drones.Api";
    public static IExportInfo Instance { get; } = new ApiModule();

    private ApiModule() { }

    public string ModuleName => Name;
}

public static class ContainerConfigurationMixin
{
    public static ContainerConfiguration WithDependenciesFromApi(
        this ContainerConfiguration containerConfiguration
    )
    {
        return containerConfiguration.WithAssemblies([typeof(ApiModule).Assembly]);
    }
}
