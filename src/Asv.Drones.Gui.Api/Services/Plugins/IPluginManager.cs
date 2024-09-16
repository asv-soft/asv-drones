using Asv.Common;

namespace Asv.Drones.Gui.Api;

public class PluginServer(string name, string sourceUri, string? username = null, string? password = null)
{
    public string Name => name;
    public string SourceUri => sourceUri;
    public string? Username => username;
    public string? Password => password;
}

public interface IPluginServerInfo
{
    public string Name { get; }
    public string SourceUri { get; }
    public string? Username { get; }
}

public interface IPluginManager
{
    IReadOnlyList<IPluginServerInfo> Servers { get; }
    void AddServer(PluginServer server);
    void RemoveServer(IPluginServerInfo server);
    Task<IReadOnlyList<IPluginSearchInfo>> Search(SearchQuery query, CancellationToken cancel);
    Task<IReadOnlyList<string>> ListPluginVersions(SearchQuery query, string pluginId, CancellationToken cancel);

    Task Install(IPluginServerInfo source, string packageId, string version, IProgress<ProgressMessage>? progress,
        CancellationToken cancel);
    
    Task InstallManually(string from, IProgress<ProgressMessage>? progress,
        CancellationToken cancel);

    void Uninstall(ILocalPluginInfo plugin);
    void CancelUninstall(ILocalPluginInfo pluginInfo);
    IEnumerable<ILocalPluginInfo> Installed { get; }
    bool IsInstalled(string packageId, out ILocalPluginInfo? info);
    SemVersion ApiVersion { get; }
}

public class SearchQuery
{
    public static readonly SearchQuery Empty = new()
    {
        Name = null,
        IncludePrerelease = false,
        Skip = 0,
        Take = 20
    };

    public string? Name { get; set; }
    public bool IncludePrerelease { get; set; }
    public int Skip { get; set; } = 0;
    public int Take { get; set; } = 20;
    public HashSet<string> Sources { get; } = new();
}