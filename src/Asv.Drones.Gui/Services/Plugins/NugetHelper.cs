using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NuGet.Common;
using NuGet.Configuration;
using NuGet.Frameworks;
using NuGet.Packaging;
using NuGet.Packaging.Core;
using NuGet.Protocol.Core.Types;
using NuGet.Resolver;

namespace Asv.Drones.Gui;

public static class NugetHelper
{
    public const string NugetPluginName = "Asv.Drones.Gui.Plugin";

    public const string NETCoreAppGroup = ".NETCoreApp";

    public static NuGetFramework DefaultFramework = NuGetFramework.ParseFolder("net8.0");


    public static HashSet<string> IncludedPackages = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase)
    {
        "System.IO.Packaging",
        "Asv.Avalonia.Toolkit",
        "Asv.Drones.Gui.Api",
        "Avalonia.Themes.Fluent",
        "Asv.Avalonia.Map",
        "Asv.Cfg",
        "Asv.Common",
        "Asv.IO",
        "Asv.Mavlink",
        "Avalonia",
        "Avalonia.Angle.Windows.Natives",
        "Avalonia.BuildServices",
        "Avalonia.Controls.ColorPicker",
        "Avalonia.Controls.DataGrid",
        "Avalonia.Controls.ItemsRepeater",
        "Avalonia.Desktop",
        "Avalonia.Diagnostics",
        "Avalonia.Fonts.Inter",
        "Avalonia.FreeDesktop",
        "Avalonia.Native",
        "Avalonia.ReactiveUI",
        "Avalonia.Remote.Protocol",
        "Avalonia.Skia",
        "Avalonia.Themes.Simple",
        "Avalonia.Win32",
        "Avalonia.X11",
        "Avalonia.Xaml.Behaviors",
        "Avalonia.Xaml.Interactions",
        "Avalonia.Xaml.Interactions.Custom",
        "Avalonia.Xaml.Interactions.DragAndDrop",
        "Avalonia.Xaml.Interactions.Draggable",
        "Avalonia.Xaml.Interactions.Events",
        "Avalonia.Xaml.Interactions.Reactive",
        "Avalonia.Xaml.Interactions.Responsive",
        "Avalonia.Xaml.Interactivity",
        "DynamicData",
        "FluentAvaloniaUI",
        "Fody",
        "Geodesy",
        "HarfBuzzSharp",
        "HarfBuzzSharp.NativeAssets.Linux",
        "HarfBuzzSharp.NativeAssets.macOS",
        "HarfBuzzSharp.NativeAssets.WebAssembly",
        "HarfBuzzSharp.NativeAssets.Win32",
        "Material.Icons",
        "Material.Icons.Avalonia",
        "MicroCom.CodeGenerator.MSBuild",
        "MicroCom.Runtime",
        "Microsoft.CodeAnalysis.Analyzers",
        "Microsoft.CodeAnalysis.Common",
        "Microsoft.CodeAnalysis.CSharp",
        "Microsoft.CodeAnalysis.CSharp.Scripting",
        "Microsoft.CodeAnalysis.Scripting.Common",
        "Microsoft.CSharp",
        "Microsoft.NETCore.Platforms",
        "Microsoft.NETCore.Targets",
        "Microsoft.Win32.SystemEvents",
        "Newtonsoft.Json",
        "NLog",
        "NuGet.Common",
        "NuGet.Configuration",
        "NuGet.Frameworks",
        "NuGet.Packaging",
        "NuGet.Packaging.Core",
        "NuGet.Protocol",
        "NuGet.Versioning",
        "ReactiveUI",
        "ReactiveUI.Fody",
        "ReactiveUI.Validation",
        "runtime.linux-arm.runtime.native.System.IO.Ports",
        "runtime.linux-arm64.runtime.native.System.IO.Ports",
        "runtime.linux-x64.runtime.native.System.IO.Ports",
        "runtime.native.System.IO.Ports",
        "runtime.osx-arm64.runtime.native.System.IO.Ports",
        "runtime.osx-x64.runtime.native.System.IO.Ports",
        "SkiaSharp",
        "SkiaSharp.NativeAssets.Linux",
        "SkiaSharp.NativeAssets.macOS",
        "SkiaSharp.NativeAssets.WebAssembly",
        "SkiaSharp.NativeAssets.Win32",
        "Splat",
        "System.Collections",
        "System.Collections.Immutable",
        "System.ComponentModel.Annotations",
        "System.Composition",
        "System.Composition.AttributedModel",
        "System.Composition.Convention",
        "System.Composition.Hosting",
        "System.Composition.Runtime",
        "System.Composition.TypedParts",
        "System.Diagnostics.Debug",
        "System.Drawing.Common",
        "System.Dynamic.Runtime",
        "System.Formats.Asn1",
        "System.Globalization",
        "System.IO",
        "System.IO.Pipelines",
        "System.IO.Ports",
        "System.Linq",
        "System.Linq.Expressions",
        "System.Memory",
        "System.Numerics.Vectors",
        "System.ObjectModel",
        "System.Reactive",
        "System.Reflection",
        "System.Reflection.Emit",
        "System.Reflection.Emit.ILGeneration",
        "System.Reflection.Emit.Lightweight",
        "System.Reflection.Extensions",
        "System.Reflection.Metadata",
        "System.Reflection.Primitives",
        "System.Reflection.TypeExtensions",
        "System.Resources.ResourceManager",
        "System.Runtime",
        "System.Runtime.CompilerServices.Unsafe",
        "System.Runtime.Extensions",
        "System.Runtime.Handles",
        "System.Runtime.InteropServices",
        "System.Security.Cryptography.Pkcs",
        "System.Security.Cryptography.ProtectedData",
        "System.Text.Encoding",
        "System.Text.Encoding.CodePages",
        "System.Text.Encodings.Web",
        "System.Text.Json",
        "System.Threading",
        "System.Threading.Tasks",
        "System.Threading.Tasks.Extensions",
        "Tmds.DBus.Protocol"
    };

    public static async Task<IReadOnlyList<SourcePackageDependencyInfo>> ListAllDepenency(
        IEnumerable<SourceRepository> repositories, SourceRepository repository, SourceCacheContext cache,
        PackageIdentity packageId, NuGetFramework framework, ILogger logger, CancellationToken cancel)
    {
        var dependencyInfoResource = await repository.GetResourceAsync<DependencyInfoResource>(cancel);
        var dependencyInfo = await dependencyInfoResource.ResolvePackage(packageId, framework, cache, logger, cancel);
        var packages = new HashSet<SourcePackageDependencyInfo>(PackageIdentityComparer.Default);
        await ListAllPackageDependencies(dependencyInfo, repositories, DefaultFramework, cache, logger, packages,
            cancel);
        var targets = new PackageIdentity[] { packageId };
        var resolver = new PackageResolver();
        var context = new PackageResolverContext(DependencyBehavior.Lowest,
            targets.Select(p => p.Id), Enumerable.Empty<string>(),
            Enumerable.Empty<PackageReference>(),
            targets,
            packages,
            Enumerable.Empty<PackageSource>(),
            logger);
        var result = new HashSet<PackageIdentity>(resolver.Resolve(context, cancel), PackageIdentityComparer.Default);
        return packages.Where(package => result.Contains(package)).ToList();
    }

    private static async Task ListAllPackageDependencies(
        SourcePackageDependencyInfo package,
        IEnumerable<SourceRepository> repositories,
        NuGetFramework framework,
        SourceCacheContext cache,
        ILogger logger,
        ISet<SourcePackageDependencyInfo> dependencies,
        CancellationToken cancellationToken)
    {
        if (dependencies.Contains(package))
            return;

        foreach (var repository in repositories)
        {
            var dependencyInfoResource = await repository.GetResourceAsync<DependencyInfoResource>(cancellationToken);
            var dependencyInfo =
                await dependencyInfoResource.ResolvePackage(package, framework, cache, logger, cancellationToken);

            if (dependencyInfo == null)
                continue;

            if (dependencies.Add(dependencyInfo))
            {
                foreach (var dependency in dependencyInfo.Dependencies)
                {
                    await ListAllPackageDependencies(
                        new SourcePackageDependencyInfo(dependency.Id, dependency.VersionRange.MinVersion,
                            Enumerable.Empty<PackageDependency>(), true, repository),
                        repositories,
                        framework,
                        cache,
                        logger,
                        dependencies,
                        cancellationToken);
                }
            }
        }
    }

    public static async Task<string> DownloadPackage(PackageIdentity identity, SourceRepository repository,
        SourceCacheContext cache, string nugetPluginFolder, ILogger logger, CancellationToken cancel)
    {
        var findPackageByIdResource = await repository.GetResourceAsync<FindPackageByIdResource>(cancel);
        var packageFolder = Path.Combine(nugetPluginFolder, $"{identity.Id}.{identity.Version}");
        if (Directory.Exists(packageFolder)) return packageFolder;

        var packageFile = Path.Combine(nugetPluginFolder, $"{identity.Id}.{identity.Version}.nupkg");

        if (File.Exists(packageFile) == false)
        {
            await using var file = File.OpenWrite(packageFile);
            await findPackageByIdResource.CopyNupkgToStreamAsync(identity.Id, identity.Version, file, cache, logger,
                cancel);
        }

        ZipFile.ExtractToDirectory(File.OpenRead(packageFile), packageFolder);

        return packageFolder;
    }

    public static void CopyNugetFiles(string pluginFolder, string nugetFolder)
    {
        PackageArchiveReader packageArchiveReader = new(nugetFolder);

        using var packageFolderReader = new PackageFolderReader(nugetFolder);
        var libs = packageFolderReader.GetLibItems();
        var platform = libs.Where(_ => _.TargetFramework.Platform == ".NETCoreApp")
            .OrderByDescending(_ => _.TargetFramework.Version).FirstOrDefault();
        if (platform == null) return;
        foreach (var file in platform.Items)
        {
            File.Copy(Path.Combine(nugetFolder, file), Path.Combine(pluginFolder, Path.GetFileName(file)), true);
        }
    }

    public static FrameworkSpecificGroup? GetPlatform(PackageReaderBase dependencyPackageArchiveReader)
    {
        return dependencyPackageArchiveReader.GetLibItems()
            .Where(_ => _.TargetFramework.Framework == DefaultFramework.Framework)
            .OrderByDescending(_ => _.TargetFramework.Version).FirstOrDefault();
    }
}