using System;
using System.Collections.Generic;
using System.Composition;
using System.Composition.Convention;
using System.Composition.Hosting;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Reflection;
using Asv.Avalonia;
using Asv.Avalonia.GeoMap;
using Asv.Avalonia.IO;
using Asv.Avalonia.Plugins;
using Asv.Cfg;
using Asv.Common;
using Asv.Drones.Api;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls.Templates;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using R3;

namespace Asv.Drones;

public partial class App : Application, IContainerHost, IShellHost
{
    private readonly CompositionHost _container;
    private IShell _shell;
    private readonly Subject<IShell> _onShellLoaded = new();

    public App()
    {
        var conventions = new ConventionBuilder();
        var containerCfg = new ContainerConfiguration();

        if (Design.IsDesignMode)
        {
            containerCfg
                .WithExport(NullContainerHost.Instance)
                .WithExport<IConfiguration>(new InMemoryConfiguration())
                .WithExport(NullLoggerFactory.Instance)
                .WithExport(NullAppPath.Instance)
                .WithExport(NullPluginManager.Instance)
                .WithExport(NullLogService.Instance)
                .WithExport(NullAppInfo.Instance)
                .WithExport<IDataTemplateHost>(this)
                .WithExport<IShellHost>(this)
                .WithExport<IMeterFactory>(new DefaultMeterFactory())
                .WithExport(TimeProvider.System)
                .WithDefaultConventions(conventions);
        }
        else
        {
            var pluginManager = AppHost.Instance.GetService<IPluginManager>();
            containerCfg
                .WithExport<IContainerHost>(this)
                .WithExport(AppHost.Instance.GetService<IConfiguration>())
                .WithExport(AppHost.Instance.GetService<ILoggerFactory>())
                .WithExport(AppHost.Instance.GetService<ILogService>())
                .WithExport(AppHost.Instance.GetService<IAppPath>())
                .WithExport(AppHost.Instance.GetService<IAppInfo>())
                .WithExport(AppHost.Instance.GetService<IMeterFactory>())
                .WithExport(pluginManager)
                .WithAssemblies(pluginManager.PluginsAssemblies)
                .WithExport<IDataTemplateHost>(this)
                .WithExport<IShellHost>(this)
                .WithExport(TimeProvider.System)
                .WithDefaultConventions(conventions);
        }

        containerCfg = containerCfg.WithAssemblies(DefaultAssemblies.Distinct());

        // TODO: load plugin manager before creating container
        _container = containerCfg.CreateContainer();
        DataTemplates.Add(new CompositionViewLocator(_container));
    }

    private IEnumerable<Assembly> DefaultAssemblies
    {
        get
        {
            yield return GetType().Assembly; // Asv.Drones
            yield return typeof(IFlightMode).Assembly; // Asv.Drones.Api
            yield return typeof(AppHost).Assembly; // Asv.Avalonia
            yield return typeof(DeviceManager).Assembly; // Asv.Avalonia.IO
            yield return typeof(IPluginManager).Assembly; // Asv.Avalonia.Plugins
        }
    }

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (Design.IsDesignMode)
        {
            Shell = DesignTimeShellViewModel.Instance;
        }
        else if (Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            Shell = _container.GetExport<IShell>(DesktopShellViewModel.ShellId);
            if (desktop.MainWindow is TopLevel topLevel)
            {
                TopLevel = topLevel;
            }
        }
        else if (Current?.ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform)
        {
            Shell = _container.GetExport<IShell>(MobileShellViewModel.ShellId);
            if (singleViewPlatform.MainView is TopLevel topLevel)
            {
                TopLevel = topLevel;
            }
        }
        else
        {
            throw new Exception("Unknown platform");
        }

        base.OnFrameworkInitializationCompleted();
#if DEBUG
        this.AttachDevTools();
#endif
    }

    public T GetExport<T>()
        where T : IExportable
    {
        return _container.GetExport<T>();
    }

    public T GetExport<T>(string contract)
        where T : IExportable
    {
        return _container.GetExport<T>(contract);
    }

    public bool TryGetExport<T>(string id, out T value)
        where T : IExportable
    {
        return _container.TryGetExport(id, out value);
    }

    public void SatisfyImports(object value)
    {
        _container.SatisfyImports(value);
    }

    public IShell Shell
    {
        get => _shell;
        private set
        {
            _shell = value;
            _onShellLoaded.OnNext(value);
        }
    }

    public Observable<IShell> OnShellLoaded => _onShellLoaded;

    public TopLevel TopLevel { get; private set; }
    public IExportInfo Source => SystemModule.Instance;
}
