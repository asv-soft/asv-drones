using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition.Hosting;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Threading;
using System.Threading.Tasks;
using Asv.Cfg;
using Asv.Common;
using Asv.Drones.Gui.Api;
using DynamicData;
using Newtonsoft.Json;
using NLog;
using NuGet.Configuration;
using NuGet.Packaging;
using NuGet.Packaging.Core;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;
using NuGet.Versioning;


namespace Asv.Drones.Gui;

public class PluginState
{
    public bool IsUninstalled { get; set; }
    public bool IsLoaded { get; set; }
    public string? LoadingError { get; set; }
    public string? InstalledFromSourceUri { get; set; }

    public void CopyFrom(PluginState state)
    {
        IsLoaded = state.IsLoaded;
        LoadingError = state.LoadingError;
        IsUninstalled = state.IsUninstalled;
        InstalledFromSourceUri = state.InstalledFromSourceUri;
    }
}

public class PluginManagerConfig
{
    public PluginServerConfig[]? Servers { get; set; }
}

public class PluginServerConfig
{
    public string Name { get; set; }
    public string SourceUri { get; set; }
    public string? Username { get; set; }
    public string? Password { get; set; }
    public string? PasswordHash { get; set; }
}

public class PluginManager : ServiceWithConfigBase<PluginManagerConfig>, IPluginManager
{
    private const string Salt = "Asv.Drones.Gui";
    private readonly ReaderWriterLockSlim _repositoriesLock = new();
    private readonly List<SourceRepository> _repositories = new();

    public const string PluginSearchTermStartWith = "Asv.Drones.Gui.Plugin.";
    


    private const string PluginStateFileName = "__PLUGIN_STATE__";
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
    private readonly string _sharedPluginFolder;
    private readonly string _nugetFolder;
    private readonly LoggerAdapter _nugetLogger = new(Logger);
    private readonly SourceCacheContext _cache;
    private readonly List<AssemblyLoadContext> _pluginContexts = new();


    public PluginManager(ContainerConfiguration containerCfg, string localDirectory, IConfiguration configuration) :
        base(configuration)
    {
        ApiVersion = SemVersion.Parse(typeof(WellKnownUri).Assembly.GetCustomAttribute<AssemblyFileVersionAttribute>()!.Version);
        
        #region Servers

        var servers = InternalGetConfig(x => x.Servers);
        var needToSave = false;
        if (servers == null)
        {
            // create default
            servers =
            [
                new PluginServerConfig
                {
                    Name = "Nuget",
                    SourceUri = "https://api.nuget.org/v3/index.json",
                }
            ];
            needToSave = true;
        }

        // try find clear text passwords and replace it
        foreach (var server in servers)
        {
            if (string.IsNullOrWhiteSpace(server.Password) && string.IsNullOrWhiteSpace(server.PasswordHash)) continue;
            if (string.IsNullOrWhiteSpace(server.Password)) continue;
            Logger.Info("Replace clear text password for server {0}", server.Name);
            server.PasswordHash = server.Password.EncryptAES(Salt);
            server.Password = null;
            needToSave = true;
        }

        if (needToSave)
        {
            InternalSaveConfig(x => x.Servers = servers);
        }

        foreach (var server in servers)
        {
            if (InternalAddServer(
                    new PluginServer(server.Name, server.SourceUri, server.Username,
                        server.PasswordHash?.DecryptAES(Salt)), false, false) == false)
            {
                Logger.Warn("Error add plugin source server {0}", server.Name);
            }
        }

        #endregion

        #region Folder creation

        _sharedPluginFolder = Path.Combine(localDirectory, "plugins");
        if (Directory.Exists(_sharedPluginFolder) == false)
        {
            Logger.Info("Create plugin folder {0}", _sharedPluginFolder);
            Directory.CreateDirectory(_sharedPluginFolder);
        }
        else
        {
            Logger.Info("Found plugin folder {0}", _sharedPluginFolder);
        }

        _nugetFolder = Path.Combine(localDirectory, "nuget");
        if (Directory.Exists(_nugetFolder) == false)
        {
            Logger.Info("Create nuget folder {0}", _nugetFolder);
            Directory.CreateDirectory(_nugetFolder);
        }
        else
        {
            Logger.Info("Found nuget cache folder {0}", _nugetFolder);
        }

        var nugetCache = Path.Combine(localDirectory, "nuget_cache");
        if (Directory.Exists(nugetCache) == false)
        {
            Logger.Info("Create nuget folder {0}", nugetCache);
            Directory.CreateDirectory(nugetCache);
        }
        else
        {
            Logger.Info("Found nuget cache folder {0}", nugetCache);
        }

        #endregion

        _cache = new SourceCacheContext
        {
            /*GeneratedTempFolder = _nugetCache,
            SessionId = Guid.Empty,
            DirectDownload = true,
            NoCache = true,
            MaxAge = DateTimeOffset.MaxValue*/
        };

        // load all plugins
        foreach (var dir in Directory.EnumerateDirectories(_sharedPluginFolder, "*", SearchOption.TopDirectoryOnly))
        {
            if (TryGetLocalPluginInfoByFolder(dir, out var info) == false)
            {
                Logger.Warn("Error read plugin info from {0}. Delete it", dir);
                Directory.Delete(dir, true);
                continue;
            }

            if (info == null) continue;
            if (info.IsUninstalled)
            {
                Logger.Info("Remove deleted plugin {0} {1} {2}", info.PackageId, info.Version, info.LocalFolder);
                Directory.Delete(info.LocalFolder, true);
                continue;
            }

            // check API version
            if (info.ApiVersion.CompareByPrecedence(ApiVersion) != 0)
            {
                Logger.Warn("Plugin {0} {1} has different API version {2} than application {3}", info.Id, info.Version,
                    info.ApiVersion, ApiVersion);
                SetPluginStateByFolder(info.LocalFolder, x =>
                {
                    x.IsLoaded = false;
                    x.LoadingError = $"Plugin has different API version {info.ApiVersion} than application {ApiVersion}";
                });
                continue;
            }

            
            
            try
            {
                
                Logger.Info("Load plugin {0} {1} {2}", info.PackageId, info.Version, info.LocalFolder);
                _pluginContexts.Add(new PluginAssemblyLoadContext(info.LocalFolder, containerCfg));
                SetPluginStateByFolder(info.LocalFolder, x =>
                {
                    x.IsLoaded = true;
                    x.LoadingError = null;
                });
            }
            catch (Exception e)
            {
                Logger.Error(e, "Error load plugin {0} {1} {2}", info.PackageId, info.Version, info.LocalFolder);
                SetPluginStateByFolder(info.LocalFolder, x =>
                {
                    x.IsLoaded = false;
                    x.LoadingError = e.Message;
                });
            }
        }
    }

