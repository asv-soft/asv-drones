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
    private readonly ILoggerFactory _loggerFactory;
    private const string Salt = "Asv.Drones.Gui";
    private readonly ReaderWriterLockSlim _repositoriesLock = new();
    private readonly List<SourceRepository> _repositories = new();

    public const string PluginSearchTermStartWith = "Asv.Drones.Gui.Plugin.";


    private readonly LoggerAdapter _nugetLogger;
    private const string PluginStateFileName = "__PLUGIN_STATE__";
    
    private readonly string _sharedPluginFolder;
    private readonly string _nugetFolder;
    private readonly SourceCacheContext _cache;
    private readonly List<AssemblyLoadContext> _pluginContexts = new();
    private readonly ILogger<PluginManager> _logger;


    public PluginManager(ContainerConfiguration containerCfg, string localDirectory, IConfiguration configuration, ILoggerFactory loggerFactory) :
        base(configuration)
    {
        _loggerFactory = loggerFactory;
        ApiVersion = SemVersion.Parse(typeof(WellKnownUri).Assembly.GetCustomAttribute<AssemblyFileVersionAttribute>()!.Version);
        
        _logger = loggerFactory.CreateLogger<PluginManager>();
        _nugetLogger = new LoggerAdapter(_logger);
        
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
            _logger.ZLogInformation($"Replace clear text password for server {server.Name}");
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
                _logger.ZLogWarning($"Error add plugin source server {server.Name}");
            }
        }

        #endregion

        #region Folder creation

        _sharedPluginFolder = Path.Combine(localDirectory, "plugins");
        if (Directory.Exists(_sharedPluginFolder) == false)
        {
            _logger.ZLogInformation($"Create plugin folder {_sharedPluginFolder}");
            Directory.CreateDirectory(_sharedPluginFolder);
        }
        else
        {
            _logger.ZLogInformation($"Found plugin folder {_sharedPluginFolder}");
        }

        _nugetFolder = Path.Combine(localDirectory, "nuget");
        if (Directory.Exists(_nugetFolder) == false)
        {
            _logger.ZLogInformation($"Create nuget folder {_nugetFolder}");
            Directory.CreateDirectory(_nugetFolder);
        }
        else
        {
            _logger.ZLogInformation($"Found nuget cache folder {_nugetFolder}");
        }

        var nugetCache = Path.Combine(localDirectory, "nuget_cache");
        if (Directory.Exists(nugetCache) == false)
        {
            _logger.ZLogInformation($"Create nuget folder {nugetCache}");
            Directory.CreateDirectory(nugetCache);
        }
        else
        {
            _logger.ZLogInformation($"Found nuget cache folder {nugetCache}");
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
                _logger.ZLogWarning($"Error read plugin info from {dir}. Delete it");
                Directory.Delete(dir, true);
                continue;
            }

            if (info == null) continue;
            if (info.IsUninstalled)
            {
                _logger.ZLogInformation($"Plugin {info.PackageId} is marked as uninstalled. Delete it");
                Directory.Delete(info.LocalFolder, true);
                continue;
            }

            // check API version
            if (info.ApiVersion.CompareByPrecedence(ApiVersion) != 0)
            {
                _logger.ZLogWarning($"Plugin {info.PackageId} {info.Version} has different API version {info.ApiVersion} than application {ApiVersion}");
                SetPluginStateByFolder(info.LocalFolder, x =>
                {
                    x.IsLoaded = false;
                    x.LoadingError = $"Plugin has different API version {info.ApiVersion} than application {ApiVersion}";
                });
                continue;
            }

            
            
            try
            {
                _logger.ZLogInformation($"Load plugin {info.PackageId} {info.Version} {info.LocalFolder}");
                _pluginContexts.Add(new PluginAssemblyLoadContext(info.LocalFolder, containerCfg, _loggerFactory));
                SetPluginStateByFolder(info.LocalFolder, x =>
                {
                    x.IsLoaded = true;
                    x.LoadingError = null;
                });
            }
            catch (Exception e)
            {
                _logger.ZLogError(e,$"Error load plugin {info.PackageId} {info.Version} {info.LocalFolder}");
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
                _logger.ZLogWarning($"Server source {server.SourceUri} already exists");
                return false;
            }

            SourceRepository repo;
            if (string.IsNullOrWhiteSpace(server.Username))
            {
                repo = Repository.Factory.GetCoreV3(new PackageSource(server.SourceUri, server.Name));
                _logger.ZLogInformation($"Add plugin server source {server.SourceUri}");
            }
            else
            {
                _logger.ZLogInformation($"Add plugin server source {server.SourceUri} with credentials");
                repo = Repository.Factory.GetCoreV3(new PackageSource(server.SourceUri, server.Name)
                {
                    Credentials =
                        new PackageSourceCredential(server.SourceUri, server.Username, server.Password, true, null),
                });
            }

            _repositories.Add(repo);
            if (saveToConfig)
            {
                InternalSaveConfig(c =>
                {
                    c.Servers = _repositories.Select(x => new PluginServerConfig
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
                _logger.ZLogWarning($"Server source {info.SourceUri} not found");
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
                        _logger.ZLogWarning($"Error create plugin search info from {package.Identity.Id} {e.Message}");
                        continue;
                    }
                    if (result.Count >= query.Take) break;
                }

                if (result.Count >= query.Take) break;
            }
            catch (Exception e)
            {
                _logger.ZLogError(e,$"Error search in {repository.PackageSource.Source}");
            }
        }

        return result;
    }

    public async Task<IReadOnlyList<string>> ListPluginVersions(SearchQuery query, string pluginId, CancellationToken cancel)
    {
        _repositoriesLock.EnterReadLock();
        var repositories = query.Sources.Count == 0
            ? _repositories.ToArray()
            : _repositories.Where(_ => query.Sources.Contains(_.PackageSource.Source)).ToArray();
        _repositoriesLock.ExitReadLock();

        var result = new List<string>();

        foreach (var repository in repositories)
        {
            try
            {
                var resource = await repository.GetResourceAsync<FindPackageByIdResource>(cancel);

                var packages = await resource.GetAllVersionsAsync(
                    pluginId,
                    new SourceCacheContext(),
                    new LoggerAdapter(Logger),
                    cancel);

                result.AddRange(packages.Select(package => package.Version.ToString()));
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
                    _logger.ZLogWarning($"Not found  {NugetHelper.NETCoreAppGroup} platform in package {identity.Id}");
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

    public async Task InstallManually(string from, IProgress<ProgressMessage>? progress, CancellationToken cancel)
    {
        ArgumentNullException.ThrowIfNull(from);
        if (!File.Exists(from))
        {
            throw new FileNotFoundException($"File {from} not found");
        }

        await using var nugetFile = File.OpenRead(from);
        var packageReader = new PackageArchiveReader(nugetFile);
        var packageIdentity = await packageReader.GetIdentityAsync(cancel);
        
        progress = progress ?? new Progress<ProgressMessage>();
        if (TryGetLocalPluginInfoById(packageIdentity.Id, out var info))
        {
            Debug.Assert(info != null, nameof(info) + " != null");
            var localVersion = NuGetVersion.Parse(info.Version);
            throw new Exception($"Local version {localVersion} of {packageIdentity.Id} exists. Remove it first.");
        }

        var currentPluginFolder = Path.Combine(_sharedPluginFolder, packageIdentity.Id);
        
        try
        {
            Directory.CreateDirectory(currentPluginFolder);
            
            var plugin = Path.Combine(currentPluginFolder, Path.GetFileName(from));
            if (!File.Exists(plugin))
            {
                File.Copy(from, plugin);
            }
            
            ExtractContentFolder(plugin,  currentPluginFolder);
            
            var platform = NugetHelper.GetPlatform(packageReader);
            if (platform == null)
            {
                throw new Exception($"Not found {NugetHelper.NETCoreAppGroup} platform in package");
            }

            foreach (var file in platform.Items)
            {
                packageReader.ExtractFile(file, Path.Combine(currentPluginFolder, Path.GetFileName(file)),
                    _nugetLogger);
            }

            var nuspecReader = packageReader.NuspecReader;
            // now we need to load all dependencies
            var dependencyInfoResource = nuspecReader.GetDependencyGroups();
            
            _repositoriesLock.EnterReadLock();
            var repositories = _repositories.ToArray();
            _repositoriesLock.ExitReadLock();
            
            var dependencies = await ListAllPackageDependencies(dependencyInfoResource, repositories, cancel);
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

            SetPluginStateById(packageIdentity.Id, x =>
            {
                x.IsLoaded = false;
                x.IsUninstalled = false;
                x.LoadingError = null;
                x.InstalledFromSourceUri = "N/A";
            });
        }
        catch (Exception)
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
    
    private static void ExtractContentFolder(string nugetPackage, string to)
    {
        if (!File.Exists(nugetPackage))
        {
            return;
        }
        
        if (!Directory.Exists(to))
        {
            return;
        }

        var archive = ZipFile.OpenRead(nugetPackage);

        foreach (var entry in archive.Entries)
        {
            if (!entry.FullName.Trim().StartsWith(@"content", StringComparison.InvariantCultureIgnoreCase))
            {
                continue;
            }
            
            var filePath = Path.Combine(to, entry.FullName);
            var fileDir = Path.GetDirectoryName(filePath);

            if (fileDir is null)
            {
                continue;
            }
            
            if (!Directory.Exists(fileDir))
            {
                Directory.CreateDirectory(fileDir);
            }

            if (!File.Exists(filePath))
            {
                entry.ExtractToFile(filePath, true);
            }
        }
    }
    
    private async Task<ISet<SourcePackageDependencyInfo>> ListAllPackageDependencies(
        IEnumerable<PackageDependencyGroup> dependencyGroups,
        SourceRepository[] repositories,
        CancellationToken cancellationToken)
    {
        var dependencies = new HashSet<SourcePackageDependencyInfo>(PackageIdentityComparer.Default);
        var packages = await ConvertToSourcePackageDependencyInfo(dependencyGroups, repositories, cancellationToken);
        
        foreach (var package in packages)
        {
            await ListAllPackageDependencies(package, repositories, dependencies, cancellationToken);
        }

        return dependencies;
    }
    
    private async Task<ISet<SourcePackageDependencyInfo>> ConvertToSourcePackageDependencyInfo(
        IEnumerable<PackageDependencyGroup> dependencyGroups,
        SourceRepository[] repositories,
        CancellationToken cancel
        )
    {
        var dependenciesSet = new HashSet<SourcePackageDependencyInfo>(PackageIdentityComparer.Default);
        var packageDependencyGroups = dependencyGroups.ToList();

        foreach (var repository in repositories)
        {
            foreach (var group in packageDependencyGroups)
            {
                foreach (var package in group.Packages)
                {
                    var version = package.VersionRange.MinVersion;
                    if (version is null)
                    {
                        continue;
                    }
                    var packageByIdSearcher = await repository.GetResourceAsync<FindPackageByIdResource>(cancel);
                    var isPackage = await packageByIdSearcher.DoesPackageExistAsync(package.Id, version,
                        _cache, _nugetLogger, cancel);

                    if (!isPackage)
                    {
                        continue;
                    }

                    var dependencyInfo = new SourcePackageDependencyInfo(package.Id, version,
                        [], true, repository);

                    if (dependenciesSet.Contains(dependencyInfo))
                    {
                        continue;
                    }
                    
                    if (NugetHelper.IncludedPackages.Contains(dependencyInfo.Id))
                    {
                        continue;
                    }
                    
                    dependenciesSet.Add(dependencyInfo);
                }
            }
        }

        return dependenciesSet;
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
            _logger.ZLogWarning($"Find more than one package in folder {pluginFolder}");
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
            _logger.ZLogError(e, $"Error read nuspec from {package[0]}");
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
            _logger.ZLogError(e, $"Error to write plugin {pluginFolder} state file");
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