using Asv.Common;

namespace Asv.Drones.Gui.Api;

public class NullPluginManager : IPluginManager
{
    public static IPluginManager Instance { get; } = new NullPluginManager();

    public IReadOnlyList<IPluginServerInfo> Sources { get; } = new List<IPluginServerInfo>();


    public IReadOnlyList<IPluginServerInfo> Servers { get; }

    public void AddServer(PluginServer server)
    {
    }

    public void RemoveServer(IPluginServerInfo server)
    {
    }

    public Task<IReadOnlyList<IPluginSearchInfo>> Search(SearchQuery query, CancellationToken cancel)
    {
        return Task.FromResult((IReadOnlyList<IPluginSearchInfo>)new List<IPluginSearchInfo>());
    }

    public Task Install(IPluginServerInfo source, string packageId, string version,
        IProgress<ProgressMessage>? progress, CancellationToken cancel)
    {
        return Task.CompletedTask;
    }

    public void Uninstall(ILocalPluginInfo plugin)
    {
    }

    public void CancelUninstall(ILocalPluginInfo pluginInfo)
    {
    }

    public IEnumerable<ILocalPluginInfo> Installed { get; } = new List<ILocalPluginInfo>();

    public bool IsInstalled(string packageId, out ILocalPluginInfo? info)
    {
        info = null;
        return false;
    }

    public SemVersion ApiVersion { get; } = new(0);
}