using Asv.Drones.Gui.Api;
using NuGet.Protocol.Core.Types;

namespace Asv.Drones.Gui;

internal class PluginSearchInfo : IPluginSearchInfo
{
    public PluginSearchInfo(IPackageSearchMetadata packageSearchMetadata, SourceRepository repository)
    {
        Authors = packageSearchMetadata.Authors;
        Title = packageSearchMetadata.Identity.Id.Replace(PluginManager.PluginSearchTermStartWith, string.Empty);
        LastVersion = packageSearchMetadata.Identity.Version.ToString();
        Source = new SourceInfo(repository);
        PackageId = packageSearchMetadata.Identity.Id;
        Description = packageSearchMetadata.Description;
        Tags = packageSearchMetadata.Tags;
        DownloadCount = packageSearchMetadata.DownloadCount;
    }

    public IPluginServerInfo Source { get; }
    public string PackageId { get; }
    public string? Title { get; }
    public string? Authors { get; }
    public string LastVersion { get; }
    public string Description { get; }
    public long? DownloadCount { get; }
    public string? Tags { get; }
}

public class DesginTimePluginSearchInfo : IPluginSearchInfo
{
    public IPluginServerInfo Source { get; } = new DesingTimeSourceInfo();
    public string PackageId { get; set; }
    public string? Title { get; set; }
    public string? Authors { get; set; }
    public string LastVersion { get; set; }
    public string Description { get; set; }
    public long? DownloadCount { get; set; }
    public string? Tags { get; set; }
}

public class DesingTimeSourceInfo : IPluginServerInfo
{
    public string SourceUri { get; set; }
    public string Name { get; set; }
    public string? Username { get; set; }
}