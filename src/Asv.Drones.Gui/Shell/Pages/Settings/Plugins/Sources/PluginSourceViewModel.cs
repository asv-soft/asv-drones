using System;
using System.Reactive;
using Asv.Drones.Gui.Api;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Asv.Drones.Gui;

public class PluginSourceViewModel : ReactiveObject
{
    public PluginSourceViewModel()
    {
        DesignTime.ThrowIfNotDesignMode();
    }

    public PluginSourceViewModel(IPluginServerInfo pluginServerInfo, PluginsSourcesViewModel pluginsSourcesViewModel)
    {
        ArgumentNullException.ThrowIfNull(pluginServerInfo);
        Id = pluginServerInfo.SourceUri;
        Name = pluginServerInfo.Name;
        SourceUri = pluginServerInfo.SourceUri;
        Remove = pluginsSourcesViewModel.Remove;
        Edit = pluginsSourcesViewModel.Edit;
        Model = pluginServerInfo;
    }

    public IPluginServerInfo Model { get; set; } = null!;
    public string Id { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string SourceUri { get; set; }
    public ReactiveCommand<PluginSourceViewModel, Unit> Edit { get; }
    public ReactiveCommand<PluginSourceViewModel, Unit> Remove { get; }
    [Reactive] public bool IsEnabled { get; set; }
}