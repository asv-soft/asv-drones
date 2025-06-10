using Asv.Avalonia;

namespace Asv.Drones.Api;

public class ApiModule : IExportInfo
{
    public const string Name = "Asv.Drones.Api";
    public static IExportInfo Instance { get; } = new ApiModule();

    private ApiModule() { }

    public string ModuleName => Name;
}