    public SemVersion ApiVersion { get; }

    #region Servers

    public IReadOnlyList<IPluginServerInfo> Servers
    {
        get
        {
            try
            {
                _repositoriesLock.EnterReadLock();
                return _repositories.Select(x => new SourceInfo(x)).ToImmutableList();
            }
            finally
            {
                _repositoriesLock.ExitReadLock();
            }
        }
    }

    public void AddServer(PluginServer server)
    {
        InternalAddServer(server, true, true);
    }

    private bool InternalAddServer(PluginServer server, bool throwExceptions, bool saveToConfig)
    {
        try
        {
            _repositoriesLock.EnterWriteLock();

            if (string.IsNullOrWhiteSpace(server.Name))
            {
                ArgumentNullException.ThrowIfNull(server.Name, nameof(server.Name));
            }

            if (string.IsNullOrWhiteSpace(server.SourceUri))
            {
                ArgumentNullException.ThrowIfNull(server.SourceUri, nameof(server.SourceUri));
            }

            if (_repositories.Any(_ => _.PackageSource.Source == server.SourceUri))
            {
                Logger.Warn("Server source {0} already exists", server.SourceUri);
                return false;
            }

            SourceRepository repo;
            if (string.IsNullOrWhiteSpace(server.Username))
            {
                repo = Repository.Factory.GetCoreV3(new PackageSource(server.SourceUri, server.Name));
                Logger.Info("Add plugin server source {0}", server.SourceUri);
            }
            else
            {
                Logger.Info("Add plugin server source {0} with credentials", server.SourceUri);
                repo = Repository.Factory.GetCoreV3(new PackageSource(server.SourceUri, server.Name)
                {
                    Credentials =
                        new PackageSourceCredential(server.SourceUri, server.Username, server.Password, true, null),
                });
            }

            _repositories.Add(repo);
            if (saveToConfig)
            {
                InternalSaveConfig(_ =>
                {
                    _.Servers = _repositories.Select(x => new PluginServerConfig
                    {
                        Name = x.PackageSource.Name,
                        SourceUri = x.PackageSource.Source,
                        PasswordHash = x.PackageSource.Credentials?.PasswordText.EncryptAES(Salt),
                        Username = x.PackageSource.Credentials?.Username
                    }).ToArray();
                });
            }

            return true;
        }
        catch
        {
            if (throwExceptions) throw;
        }
        finally
        {
            _repositoriesLock.ExitWriteLock();
        }

        return false;
    }

