using System;
using System.Reactive;
using Asv.Common;
using Asv.Drones.Gui.Api;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Asv.Drones.Gui;

public class InstalledPluginInfoViewModel : DisposableReactiveObject
{
    private readonly IPluginManager _manager;
    private readonly ILocalPluginInfo _pluginInfo;

    public InstalledPluginInfoViewModel()
    {
        DesignTime.ThrowIfNotDesignMode();
        Uninstall = ReactiveCommand.Create(() => { });
        CancelUninstall = ReactiveCommand.Create(() => { });
    }

    public InstalledPluginInfoViewModel(ILocalPluginInfo pluginInfo, IPluginManager manager, ILogService log)
    {
        _pluginInfo = pluginInfo;
        _manager = manager;
        Id = pluginInfo.Id;

        Uninstall = ReactiveCommand.Create(UninstallImpl).DisposeItWith(Disposable);
        CancelUninstall = ReactiveCommand.Create(CancelUninstallImpl).DisposeItWith(Disposable);
        Name = pluginInfo.Title;
        Author = pluginInfo.Authors;
        Description = pluginInfo.Description;
        SourceName = pluginInfo.SourceUri;
        LocalVersion = $"{pluginInfo.Version} (API: {pluginInfo.ApiVersion})"; 
        IsUninstalled = pluginInfo.IsUninstalled;
        IsLoaded = pluginInfo.IsLoaded;
        LoadingError = pluginInfo.LoadingError;
    }


    private void CancelUninstallImpl()
    {
        if (_pluginInfo == null)
        {
            throw new Exception("Plugin not installed");
        }

        _manager.CancelUninstall(_pluginInfo);
        IsUninstalled = false;
    }

    private void UninstallImpl()
    {
        if (_pluginInfo == null)
        {
            throw new Exception("Plugin not installed");
        }

        _manager.Uninstall(_pluginInfo);
        IsUninstalled = true;
    }

    public string Id { get; set; }
    public string? Author { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string SourceName { get; set; }
    public string LocalVersion { get; set; }
    [Reactive] public string LoadingError { get; set; }
    [Reactive] public bool IsLoaded { get; set; }
    [Reactive] public bool IsUninstalled { get; set; }
    public ReactiveCommand<Unit, Unit> Uninstall { get; }
    public ReactiveCommand<Unit, Unit> CancelUninstall { get; }
}