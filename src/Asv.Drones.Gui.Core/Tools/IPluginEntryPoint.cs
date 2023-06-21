using System.ComponentModel.Composition;

namespace Asv.Drones.Gui.Core
{
    /// <summary>
    /// This interface is used as the entry point when loading the program
    /// </summary>
    public interface IPluginEntryPoint
    {
        /// <summary>
        /// Call when initializes the application by loading XAML etc.
        /// </summary>
        void Initialize();

        void OnFrameworkInitializationCompleted();
        void OnShutdownRequested();
    }


    /// <summary>
    /// This attribute is used to find a matching plugin entry points
    /// </summary>
    [MetadataAttribute]
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class PluginEntryPointAttribute : ExportAttribute, IPluginMetadata
    {
        public PluginEntryPointAttribute(string name, params string[] dependency)
            : base(typeof(IPluginEntryPoint))
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(name));
            Name = name;
            Dependency = dependency;
        }

        public string[] Dependency { get; }
        public string Name { get; }
        public int LoadingOrder { get; }
    }

    public interface IPluginMetadata
    {
        string[] Dependency { get; }
        string Name { get; }
    }
}