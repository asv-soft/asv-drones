using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using Asv.Avalonia;
using Asv.Avalonia.Plugins;
using Asv.Common;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using NuGet.Packaging;

namespace Asv.Drones.Desktop;

internal static class SafePluginsMixin
{
    public static IHostApplicationBuilder UseSafeModulePlugins(
        this IHostApplicationBuilder builder,
        Action<PluginsMixin.Builder>? configure = null
    )
    {
        var pluginOptions =
            builder
                .Configuration.GetSection(PluginBootloaderOptions.SectionName)
                .Get<PluginBootloaderOptions>()
            ?? new PluginBootloaderOptions();

        configure ??= b => b.UseDefault();
        configure(new PluginsMixin.Builder(builder, pluginOptions));

        var loader = new SafePluginBootloader(Options.Create(pluginOptions), builder.Environment);
        builder.Services.AddSingleton<IPluginBootloader>(loader);
        builder.AddPostConfigureCallbacks(builder => loader.InitPlugins(builder));
        return builder;
    }
}

internal sealed class SafePluginBootloader : IPluginBootloader
{
    private const string PackageFilePostfix = ".nupkg";

    private readonly string _apiPackageId;
    private readonly SemVersion _apiVersion;
    private readonly string _assemblyPluginPrefix;
    private readonly List<PluginAssemblyLoadContext> _pluginContexts = [];
    private readonly List<Assembly> _pluginAssemblies = [];
    private readonly List<ILocalPluginInfo> _info = [];

    public SafePluginBootloader(
        IOptions<PluginBootloaderOptions> options,
        IHostEnvironment environment
    )
    {
        _apiPackageId =
            options.Value.ApiPackageName
            ?? throw new InvalidOperationException(
                $"ApiPackageName is required in {PluginBootloaderOptions.SectionName} configuration section"
            );
        _apiVersion = SemVersion.Parse(
            options.Value.ApiVersion
                ?? throw new InvalidOperationException(
                    $"ApiVersion is required in {PluginBootloaderOptions.SectionName} configuration section"
                )
        );
        _assemblyPluginPrefix =
            options.Value.PluginAssemblyPrefix
            ?? throw new InvalidOperationException(
                $"PluginAssemblyPrefix is required in {PluginBootloaderOptions.SectionName} configuration section"
            );

        var relativeFolder = Path.GetFullPath(
            Path.Combine(environment.ContentRootPath, options.Value.RelativeFolder)
        );
        if (Directory.Exists(relativeFolder))
        {
            foreach (var dir in Directory.EnumerateDirectories(relativeFolder))
            {
                ProcessPluginFolder(dir);
            }
        }

        if (options.Value.AdditionalFolderPerPlugin is null)
        {
            return;
        }

        foreach (var absolutePath in options.Value.AdditionalFolderPerPlugin.Distinct())
        {
            var fullPath = Path.GetFullPath(absolutePath);
            if (Directory.Exists(fullPath))
            {
                ProcessPluginFolder(fullPath);
            }
        }
    }

    public SemVersion ApiVersion => _apiVersion;
    public IEnumerable<ILocalPluginInfo> Installed => _info;

    public void InitPlugins(IHostApplicationBuilder builder)
    {
        foreach (var assembly in _pluginAssemblies)
        {
            try
            {
                foreach (
                    var pluginAppBuilder in assembly
                        .GetTypes()
                        .Where(t =>
                            typeof(IPluginAppBuilder).IsAssignableFrom(t)
                            && t is { IsClass: true, IsAbstract: false }
                        )
                        .Select(t => Activator.CreateInstance(t) as IPluginAppBuilder)
                        .Where(p => p is not null)
                )
                {
                    pluginAppBuilder?.Register(builder);
                }
            }
            catch (Exception e)
            {
                WritePluginCrashReport(
                    Path.GetDirectoryName(assembly.Location) ?? AppContext.BaseDirectory,
                    e
                );
            }
        }
    }

    private void ProcessPluginFolder(string folder)
    {
        try
        {
            var info = GetInfo(folder);
            if (info is null)
            {
                return;
            }

            if (info.IsUninstalled)
            {
                Directory.Delete(folder, true);
                return;
            }

            _info.Add(info);
            if (info.ApiVersion.CompareByPrecedence(_apiVersion) != 0)
            {
                PluginState.Edit(
                    folder,
                    x =>
                    {
                        x.IsLoaded = false;
                        x.LoadingError =
                            $"Plugin has different API version {info.ApiVersion} than application {_apiVersion}";
                    }
                );
                return;
            }

            var context = PluginAssemblyLoadContext.Create(
                folder,
                _assemblyPluginPrefix,
                _pluginAssemblies
            );
            _pluginContexts.Add(context);
        }
        catch (Exception e)
        {
            WritePluginCrashReport(folder, e);
        }
    }

    private ILocalPluginInfo? GetInfo(string folder)
    {
        var package = Directory
            .EnumerateFiles(folder, $"*{PackageFilePostfix}", SearchOption.TopDirectoryOnly)
            .ToImmutableArray();
        if (package.Length == 0)
        {
            return null;
        }

        if (package.Length > 1)
        {
            throw new InvalidOperationException(
                $"Find more than one package in folder {folder}: {string.Join(",", package)}"
            );
        }

        var state = PluginState.Read(folder);
        if (state is null)
        {
            state = PluginState.Write(
                folder,
                new PluginState
                {
                    IsLoaded = false,
                    LoadingError = null,
                    IsUninstalled = false,
                    InstalledFromSourceUri = new Uri(folder).ToString(),
                }
            );
        }

        Debug.Assert(state != null, nameof(state) + " != null");
        using var reader = new PackageArchiveReader(package[0]);
        return new LocalPluginInfo(reader, folder, state, _apiPackageId);
    }

    private static void WritePluginCrashReport(string path, Exception e)
    {
        var dir = Directory.Exists(path)
            ? path
            : Path.GetDirectoryName(path) ?? AppContext.BaseDirectory;
        Directory.CreateDirectory(dir);
        ExceptionReport.WriteToFile(dir, e, out _);
    }
}
