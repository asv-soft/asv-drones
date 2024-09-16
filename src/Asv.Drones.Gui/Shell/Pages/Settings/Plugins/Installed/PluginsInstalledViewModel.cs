using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using Asv.Common;
using Asv.Drones.Gui.Api;
using DynamicData;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Asv.Drones.Gui;

public class PluginsInstalledViewModel : TreePageViewModel
{
    private readonly IPluginManager _manager;
    private readonly ILogService _log;
    private readonly ReadOnlyObservableCollection<InstalledPluginInfoViewModel> _plugins;
    private readonly SourceCache<ILocalPluginInfo, string> _pluginSearchSource;

    public PluginsInstalledViewModel() : base(WellKnownUri.UndefinedUri)
    {
        DesignTime.ThrowIfNotDesignMode();
        _plugins = new ReadOnlyObservableCollection<InstalledPluginInfoViewModel>(
            new ObservableCollection<InstalledPluginInfoViewModel>(
                new[]
                {
                    new InstalledPluginInfoViewModel
                    {
                        Id = "#1",
                        Author = "Asv Soft",
                        SourceName = "Nuget",
                        Name = "Example1",
                        Description = "Example plugin",
                        LocalVersion = "1.0.0",
                        IsUninstalled = true,
                    },
                    new InstalledPluginInfoViewModel
                    {
                        Id = "#2",
                        Author = "Asv Soft",
                        SourceName = "Github",
                        Name = "Example2",
                        Description = "Example plugin",
                        LocalVersion = "0.1.0",
                    }
                }));
        SelectedPlugin = _plugins.First();
    }

    public PluginsInstalledViewModel(IPluginManager manager, ILogService log, ITreePageContext context) : base(
        WellKnownUri.ShellPageSettingsPluginsLocal)
    {
        ArgumentNullException.ThrowIfNull(log);
        _manager = manager ?? throw new ArgumentNullException(nameof(manager));
        _log = log;

        _pluginSearchSource = new SourceCache<ILocalPluginInfo, string>(x => x.Id).DisposeItWith(Disposable);
        _pluginSearchSource.Connect().Transform(x => new InstalledPluginInfoViewModel(x, _manager, _log))
            .Bind(out _plugins).Subscribe().DisposeItWith(Disposable);
        Search = ReactiveCommand.CreateRunInBackground(SearchImpl).DisposeItWith(Disposable);
        Search.Execute().Subscribe();
    }

    public ReactiveCommand<Unit, Unit> Search { get; set; }

    private void SearchImpl()
    {
        _pluginSearchSource.AddOrUpdate(OnlyVerified
            ? _manager.Installed.Where(item => item.IsVerified)
            : _manager.Installed);
    }

    public ReadOnlyObservableCollection<InstalledPluginInfoViewModel> Plugins => _plugins;
    [Reactive] public bool OnlyVerified { get; set; } = true;
    [Reactive] public string SearchString { get; set; }
    [Reactive] public InstalledPluginInfoViewModel SelectedPlugin { get; set; }
}