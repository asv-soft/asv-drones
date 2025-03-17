using Asv.Avalonia;

namespace Asv.Drones.Api;

public class Module : IExportInfo
{
    public const string Name = "Asv.Drones.Api";
    public static IExportInfo Instance { get; } = new Module();

    private Module() { }

    public string ModuleName => Name;
}
