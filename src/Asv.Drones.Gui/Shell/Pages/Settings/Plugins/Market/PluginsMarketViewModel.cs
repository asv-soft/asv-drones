using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Threading;
using System.Threading.Tasks;
using Asv.Cfg;
using Asv.Common;
using Asv.Drones.Gui.Api;
using DynamicData;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Asv.Drones.Gui;

public class PluginsMarketViewModel : TreePageViewModel
{
    private readonly IPluginManager _manager;
    private readonly ILogService _log;
    private readonly SourceCache<PluginInfoViewModel, string> _pluginSearchSource;
    private readonly ReadOnlyObservableCollection<PluginInfoViewModel> _plugins;
    private readonly IConfiguration _cfg;
    private string _previouslySelectedPluginId;
    
    public PluginsMarketViewModel() : base(WellKnownUri.UndefinedUri)
    {
        DesignTime.ThrowIfNotDesignMode();
        _plugins = new ReadOnlyObservableCollection<PluginInfoViewModel>(new ObservableCollection<PluginInfoViewModel>(
            new[]
            {
                new PluginInfoViewModel
                {
                    Id = "#1",
                    Author = "Asv Soft",
                    SourceName = "Nuget",
                    Name = "Example1",
                    Description = "Example plugin",
                    LastVersion = "1.0.0",
                    IsInstalled = true,
                    LocalVersion = "3.4.5",
                },
                new PluginInfoViewModel
                {
                    Id = "#2",
                    Author = "Asv Soft",
                    SourceName = "Github",
                    Name = "Example2",
                    Description = "Example plugin",
                    LastVersion = "0.1.0",
                }
            }));
        SelectedPlugin = _plugins.First();
    }

    public PluginsMarketViewModel(IPluginManager manager, ILogService log, IConfiguration cfg) : base(WellKnownUri
        .ShellPageSettingsPluginsMarketUri)
    {
        ArgumentNullException.ThrowIfNull(log);
        _manager = manager ?? throw new ArgumentNullException(nameof(manager));
        _log = log;
        _cfg = cfg;

        _pluginSearchSource = new SourceCache<PluginInfoViewModel, string>(v => v.Id).DisposeItWith(Disposable);
        _pluginSearchSource.Connect().Bind(out _plugins).Subscribe().DisposeItWith(Disposable);
        Search = new CancellableCommandWithProgress<Unit, Unit>(SearchImpl, "Search", log, TaskPoolScheduler.Default)
            .DisposeItWith(Disposable);
        InstallManually = ReactiveCommand.CreateFromTask<IProgress<double>, Unit>(InstallManuallyImpl)
            .DisposeItWith(Disposable);
    }

    private async Task<Unit> SearchImpl(Unit arg, IProgress<double> progress, CancellationToken cancel)
    {
        var items = await _manager.Search(SearchQuery.Empty, cancel);
        
        if (SelectedPlugin is not null)
        {
            _previouslySelectedPluginId = SelectedPlugin.Id;
        }
        
        SelectedPlugin = null;
        _pluginSearchSource.Clear();
        var filteredItems = OnlyVerified 
            ? items.Where(item => item.IsVerified) 
            : items;
        _pluginSearchSource.AddOrUpdate(filteredItems.Select(item => new PluginInfoViewModel(item, _manager, _log)));
        SelectedPlugin = _plugins.FirstOrDefault(plugin => plugin.Id == _previouslySelectedPluginId) ?? _plugins.First();
        return Unit.Default;
    }
    
    private async Task<Unit> InstallManuallyImpl(IProgress<double> progress, CancellationToken cancel)
    {
        var installer = new PluginInstaller(_cfg, _log, _manager);
        await installer.ShowInstallDialog();
        
        return Unit.Default;
    }
    
    public CancellableCommandWithProgress<Unit, Unit> Search { get; }
    public ReactiveCommand<IProgress<double>, Unit> InstallManually { get; }
    public ReadOnlyObservableCollection<PluginInfoViewModel> Plugins => _plugins;
    [Reactive] public bool OnlyVerified { get; set; } = true;
    [Reactive] public string SearchString { get; set; }
    [Reactive] public PluginInfoViewModel? SelectedPlugin { get; set; }
}