﻿using System;
using System.Composition;
using Asv.Drones.Gui.Api;
using Material.Icons;

namespace Asv.Drones.Gui;

[Export(WellKnownUri.ShellPageSettings, typeof(ITreePageMenuItem))]
[method: ImportingConstructor]
public class PluginsSourcesTreeMenuItem(IPluginManager mng, ILogService log)
    : TreePageMenuItem(WellKnownUri.ShellPageSettingsPluginsSourceUri)
{
    public override Uri ParentId => WellKnownUri.ShellPageSettingsPluginsUri;
    public override string? Name => "Servers";
    public override string? Description => "Plugin source servers";
    public override MaterialIconKind Icon => MaterialIconKind.Web;
    public override int Order => 10;

    public override ITreePage? CreatePage(ITreePageContext context)
    {
        return new PluginsSourcesViewModel((ISettingsPageContext)context, mng, log);
    }
}