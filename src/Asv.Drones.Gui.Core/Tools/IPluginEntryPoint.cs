using System.ComponentModel.Composition;

namespace Asv.Drones.Gui.Core
{
    /// <summary>
    /// Represents an entry point for a plugin.
    /// </summary>
    public interface IPluginEntryPoint
    {
        /// <summary>
        /// Initializes the application by loading XAML etc.
        /// </summary>
        void Initialize();

        /// <summary>
        /// Method called when the framework initialization is completed and the application can start.
        /// </summary>
        /// <remarks>
        /// This method is typically used to perform any necessary setup or initialization tasks after the framework has completed its initialization process.
        /// </remarks>
        void OnFrameworkInitializationCompleted();

        /// <summary>
        /// Called when a shutdown is requested. It is typically used to perform any final cleanup operations before the application exits. </summary> <remarks>
        /// This method can be overridden in a derived class to provide custom shutdown behavior. </remarks> <seealso cref="Application.ShutdownRequested"/>
        /// /
        void OnShutdownRequested();
    }


    /// <summary>
    /// Represents an attribute used to mark a plugin entry point.
    /// </summary>
    /// <remarks>
    /// The PluginEntryPointAttribute is used to specify the name of the plugin and its dependencies.
    /// </remarks>
    [MetadataAttribute]
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class PluginEntryPointAttribute : ExportAttribute, IPluginMetadata
    {
        /// <summary>
        /// Represents an attribute used to mark a plugin entry point.
        /// </summary>
        /// <remarks>
        /// The PluginEntryPointAttribute is used to specify the name of the plugin and its dependencies.
        /// </remarks>
        /// <param name="name">The name of the plugin entry point.</param>
        /// <param name="dependency">An array of strings representing the dependencies of the plugin.</param>
        public PluginEntryPointAttribute(string name, params string[] dependency)
            : base(typeof(IPluginEntryPoint))
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(name));
            Name = name;
            Dependency = dependency;
        }

        /// <summary>
        /// Gets the dependencies of the property.
        /// </summary>
        /// <returns>
        /// An array of strings representing the dependencies of the property.
        /// </returns>
        public string[] Dependency { get; }

        /// <summary>
        /// Gets the name of the property.
        /// </summary>
        /// <value>
        /// The name of the property.
        /// </value>
        public string Name { get; }
    }

    /// <summary>
    /// Represents the metadata for a plugin.
    /// </summary>
    public interface IPluginMetadata
    {
        /// <summary>
        /// Gets or sets the dependencies of the current object.
        /// </summary>
        /// <returns>
        /// An array of strings representing the dependencies of the current object.
        /// </returns>
        string[] Dependency { get; }

        /// <summary>
        /// Gets the name of the property.
        /// </summary>
        /// <value>
        /// A string representing the property name.
        /// </value>
        string Name { get; }
    }
}