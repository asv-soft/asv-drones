using System;
using System.Composition.Hosting;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using NLog;

namespace Asv.Drones.Gui;

public class PluginAssemblyLoadContext : AssemblyLoadContext
{
    private readonly Logger Logger;
    private readonly string _pluginFolder;

    public PluginAssemblyLoadContext(string pluginPath, ContainerConfiguration containerCfg)
    {
        Logger = LogManager.GetLogger(Path.GetFileNameWithoutExtension(pluginPath));
        _pluginFolder = Path.GetFullPath(pluginPath);
        foreach (var file in Directory.EnumerateFiles(pluginPath, NugetHelper.NugetPluginName + "*.dll",
                     SearchOption.AllDirectories))
        {
            var fullPath = Path.GetFullPath(file);
            try
            {
                containerCfg.WithAssembly(LoadFromAssemblyPath(fullPath));
            }
            catch (Exception e)
            {
                Logger.Error(e, "Error load plugin assembly {0}", fullPath);
            }
        }
    }

    protected override Assembly? Load(AssemblyName assemblyName)
    {
        // if assembly already loaded at main context => return null (it will be loaded by main context)
        if (Default.Assemblies.Any(x => x.GetName().Name == assemblyName.Name))
        {
            return null;
        }

        // this plugin wants to load assembly, but main context not contains it yet => try to load from main context
        try
        {
            return Default.LoadFromAssemblyName(assemblyName);
        }
        catch (Exception e)
        {
            Logger.Warn("Assembly {0} not found at main context", assemblyName.Name);
        }

        // if we here, it's mean that assembly not found at main context => try to load from plugin folder
        foreach (var file in Directory.GetFiles(_pluginFolder, assemblyName.Name + ".dll", SearchOption.AllDirectories))
        {
            var fullPath = Path.GetFullPath(file);
            Logger.Info("Load assembly {0} from plugin folder {1}", assemblyName, fullPath);
            return LoadFromAssemblyPath(fullPath);
        }

        return null;
    }
}