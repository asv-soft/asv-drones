using Asv.Common;
using Avalonia.Media.Imaging;
using NuGet.Packaging.Core;

namespace Asv.Drones.Gui.Api;

public interface ILocalPluginInfo : IPluginSpecification
{
    string Id => $"{SourceUri}|{PackageId}";
    string SourceUri { get; }
    string LocalFolder { get; }
    string Version { get; }
    bool IsUninstalled { get; }
    bool IsLoaded { get; }
    string LoadingError { get; }
    Bitmap? Icon {get;}
}

public interface IPluginSearchInfo : IPluginSpecification
{
    string Id => $"{Source.SourceUri}|{PackageId}";
    IPluginServerInfo Source { get; }
    IEnumerable<PackageDependency> Dependencies { get; }
    string LastVersion { get; }
    long? DownloadCount { get; }
}

public interface IPluginSpecification
{
    SemVersion ApiVersion { get; }
    string PackageId { get; }
    string? Title { get; }
    public string? Description { get; }
    string? Authors { get; }
    string? Tags { get; }
    bool IsVerified { get; }

}