    public void RemoveServer(IPluginServerInfo info)
    {
        try
        {
            _repositoriesLock.EnterWriteLock();
            var repository = _repositories.FirstOrDefault(_ => _.PackageSource.Source == info.SourceUri);
            if (repository == null)
            {
                Logger.Warn("Server source {0} not found", info.SourceUri);
                return;
            }

            _repositories.Remove(repository);
            InternalSaveConfig(_ =>
            {
                _.Servers = _repositories.Select(x => new PluginServerConfig
                {
                    Name = x.PackageSource.Name,
                    SourceUri = x.PackageSource.Source,
                    PasswordHash = x.PackageSource.Credentials?.PasswordText.EncryptAES(Salt),
                    Username = x.PackageSource.Credentials?.Username
                }).ToArray();
            });
        }
        finally
        {
            _repositoriesLock.ExitWriteLock();
        }
    }

    #endregion

    #region Plugin management

    public async Task<IReadOnlyList<IPluginSearchInfo>> Search(SearchQuery query, CancellationToken cancel)
    {
        ArgumentNullException.ThrowIfNull(query);
        _repositoriesLock.EnterReadLock();
        var repositories = query.Sources.Count == 0
            ? _repositories.ToArray()
            : _repositories.Where(_ => query.Sources.Contains(_.PackageSource.Source)).ToArray();
        _repositoriesLock.ExitReadLock();

        var result = new List<IPluginSearchInfo>();

        foreach (var repository in repositories)
        {
            try
            {
                var resource = await repository.GetResourceAsync<PackageSearchResource>(cancel);
                var filter = new SearchFilter(query.IncludePrerelease);
                var searchTerm = query.Name == null
                    ? PluginSearchTermStartWith
                    : PluginSearchTermStartWith + query.Name;
                var packages =
                    await resource.SearchAsync(searchTerm, filter, query.Skip, query.Take, _nugetLogger, cancel);
                
                foreach (var package in packages)
                {
                    try
                    {
                        var dependencyInfoResource = await repository.GetResourceAsync<DependencyInfoResource>(cancel);
                        var dependencyInfo = await dependencyInfoResource.ResolvePackage(package.Identity, NugetHelper.DefaultFramework,
                            _cache, _nugetLogger, cancel);
                        if (dependencyInfo == null)
                            continue;
                        result.Add(new PluginSearchInfo(package, repository,dependencyInfo));
                    }
                    catch (Exception e)
                    {
                        Logger.Warn("Error create plugin search info from {0} {1}", package.Identity.Id, e.Message);
                        continue;
                    }
                    if (result.Count >= query.Take) break;
                }

                if (result.Count >= query.Take) break;
            }
            catch (Exception e)
            {
                Logger.Error(e, "Error search in {0}", repository.PackageSource.Source);
            }
        }

        return result;
    }

    public async Task Install(IPluginServerInfo source, string packageId, string version,
        IProgress<ProgressMessage>? progress, CancellationToken cancel)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(packageId);
        ArgumentNullException.ThrowIfNull(version);
        progress = progress ?? new Progress<ProgressMessage>();
        var downloadVersion = NuGetVersion.Parse(version);
        if (TryGetLocalPluginInfoById(packageId, out var info) == true)
        {
            Debug.Assert(info != null, nameof(info) + " != null");
            var localVersion = NuGetVersion.Parse(info.Version);
            throw new Exception($"Local version {localVersion} of {packageId} is exists. Remove it first.");
        }

        var currentPluginFolder = Path.Combine(_sharedPluginFolder, packageId);

