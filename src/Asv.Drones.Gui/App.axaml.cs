using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using Asv.Cfg;
using Asv.Cfg.ImMemory;
using Asv.Cfg.Json;
using Asv.Drones.Gui.Core;
using System.Collections.Generic;
using System.Reflection;
using System.ComponentModel.Composition.Primitives;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Asv.Drones.Gui.Uav;
using FluentAvalonia.Styling;

namespace Asv.Drones.Gui
{
    public partial class App : Application
    {
        private readonly CompositionContainer _container;

        public App()
        {
            _container = new CompositionContainer(new AggregateCatalog(Catalogs().ToArray()), CompositionOptions.IsThreadSafe);
            var batch = new CompositionBatch();
            RegisterDefaultServices(batch);
            _container.Compose(batch);
        }

        private void RegisterDefaultServices(CompositionBatch batch)
        {
            batch.AddExportedValue(new ViewLocator(_container));
            batch.AddExportedValue(_container);
            batch.AddExportedValue(GetAppInfo());
            var path = GetAppPathInfo();
            batch.AddExportedValue(GetAppPathInfo());
            var config = new JsonOneFileConfiguration(path.ApplicationConfigFilePath, true, null);
            batch.AddExportedValue<IConfiguration>(config);
            batch.AddExportedValue<ILocalizationService>(new LocalizationServiceBase(config));
        }

        

        #region AppInfo

        private class AppInfo : IAppInfo
        {
            public AppInfo(string name, string version, string author, string appUrl, string appLicense)
            {
                Name = name;
                Version = version;
                Author = author;
                AppUrl = appUrl;
                AppLicense = appLicense;

            }

            public string Name { get; }
            public string Version { get; }
            public string Author { get; }
            public string AppUrl { get; }
            public string AppLicense { get; }
            public string CurrentAvaloniaVersion
            {
                get
                {
                    return typeof(AppBuilder).Assembly.GetName().Version?.ToString();
                }
            }
        }

        private IAppInfo GetAppInfo()
        {
            var assm = GetType().Assembly.GetName();
            return new AppInfo(assm.Name ?? "Asv.Drones", assm.Version?.ToString() ?? "0.0.0",
                "https://github.com/asvol", "https://github.com/asvol/asv-drones", "MIT License");
        }


        #endregion

        #region AppPathInfo

        private class AppPathInfo:IAppPathInfo
        {
            public AppPathInfo()
            {
                CurrentDirectory = Environment.CurrentDirectory;
#if DEBUG
                ApplicationDataFolder = Path.Combine(CurrentDirectory, "AsvDrones");
                ApplicationConfigFilePath = Path.Combine(ApplicationDataFolder, "config.json");
#else
                ApplicationDataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "AsvDrones") ?? Path.Combine(CurrentDirectory,"Data");
                ApplicationConfigFilePath = Path.Combine(ApplicationDataFolder, "config.json");
#endif
                
            }

            public string CurrentDirectory { get; }
            public string ApplicationDataFolder { get; }
            public string ApplicationConfigFilePath { get; }

        }

        private IAppPathInfo GetAppPathInfo()
        {
            return new AppPathInfo();
        }

        #endregion

        private IEnumerable<Assembly> Assemblies()
        {
            yield return typeof(ExportViewAttribute).Assembly;
            yield return typeof(IMavlinkDevicesService).Assembly;

        }

        private IEnumerable<ComposablePartCatalog> Catalogs()
        {
            return Assemblies().Distinct().Select(assembly => new AssemblyCatalog(assembly));

            // Enable this feature to load plugins at runtime

            // var dir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            // if (dir != null)
            // {
            //     var cat = new DirectoryCatalog(dir, "Asv.Drones.Gui.Plugins.*.dll");
            //     cat.Refresh();
            //     yield return cat;
            // }
        }

        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
            DataTemplates.Add(_container.GetExportedValue<ViewLocator>() ?? throw new InvalidOperationException());
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.MainWindow = new MainWindow()
                {
                    DataContext = _container.GetExportedValue<ShellViewModel>()
                };
                desktop.ShutdownRequested += (_, _) =>
                {
                    _container.GetExportedValue<ViewLocator>()?.Dispose();
                    _container.Dispose();
                };
            }
            base.OnFrameworkInitializationCompleted();
        }
    }
}
