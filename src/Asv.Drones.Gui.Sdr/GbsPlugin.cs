using System.ComponentModel.Composition;
using Asv.Drones.Gui.Core;

namespace Asv.Drones.Gui.Gbs;

[PluginEntryPoint(Name, CorePlugin.Name)]
[PartCreationPolicy(CreationPolicy.Shared)]
public class SdrPlugin:IPluginEntryPoint
{
    private const string Name = "Sdr";

    public void Initialize()
    {
        
    }

    public void OnFrameworkInitializationCompleted()
    {
        
    }

    public void OnShutdownRequested()
    {
        
    }
}