using System;
using System.Linq;
using Asv.Common;
using Asv.Drones.Gui.Api;
using NuGet.Protocol.Core.Types;

namespace Asv.Drones.Gui;

internal class PluginSearchInfo : IPluginSearchInfo
{
    public PluginSearchInfo(IPackageSearchMetadata packageSearchMetadata, SourceRepository repository,
        SourcePackageDependencyInfo dependencyInfo)
    {
        Authors = packageSearchMetadata.Authors;
        Title = packageSearchMetadata.Identity.Id.Replace(PluginManager.PluginSearchTermStartWith, string.Empty);
        LastVersion = packageSearchMetadata.Identity.Version.ToString();
        Source = new SourceInfo(repository);
        PackageId = packageSearchMetadata.Identity.Id;
        Description = packageSearchMetadata.Description;
        Tags = packageSearchMetadata.Tags;
        DownloadCount = packageSearchMetadata.DownloadCount;
        
        var apiPackage = dependencyInfo.Dependencies.FirstOrDefault(x => x.Id == NugetHelper.PluginApiPackageName);
        if (apiPackage == null)
        {
            throw new Exception($"Plugin {packageSearchMetadata.Identity.Id} does not contain API package as dependency");
        }
        ApiVersion = apiPackage.VersionRange.MinVersion?.ToNormalizedString() ?? throw new InvalidOperationException("Api version not found in plugin dependencies");
    }

    public IPluginServerInfo Source { get; }
    public SemVersion ApiVersion { get; }
    public string PackageId { get; }
    public string? Title { get; }
    public string? Authors { get; }
    public string LastVersion { get; }
    public string Description { get; }
    public long? DownloadCount { get; }
    public string? Tags { get; }
}
