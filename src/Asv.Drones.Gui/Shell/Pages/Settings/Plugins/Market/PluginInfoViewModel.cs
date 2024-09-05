using System;
using System.Collections.Generic;
using System.Reactive;
using System.Threading;
using System.Threading.Tasks;
using Asv.Common;
using Asv.Drones.Gui.Api;
using DynamicData.Binding;
using NuGet.Protocol.Core.Types;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Asv.Drones.Gui;

public class PluginInfoViewModel : DisposableReactiveObject
{
    private readonly IPluginSearchInfo _pluginInfo;
    private readonly IPluginManager _manager;
    private ILocalPluginInfo? _localInfo;

    public PluginInfoViewModel()
    {
        DesignTime.ThrowIfNotDesignMode();
        Install = CoreDesignTime.CancellableCommand<Unit, Unit>();
        Uninstall = CoreDesignTime.CancellableCommand<Unit, Unit>();
        CancelUninstall = CoreDesignTime.CancellableCommand<Unit, Unit>();
    }

    public PluginInfoViewModel(IPluginSearchInfo pluginInfo, IPluginManager manager, ILogService log)
    {
        _pluginInfo = pluginInfo;
        _manager = manager;
        Id = pluginInfo.Id;
        Install = new CancellableCommandWithProgress<Unit, Unit>(InstallImpl, "Install", log, RxApp.TaskpoolScheduler)
            .DisposeItWith(Disposable);
        Uninstall = new CancellableCommandWithProgress<Unit, Unit>(UninstallImpl, "Uninstall", log,
            RxApp.TaskpoolScheduler).DisposeItWith(Disposable);
        CancelUninstall = new CancellableCommandWithProgress<Unit, Unit>(CancelUninstallImpl, "CancelUniistal", log,
                RxApp.TaskpoolScheduler).DisposeItWith(Disposable);
        IsInstalled = _manager.IsInstalled(pluginInfo.PackageId, out _localInfo);
        if (_localInfo !=null)
        {
            IsUninstalled = _localInfo.IsUninstalled;
        }
        Name = pluginInfo.Title;
        Author = pluginInfo.Authors;
        Description = pluginInfo.Description;
        SourceName = pluginInfo.Source.Name;
        SourceUri = pluginInfo.Source.SourceUri;
        LastVersion = $"{pluginInfo.LastVersion} (API: {pluginInfo.ApiVersion})";
        IsApiCompatible = pluginInfo.ApiVersion == manager.ApiVersion;
        LocalVersion = (_localInfo != null) ? $"{_localInfo?.Version} (API: {_localInfo?.ApiVersion})" : null;
        if (Author != null) IsVerified = Author.Contains("https://github.com/asv-soft") && SourceUri.Contains("https://nuget.pkg.github.com/asv-soft/index.json");
        Version = pluginInfo.LastVersion;
    }

    public bool IsApiCompatible { get; set; }
    public string Id { get; set; }
    public string? Author { get; set; }
    public string? SourceUri { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string SourceName { get; set; }
    public string LastVersion { get; set; }
    public string Version { get; set; }
    public string? LocalVersion { get; set; }
    [Reactive]public bool IsInstalled { get; set; }
    [Reactive]public bool IsUninstalled { get; set; }
    [Reactive] public bool IsVerified { get; set; }
    

    private async Task<Unit> UninstallImpl(Unit arg, IProgress<double> progress, CancellationToken cancel)
    {
        if (_localInfo == null)
        {
            throw new Exception("Plugin not installed");
        }

        _manager.Uninstall(_localInfo);
        IsUninstalled = true;
        return Unit.Default;
    }
    private async Task<Unit> CancelUninstallImpl(Unit arg, IProgress<double> progress, CancellationToken cancel)
    {
        if (_localInfo == null)
        {
            throw new Exception("Plugin not installed");
        }

        _manager.CancelUninstall(_localInfo);
        IsUninstalled = false;
        return Unit.Default;
    }

    private async Task<Unit> InstallImpl(Unit arg, IProgress<double> progress, CancellationToken cancel)
    {
        await _manager.Install(_pluginInfo.Source, _pluginInfo.PackageId, _pluginInfo.LastVersion,
            new Progress<ProgressMessage>(
                m => { progress.Report(m.Progress); }), cancel);
        IsInstalled = _manager.IsInstalled(_pluginInfo.PackageId, out _localInfo);
        return Unit.Default;
    }

    public CancellableCommandWithProgress<Unit, Unit> Uninstall { get; }
    public CancellableCommandWithProgress<Unit, Unit> CancelUninstall { get; }
    public CancellableCommandWithProgress<Unit, Unit> Install { get; }
}