using System;
using System.Linq;
using Asv.Common;
using Asv.Drones.Gui.Api;
using NuGet.Packaging;

namespace Asv.Drones.Gui;

public class LocalPluginInfo : ILocalPluginInfo
{
    public LocalPluginInfo(PackageArchiveReader reader, string pluginFolder, PluginState state)
    {
        ArgumentNullException.ThrowIfNull(reader);
        var nuspec = reader.NuspecReader;
        Id = nuspec.GetId();
        Title = nuspec.GetTitle();
        Version = nuspec.GetVersion().ToFullString();
        LocalFolder = pluginFolder;
        PackageId = nuspec.GetId();
        Description = nuspec.GetDescription();
        Authors = nuspec.GetAuthors();
        Tags = nuspec.GetTags();
        SourceUri = state.InstalledFromSourceUri;
        IsUninstalled = state.IsUninstalled;
        IsLoaded = state.IsLoaded;
        LoadingError = state.LoadingError;
        var apiPackage = reader.GetPackageDependencies()
            .SelectMany(x => x.Packages).FirstOrDefault(x => x.Id == NugetHelper.PluginApiPackageName);
        if (apiPackage == null)
        {
            throw new Exception($"Plugin {Id} does not contain API package as dependency");
        }
        ApiVersion = apiPackage.VersionRange.MinVersion?.ToNormalizedString() ?? throw new InvalidOperationException("Api version not found in plugin dependencies");
    }

    public string? SourceUri { get; }
    public SemVersion ApiVersion { get; }
    public string PackageId { get; }
    public string LocalFolder { get; }
    public string Title { get; }
    public string? Description { get; }
    public string? Authors { get; }
    public string? Tags { get; }
    public string Id { get; }
    public string Version { get; }
    public bool IsUninstalled { get; }
    public bool IsLoaded { get; }
    public string? LoadingError { get; }
}