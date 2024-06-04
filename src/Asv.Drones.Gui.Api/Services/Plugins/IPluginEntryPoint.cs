namespace Asv.Drones.Gui.Api
{
    /// <summary>
    /// This interface is used as the entry point when loading plugins
    /// </summary>
    public interface IPluginEntryPoint
    {
        /// <summary>
        /// Call when initializes the application Application.Initialize()
        /// Will be called before main window\activity is shown
        /// </summary>
        void Initialize();

        /// <summary>
        /// Will be called after main window\activity is shown and Application.nFrameworkInitializationCompleted()
        /// </summary>
        void OnFrameworkInitializationCompleted();
    }


    public interface IPluginMetadata
    {
        string[] Dependency { get; }
        string Name { get; }
    }

    public class PluginMetadata : IPluginMetadata
    {
        public string[] Dependency { get; set; }
        public string Name { get; set; }
    }
}