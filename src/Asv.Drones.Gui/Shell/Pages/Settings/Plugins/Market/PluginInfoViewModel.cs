using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Threading;
using System.Threading.Tasks;
using Asv.Common;
using Asv.Drones.Gui.Api;
using DynamicData;
using DynamicData.Binding;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Asv.Drones.Gui;

public class PluginInfoViewModel : DisposableReactiveObject
{
    private readonly IPluginSearchInfo _pluginInfo;
    private readonly IPluginManager _manager;
    private readonly SourceCache<string, string> _pluginAllVersions;
    private readonly ReadOnlyObservableCollection<string> _pluginVersions;
    private readonly ILocalPluginInfo? _localInfo;

    public PluginInfoViewModel()
    {
        DesignTime.ThrowIfNotDesignMode();
        Install = CoreDesignTime.CancellableCommand<Unit, Unit>();
        Uninstall = CoreDesignTime.CancellableCommand<Unit, Unit>();
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
        IsInstalled = _manager.IsInstalled(pluginInfo.PackageId, out _localInfo);
        Name = pluginInfo.Title;
        Author = pluginInfo.Authors;
        Description = pluginInfo.Description;
        SourceName = pluginInfo.Source.Name;
        SourceUri = pluginInfo.Source.SourceUri;
        LastVersion = $"{pluginInfo.LastVersion} (API: {pluginInfo.ApiVersion})";
        IsApiCompatible = pluginInfo.ApiVersion == manager.ApiVersion;
        LocalVersion = (_localInfo != null) ? $"{_localInfo?.Version} (API: {_localInfo?.ApiVersion})" : null;
        
        _pluginAllVersions = new SourceCache<string, string>(s => s).DisposeItWith(Disposable);
        
        _pluginAllVersions
            .Connect()
            .Sort(SortExpressionComparer<string>.Descending(x => x))
            .Bind(out _pluginVersions)
            .Subscribe()
            .DisposeItWith(Disposable);

        Task.Factory.StartNew(GetPreviousVersions);
    }
    
    public bool IsApiCompatible { get; set; }
    public string Id { get; set; }
    public string? Author { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string SourceName { get; set; }
    public string SourceUri { get; set; }
    public string LastVersion { get; set; }
    public string? LocalVersion { get; set; }
    public bool IsInstalled { get; set; }
    public ReadOnlyObservableCollection<string> PluginVersions => _pluginVersions;

    private async Task<Unit> UninstallImpl(Unit arg, IProgress<double> progress, CancellationToken cancel)
    {
        if (_localInfo == null)
        {
            throw new Exception("Plugin not installed");
        }

        _manager.Uninstall(_localInfo);
        return Unit.Default;
    }

    private async Task<Unit> InstallImpl(Unit arg, IProgress<double> progress, CancellationToken cancel)
    {
        await _manager.Install(_pluginInfo.Source, _pluginInfo.PackageId, SelectedVersion,
            new Progress<ProgressMessage>(
                _ => { progress.Report(_.Progress); }), cancel);
        return Unit.Default;
    }

    private async Task<Unit> GetPreviousVersions()
    {
        var searchQuery = new SearchQuery()
        {
            Name = Name,
            IncludePrerelease = true,
        };

        foreach (var server in _manager.Servers)
        {
            searchQuery.Sources.Add(server.SourceUri);
        }

        var previousVersions = await _manager.ListPluginVersions(searchQuery, _pluginInfo.PackageId, new CancellationToken());
        _pluginAllVersions.Clear();
        _pluginAllVersions.AddOrUpdate(previousVersions);

        if (_pluginVersions.Count > 0)
        {
            SelectedVersion = _pluginVersions.First();
        }

        return Unit.Default;
    }

    public CancellableCommandWithProgress<Unit, Unit> Uninstall { get; }
    public CancellableCommandWithProgress<Unit, Unit> Install { get; }
    [Reactive] public string SelectedVersion { get; set; }
}