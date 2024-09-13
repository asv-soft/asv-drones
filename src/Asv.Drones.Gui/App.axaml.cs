using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition.Convention;
using System.Composition.Hosting;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using Asv.Cfg;
using Asv.Cfg.Json;
using Asv.Common;
using Asv.Drones.Gui.Api;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Styling;
using FluentAvalonia.Styling;
using Microsoft.Extensions.Logging;
using ZLogger;

namespace Asv.Drones.Gui;

public class AppHostConfig
{
    public string? Theme { get; set; } = FluentAvaloniaTheme.DarkModeString;
}

public partial class App : Application, IApplicationHost
{

    private ThemeItem[] _themes;

    private bool _initThemeFirst;
    private readonly Stack<KeyValuePair<IPluginMetadata, IPluginEntryPoint>> _plugins = [];
    private readonly ILogger<App> _logger;

    public App()
    {
        // order of init members is important !!!
        Paths = InitApplicationPaths();
        Info = InitApplicationInfo();
        Logs = new LogService(Args["logs-folder", Path.Combine(Paths.AppDataFolder, "logs")]);
        _logger = Logs.CreateLogger<App>();
        Configuration = new JsonOneFileConfiguration(Args["config-file",Path.Combine(Paths.AppDataFolder, "config.json")], true,
                                                     TimeSpan.FromMilliseconds(100));
        Localization = new LocalizationServiceBase(Configuration);
        var conventions = new ConventionBuilder();
        var containerCfg = new ContainerConfiguration()
                           .WithExport(typeof(IAppPathInfo), Paths)
                           .WithExport(typeof(IAppInfo), Info)
                           .WithExport(typeof(IApplicationHost), this)
                           .WithExport(typeof(IDataTemplateHost), this)
                           .WithExport(typeof(IConfiguration), Configuration)
                           .WithExport(typeof(ILogService), Logs)
                           .WithExport(typeof(ILoggerFactory), Logs)
                           .WithExport(typeof(ILocalizationService), Localization)
                           .WithDefaultConventions(conventions);

        PluginManager = Design.IsDesignMode
                            ? NullPluginManager.Instance
                            : new PluginManager(containerCfg, Args["plugin-folder", Paths.AppDataFolder], Configuration, Logs);
        containerCfg = containerCfg
                       .WithExport(typeof(IPluginManager), PluginManager)
                       .WithAssemblies(DefaultAssemblies.Distinct()); // load dependencies from default assemblies        
        Container = containerCfg.CreateContainer();
        DataTemplateHost.DataTemplates.Add(new ViewLocator(Container));
    }
    
    
    private IEnumerable<Assembly> DefaultAssemblies
    {
        get
        {
            yield return GetType().Assembly;
            yield return typeof(ExportViewAttribute).Assembly;
        }
    }


    #region IApp host interface impl

    public ILocalizationService Localization { get; }
    public ILogService Logs { get; }
    public IAppArgs Args => AppArgs.Instance;
    public IAppInfo Info { get; }
    public IAppPathInfo Paths { get; }
    public IDataTemplateHost DataTemplateHost => this;
    public CompositionHost Container { get; }
    public IConfiguration Configuration { get; }
    public IPluginManager PluginManager { get; }
    public IEnumerable<IThemeInfo> Themes => _themes;
    public IRxEditableValue<IThemeInfo> CurrentTheme { get; private set; }
    public IShell? Shell { get; private set; }

    public void RestartApplication()
    {
        Environment.Exit(0);
    }


    #endregion

    #region Init functions

    private IAppPathInfo InitApplicationPaths()
    {
#if DEBUG
        var appDataBaseDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
#else
        var appDataBaseDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
#endif 
        
        
        
        // if (OperatingSystem.IsAndroid() || OperatingSystem.IsIOS()) // TODO: Check correct folders for OperatingSystem.IsAndroid() and OperatingSystem.IsIOS() ?

        var defaultPath = appDataBaseDirectory == null
            ? "asv-data-folder"
            : Path.Combine(appDataBaseDirectory, "asv-data-folder");
        
        var info = new AppPathInfo
        {
            AppDataFolder = Args["data-folder",defaultPath],
        };
        if (Directory.Exists(info.AppDataFolder) == false) Directory.CreateDirectory(info.AppDataFolder);
        return info;
    }

