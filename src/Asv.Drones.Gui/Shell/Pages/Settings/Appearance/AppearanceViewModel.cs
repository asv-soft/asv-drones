using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Windows.Input;
using Asv.Avalonia.Map;
using Asv.Common;
using Asv.Drones.Gui.Api;
using DynamicData.Binding;
using Material.Icons;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Asv.Drones.Gui;

public class AppearanceViewModel : TreePageWithValidationViewModel
{
    private readonly IApplicationHost _host;
    private readonly ILocalizationService _loc;
    private readonly IMapService _map;

    public AppearanceViewModel() : base(WellKnownUri.ShellPageSettingsAppearanceUri)
    {
        DesignTime.ThrowIfNotDesignMode();
        Info = AppInfo.DesignTimeInstance;
    }

    public AppearanceViewModel(ISettingsPageContext context, IApplicationHost host, ILocalizationService loc,
        IMapService map, IPluginManager pluginManager) : base(WellKnownUri.ShellPageSettingsAppearanceUri)
    {
        _host = host ?? throw new ArgumentNullException(nameof(host));
        _loc = loc ?? throw new ArgumentNullException(nameof(loc));
        _map = map;
        Info = host.Info;
        FullVersionString = $"{host.Info.Version} (API: {pluginManager.ApiVersion})" ;
        AvailableAccessMode = new List<MapAccessPair>()
        {
            new() { Mode = AccessMode.ServerOnly, Name = RS.AppearanceViewModel_ServerOnly_AccessMode },
            new() { Mode = AccessMode.ServerAndCache, Name = RS.AppearanceViewModel_ServerAndCache_AccessMode },
            new() { Mode = AccessMode.CacheOnly, Name = RS.AppearanceViewModel_CacheOnly_AccessMode }
        };
        CurrentMapAccessMode = AvailableAccessMode.First(v => v.Mode == map.CurrentMapAccessMode.Value);
        host.CurrentTheme.Subscribe(x => SelectedTheme = x).DisposeItWith(Disposable);
        this.WhenValueChanged(x => x.SelectedTheme, false)
            .Subscribe(_host.CurrentTheme!)
            .DisposeItWith(Disposable);

        _loc.CurrentLanguage.Subscribe(x => SelectedLanguage = x).DisposeItWith(Disposable);
        this.WhenValueChanged(x => x.SelectedLanguage, false)
            .Do(x => context.SetRebootRequired())
            .Subscribe(_loc.CurrentLanguage!)
            .DisposeItWith(Disposable);

        map.CurrentMapProvider.Subscribe(provider => { CurrentMapProvider = provider; }).DisposeItWith(Disposable);

        this.WhenAnyValue(vm => vm.CurrentMapProvider)
            .Subscribe(provider => map.CurrentMapProvider.OnNext(provider))
            .DisposeItWith(Disposable);

        this.WhenAnyValue(vm => vm.CurrentMapAccessMode)
            .Subscribe(mode =>
            {
                // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
                if (mode == null) return;
                map.CurrentMapAccessMode.OnNext(mode.Mode);
            })
            .DisposeItWith(Disposable);

        ClearMapStorageCommand = ReactiveCommand.Create(ClearMapStorage).DisposeItWith(Disposable);

        UpdateDescription();
    }
    
    public string FullVersionString { get; set; }

    private void ClearMapStorage()
    {
        if (string.IsNullOrWhiteSpace(_map.MapCacheDirectory)) return;

        try
        {
            var dir = new DirectoryInfo(_map.MapCacheDirectory);

            foreach (var subDirectory in dir.GetDirectories())
            {
                subDirectory.Delete(recursive: true);
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
        }

        UpdateDescription();
    }

    private void UpdateDescription()
    {
        MapStorageDescription = string.Format(RS.MapSettingsView_MapsInfo_Description, _map.MapCacheDirectory,
            _loc.ByteSize.ConvertToStringWithUnits(_map.CalculateMapCacheSize()));
    }

    public IAppInfo Info { get; }

    #region Theme

    public IEnumerable<IThemeInfo> AppThemes => _host.Themes;
    [Reactive] public IThemeInfo? SelectedTheme { get; set; }

    #endregion

    #region Language

    public string LanguageIcon => MaterialIconDataProvider.GetData(MaterialIconKind.Translate);
    [Reactive] public ILanguageInfo? SelectedLanguage { get; set; }
    public IEnumerable<ILanguageInfo> AppLanguages => _loc.AvailableLanguages;

    #endregion

    #region Map

    [Reactive] public MapAccessPair CurrentMapAccessMode { get; set; }

    public IEnumerable<MapAccessPair> AvailableAccessMode { get; set; }

    [Reactive] public GMapProvider CurrentMapProvider { get; set; }
    public IEnumerable<GMapProvider> AvailableProviders => _map.AvailableProviders;
    [Reactive] public string MapStorageDescription { get; set; }
    public ICommand ClearMapStorageCommand { get; set; }
    public string MapAccessIcon => MaterialIconDataProvider.GetData(MaterialIconKind.FolderArrowUpDown);
    public string MapIcon => MaterialIconDataProvider.GetData(MaterialIconKind.Map);
    public string FolderIcon => MaterialIconDataProvider.GetData(MaterialIconKind.Database);

    #endregion
}

public class MapAccessPair
{
    public AccessMode Mode { get; set; }
    public string Name { get; set; }
}