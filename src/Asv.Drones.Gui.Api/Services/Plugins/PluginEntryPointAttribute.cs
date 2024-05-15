using System.Composition;

namespace Asv.Drones.Gui.Api;

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
}