    private IAppInfo InitApplicationInfo()
    {
        const string zeroVersion = "0.0.0";
        var assemblyName = GetType().Assembly.GetName(); // hack - to get application assembly
        var version = assemblyName.Version != null
                          ? $"{assemblyName.Version.Major}.{assemblyName.Version.Minor}.{assemblyName.Version.Build}"
                          : zeroVersion;
        var name = assemblyName.Name ?? "Asv.Drones";
        const string author = "https://asv.me/";
        const string appUrl = "https://docs.asv.me/";
        const string licence = "MIT License";
        var avaloniaVersion =
            typeof(AppBuilder).Assembly.GetName().Version?.ToString() ?? zeroVersion; // hack to get Avalonia version
        return new AppInfo
        {
            Name = name,
            Version = version,
            Author = author,
            AppUrl = appUrl,
            AppLicense = licence,
            AvaloniaVersion = avaloniaVersion
        };
    }

    private void InitThemes()
    {
        _themes =
        [
            new ThemeItem(FluentAvaloniaTheme.DarkModeString, RS.ThemeService_Themes_Dark, ThemeVariant.Dark),
            new ThemeItem(FluentAvaloniaTheme.LightModeString, RS.ThemeService_Themes_Light, ThemeVariant.Light)
        ];

        CurrentTheme =
            new RxValue<IThemeInfo>(
                _themes.FirstOrDefault(item => item.Id.Equals(Configuration.Get<AppHostConfig>().Theme)) ??
                throw new InvalidOperationException());
        CurrentTheme.Subscribe(info =>
        {
            if (info is not ThemeItem item)
            {
                _logger.ZLogError($"Invalid theme item");
                return;
            }

            RequestedThemeVariant = item.Theme;
            // we no need to save theme on first init
            if (_initThemeFirst == false)
            {
                _initThemeFirst = true;
                return;
            }
            var config = Configuration.Get<AppHostConfig>();
            config.Theme = info.Id;
            Configuration.Set(config);
        });
    }

    #endregion

    #region Base overrided methods

    public override void Initialize()
    {
        if (Design.IsDesignMode)
        {
            AvaloniaXamlLoader.Load(this);
            // Default logic doesn't auto detect windows theme anymore in designer
            // to stop light mode, force here
            RequestedThemeVariant = ThemeVariant.Dark;
            return;
        }

        var plugins = Container.GetExports<Lazy<IPluginEntryPoint, PluginMetadata>>().ToImmutableArray();
        var sort = plugins.ToDictionary(_ => _.Metadata.Name, x => x.Metadata.Dependency);
        _logger.ZLogError($" Begin loading plugins [{plugins.Length} items]");
        // we need to sort plugins by dependency
        foreach (var name in DepthFirstSearch.Sort(sort))
        {
            try
            {
                _logger.ZLogTrace($"Init plugin {name}");
                var plugin = plugins.First(_ => _.Metadata.Name == name);
                var item = new KeyValuePair<IPluginMetadata, IPluginEntryPoint>(plugin.Metadata, plugin.Value);
                _plugins.Push(item);
                _logger.ZLogTrace($"Load plugin entry point '{plugin.Metadata.Name}' depended on [{string.Join(",", plugin.Metadata.Dependency)}]");
            }
            catch (Exception e)
            {
                _logger.ZLogError(e, $"Error to load plugin entry point: {name}:{e.Message}");
                if (Debugger.IsAttached)
                {
                    Debugger.Break();
                }
            }
        }

        AvaloniaXamlLoader.Load(this);
        InitThemes();
    }


    public override void OnFrameworkInitializationCompleted()
    {
        #region Initialize plugins

        foreach (var plugin in _plugins)
        {
            try
            {
                _logger.ZLogTrace($"Initialize plugin stage '{plugin.Key.Name}'");
                plugin.Value.Initialize();
            }
            catch (Exception e)
            {
                _logger.ZLogError(e, $"Error to initialize plugin entry point: {plugin.Key.Name}:{e.Message}");
                if (Debugger.IsAttached)
                {
                    Debugger.Break();
                }
            }
        }

        #endregion

        #region Create main window\activity

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow(Container)
            {
                DataContext = Shell = Container.GetExport<IShell>()
            };
        }
        else if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform)
        {
            singleViewPlatform.MainView = new ShellView()
            {
                DataContext = Shell = Container.GetExport<IShell>()
            };
        }

        #endregion

        base.OnFrameworkInitializationCompleted();

        #region OnFrameworkInitializationCompleted for plugins

        foreach (var plugin in _plugins)
        {
            try
            {
                plugin.Value.OnFrameworkInitializationCompleted();
                _logger.ZLogTrace($"Call OnFrameworkInitializationCompleted for plugin entry point '{plugin.Key.Name}'");
            }
            catch (Exception e)
            {
                _logger.ZLogError(e,
                                 $"Error to call OnFrameworkInitializationCompleted for plugin entry point: {plugin.Key.Name}:{e.Message}");
                if (Debugger.IsAttached)
                {
                    Debugger.Break();
                }
            }
        }

        #endregion
    }

    

    #endregion
}