using System;
using System.Composition;
using System.Composition.Convention;
using System.Composition.Hosting;
using Asv.Avalonia;
using Asv.Avalonia.GeoMap;
using Asv.Avalonia.IO;
using Asv.Avalonia.Plugins;
using Asv.Drones.Api;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls.Templates;
using Avalonia.Markup.Xaml;
using R3;

namespace Asv.Drones;

public partial class App : Application, IContainerHost, IShellHost
{
    private IShell _shell;
    private readonly CompositionHost _container;
    private readonly Subject<IShell> _onShellLoaded = new();

    public App()
    {
        var conventions = new ConventionBuilder();
        var containerCfg = new ContainerConfiguration();

        containerCfg
            .WithDependenciesFromSystemModule()
            .WithDependenciesFromIoModule()
            .WithDependenciesFromPluginManagerModule()
            .WithDependenciesFromGeoMapModule()
            .WithDependenciesFromApi()
            .WithDependenciesFromTheApp(this)
            .WithDefaultConventions(conventions);

        _container = containerCfg.CreateContainer();
        DataTemplates.Add(new CompositionViewLocator(_container));

        if (!Design.IsDesignMode)
        {
            _container.GetExport<IAppStartupService>().AppCtor();
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
        if (!Design.IsDesignMode)
        {
            _container.GetExport<IAppStartupService>().OnFrameworkInitializationCompleted();
        }
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

    public void Dispose()
    {
        _container.Dispose();
        _shell.Dispose();
        _onShellLoaded.Dispose();
        GC.SuppressFinalize(this);
    }
}

public static class ContainerConfigurationMixin
{
    public static ContainerConfiguration WithDependenciesFromTheApp(
        this ContainerConfiguration containerConfiguration,
        App app
    )
    {
        containerConfiguration.WithExport<IDataTemplateHost>(app).WithExport<IShellHost>(app);

        if (Design.IsDesignMode)
        {
            containerConfiguration.WithExport(NullContainerHost.Instance);
        }
        else
        {
            containerConfiguration.WithExport<IContainerHost>(app);
        }

        return containerConfiguration.WithAssemblies([app.GetType().Assembly]);
    }
}
