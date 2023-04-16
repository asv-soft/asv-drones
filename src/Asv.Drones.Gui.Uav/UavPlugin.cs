using Asv.Drones.Gui.Core;
using System.ComponentModel.Composition;

namespace Asv.Drones.Gui.Uav
{
    /// <summary>
    /// This module only imports all UAV services to be created by IoC container
    /// </summary>
    [PluginEntryPoint("Core", LoadingOrder)]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class UavPlugin:IPluginEntryPoint
    {
        public const int LoadingOrder = CoreServicePlugin.LoadingOrder + byte.MaxValue; // load after services loaded

        [ImportingConstructor]
        public UavPlugin(IMavlinkDevicesService mavlinkDevices)
        {
            
        }

        public void Initialize()
        {
            // do nothing
        }

        public void OnFrameworkInitializationCompleted()
        {
            // do nothing
        }

        public void OnShutdownRequested()
        {
            // do nothing
        }
    }
}