        try
        {
            Directory.CreateDirectory(currentPluginFolder);
            var repository = _repositories.FirstOrDefault(_ => _.PackageSource.Source == source.SourceUri);
            if (repository == null)
            {
                throw new Exception($"Source {source.SourceUri} not found");
            }

            var packageIdentity = new PackageIdentity(packageId, downloadVersion);

            var packageFile =
                Path.Combine(currentPluginFolder, $"{packageIdentity.Id}.{packageIdentity.Version}.nupkg");
            var findPackageByIdResource = await repository.GetResourceAsync<FindPackageByIdResource>(cancel);
            await using (var file = File.OpenWrite(packageFile))
            {
                await findPackageByIdResource.CopyNupkgToStreamAsync(packageIdentity.Id, packageIdentity.Version, file,
                    _cache, _nugetLogger, cancel);
                file.Flush(true);
            }

            using var packageArchiveReader = new PackageArchiveReader(packageFile);
            var platform = NugetHelper.GetPlatform(packageArchiveReader);
            if (platform == null)
            {
                throw new Exception($"Not found {NugetHelper.NETCoreAppGroup} platform in package");
            }

            foreach (var file in platform.Items)
            {
                packageArchiveReader.ExtractFile(file, Path.Combine(currentPluginFolder, Path.GetFileName(file)),
                    _nugetLogger);
            }

            // now we need to load all dependencies
            var dependencyInfoResource = await repository.GetResourceAsync<DependencyInfoResource>(cancel);
            var dependencyInfo = await dependencyInfoResource.ResolvePackage(packageIdentity,
                NugetHelper.DefaultFramework, _cache, _nugetLogger, cancel);

            _repositoriesLock.EnterReadLock();
            var repositories = _repositories.ToArray();
            _repositoriesLock.ExitReadLock();

            var dependencies = new HashSet<SourcePackageDependencyInfo>(PackageIdentityComparer.Default);

            await ListAllPackageDependencies(dependencyInfo, repositories, dependencies, cancel);
            foreach (var identity in dependencies)
            {
                // if base application contains this package we don't need to download it
                // TODO: There is a potential problem with the version of NuGet packages in the base application
                if (NugetHelper.IncludedPackages.Contains(identity.Id)) continue;
                // if self package we don't need to download it
                if (identity.Equals(packageIdentity)) continue;

                var dependencyPackageFile = Path.Combine(_nugetFolder, $"{identity.Id}.{identity.Version}.nupkg");
                // if we already have this package we don't need to download it
                if (File.Exists(dependencyPackageFile) == false)
                {
                    var dependencyFindPackageByIdResource =
                        await identity.Source.GetResourceAsync<FindPackageByIdResource>(cancel);
                    await using var file = File.OpenWrite(dependencyPackageFile);
                    await dependencyFindPackageByIdResource.CopyNupkgToStreamAsync(identity.Id, identity.Version, file,
                        _cache, _nugetLogger, cancel);
                }

                using var dependencyPackageArchiveReader = new PackageArchiveReader(dependencyPackageFile);
                var dependencyPlatform = NugetHelper.GetPlatform(dependencyPackageArchiveReader);
                if (dependencyPlatform == null)
                {
                    Logger.Warn($"Not found  {NugetHelper.NETCoreAppGroup} platform in package " + identity.Id);
                    continue;
                }

                foreach (var file in dependencyPlatform.Items)
                {
                    dependencyPackageArchiveReader.ExtractFile(file,
                        Path.Combine(currentPluginFolder, Path.GetFileName(file)), _nugetLogger);
                }
            }

            SetPluginStateById(packageId, x =>
            {
                x.IsLoaded = false;
                x.IsUninstalled = false;
                x.LoadingError = null;
                x.InstalledFromSourceUri = source.SourceUri;
            });
        }
        catch (Exception e)
        {
            if (Directory.Exists(currentPluginFolder))
            {
                try
                {
                    Directory.Delete(currentPluginFolder, true);
                }
                catch
                {
                    // ignore
                }
            }
            throw;
        }
    }

    private async Task ListAllPackageDependencies(
        SourcePackageDependencyInfo package,
        SourceRepository[] repositories,
        ISet<SourcePackageDependencyInfo> dependencies,
        CancellationToken cancellationToken)
    {
        if (dependencies.Contains(package))
            return;

        foreach (var repository in repositories)
        {
            var dependencyInfoResource = await repository.GetResourceAsync<DependencyInfoResource>(cancellationToken);
            var dependencyInfo = await dependencyInfoResource.ResolvePackage(package, NugetHelper.DefaultFramework,
                _cache, _nugetLogger, cancellationToken);
            if (dependencyInfo == null)
                continue;

            if (NugetHelper.IncludedPackages.Contains(dependencyInfo.Id))
                continue;

            if (!dependencies.Add(dependencyInfo)) continue;
            foreach (var dependency in dependencyInfo.Dependencies)
            {
                await ListAllPackageDependencies(
                    new SourcePackageDependencyInfo(dependency.Id, dependency.VersionRange.MinVersion,
                        Enumerable.Empty<PackageDependency>(), true, repository),
                    repositories,
                    dependencies,
                    cancellationToken);
            }
        }
    }

    public void Uninstall(ILocalPluginInfo plugin)
    {
        // we cannot remove plugin folder because it used by application
        // so we set state PluginStateEnum.MarkedAsUninstalled
        // and application will remove folder on next start
        SetPluginStateById(plugin.PackageId, x => x.IsUninstalled = true);
    }

    public void CancelUninstall(ILocalPluginInfo pluginInfo)
    {
        SetPluginStateById(pluginInfo.PackageId, x => x.IsUninstalled = false);
    }

    public IEnumerable<ILocalPluginInfo> Installed
    {
        get
        {
            foreach (var folder in Directory.EnumerateDirectories(_sharedPluginFolder))
            {
                if (!TryGetLocalPluginInfoByFolder(folder, out var info)) continue;
                if (info != null) yield return info;
            }
        }
    }

    public bool IsInstalled(string packageId, out ILocalPluginInfo? info)
    {
        return TryGetLocalPluginInfoById(packageId, out info);
    }

    private bool TryGetLocalPluginInfoByFolder(string pluginFolder, out ILocalPluginInfo? info)
    {
        info = null;
        var package = Directory.EnumerateFiles(pluginFolder, "*.nupkg", SearchOption.TopDirectoryOnly)
            .ToImmutableArray();
        if (package.Length == 0)
        {
            return false;
        }

        if (package.Length > 1)
        {
            Logger.Warn("Find more than one package in folder {0}", pluginFolder);
        }

        if (TryGetPluginStateByFolder(pluginFolder, out var state) == false)
        {
            state = new PluginState
            {
                IsLoaded = false,
                LoadingError = null,
                IsUninstalled = false,
                InstalledFromSourceUri = new Uri(pluginFolder).ToString()
            };
            SetPluginStateByFolder(pluginFolder, x => x.CopyFrom(state));
        }

        Debug.Assert(state != null, nameof(state) + " != null");
        using var reader = new PackageArchiveReader(package[0]);
        try
        {
            info = new LocalPluginInfo(reader, pluginFolder, state);
        }
        catch (Exception e)
        {
            Logger.Error(e, "Error read nuspec from {0}", package[0]);
            return false;
        }

        return true;
    }

    private bool TryGetLocalPluginInfoById(string packageId, out ILocalPluginInfo? info)
    {
        info = null;
        ArgumentNullException.ThrowIfNull(packageId);
        var pluginFolder = Path.Combine(_sharedPluginFolder, packageId);
        return Directory.Exists(pluginFolder) && TryGetLocalPluginInfoByFolder(pluginFolder, out info);
    }


    private bool SetPluginStateById(string packageId, Action<PluginState> edit)
    {
        ArgumentNullException.ThrowIfNull(packageId);
        var pluginFolder = Path.Combine(_sharedPluginFolder, packageId);
        if (Directory.Exists(pluginFolder) == false) return false;
        return SetPluginStateByFolder(pluginFolder, edit);
    }

    private bool SetPluginStateByFolder(string pluginFolder, Action<PluginState> edit)
    {
        if (TryGetPluginStateByFolder(pluginFolder, out var state) == false)
        {
            state = new PluginState();
        }

        if (state == null) return false;
        try
        {
            var stateFilePath = Path.Combine(pluginFolder, PluginStateFileName);
            if (File.Exists(stateFilePath))
            {
                File.Delete(stateFilePath);
            }

            edit(state);
            File.WriteAllText(stateFilePath, JsonConvert.SerializeObject(state));
            return true;
        }
        catch (Exception e)
        {
            Logger.Error(e, $"Error to write plugin {pluginFolder} state file");
            return false;
        }
    }

    private bool TryGetPluginStateById(string packageId, out PluginState? state)
    {
        state = null;
        ArgumentNullException.ThrowIfNull(packageId);
        var pluginFolder = Path.Combine(_sharedPluginFolder, packageId);
        return Directory.Exists(pluginFolder) && TryGetPluginStateByFolder(pluginFolder, out state);
    }

    private bool TryGetPluginStateByFolder(string pluginFolder, out PluginState? state)
    {
        state = null;
        var stateFilePath = Path.Combine(pluginFolder, PluginStateFileName);
        if (File.Exists(stateFilePath) == false) return false;
        try
        {
            state = JsonConvert.DeserializeObject<PluginState>(File.ReadAllText(stateFilePath));
            return true;
        }
        catch (Exception e)
        {
            return false;
        }
    }

    #endregion
}