using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Composition;
using System.Reactive;
using System.Threading.Tasks;
using Asv.Cfg;
using Asv.Common;
using Asv.Drones.Gui.Api;
using Material.Icons;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using ZLogger;

namespace Asv.Drones.Gui;

public class SettingsPageViewModelConfig
{
    public string? LastPageUri { get; set; }
}

[ExportShellPage(WellKnownUri.ShellPageSettings)]
public class SettingsPageViewModel : ShellPage, ISettingsPageContext
{
    private const string UriArgNameEmpty = "empty";
    private static ILogger<SettingsPageViewModel> _logger;
    private readonly IConfiguration _cfg;

    public static Uri GenerateUri(bool loadEmpty)
    {
        return new Uri($"{WellKnownUri.ShellPageSettings}?{UriArgNameEmpty}={loadEmpty}");
    }
    
    public SettingsPageViewModel() : this(TreePageExplorerDesignTime.Instances, new InMemoryConfiguration(), NullLogService.Instance, 
        AppInfo.DesignTimeInstance,NullLoggerFactory.Instance)
    {
        DesignTime.ThrowIfNotDesignMode();

        IsRebootRequired = true;
        Settings = new TreePageExplorerViewModel();
    }

    public TreePageExplorerViewModel Settings { get; }

    [ImportingConstructor]
    public SettingsPageViewModel(
        [ImportMany(WellKnownUri.ShellPageSettings)]
        IEnumerable<IViewModelProvider<ITreePageMenuItem>> items,
        IConfiguration cfg,
        ILogService log, IAppInfo info, ILoggerFactory loggerFactory, IApplicationHost? host = null) : base(WellKnownUri.ShellPageSettings)
    {
        _logger = loggerFactory.CreateLogger<SettingsPageViewModel>();
        _cfg = cfg;
        Title = RS.SettingsShellMenuProvider_SettingsShellMenuProvider_Settings;
        Icon = MaterialIconKind.Settings;
        CurrentVersion = info.Version;
        AppUrl = info.AppUrl;
        Author = info.Author;
        AppLicense = info.AppLicense;
        AppName = info.Name;
        CurrentAvaloniaVersion = info.AvaloniaVersion;

        Settings = new TreePageExplorerViewModel(items, this, log)
            .DisposeItWith(Disposable);
        Settings.Title = RS.SettingsShellMenuProvider_SettingsShellMenuProvider_Settings;
        Settings.Icon = Icon;
        Restart = ReactiveCommand
            .Create(() => host?.RestartApplication())
            .DisposeItWith(Disposable);
    }

    private async void LoadLastPage()
    {
        var lastPage = _cfg.Get<SettingsPageViewModelConfig>()?.LastPageUri;
        if (lastPage == null) return;
        try
        {
            _logger.ZLogDebug($"Load last settings page:{lastPage}");   
            await Settings.GoTo(new Uri(lastPage));
        }
        catch (Exception e)
        {
            _logger.ZLogError(e,$"Error to load last settings page:{e.Message}");
        }
    }

    public string Author { get; set; }
    public string CurrentVersion { get; }
    public string AppUrl { get; }
    public string AppName { get; }
    public string AppLicense { get; }
    public string CurrentAvaloniaVersion { get; }
    [Reactive] public bool IsRebootRequired { get; set; }
    public ReactiveCommand<Unit, Unit> Restart { get; }

    public void SetRebootRequired()
    {
        IsRebootRequired = true;
    }

    public override void SetArgs(NameValueCollection args)
    {
        var loadLastPageString = args[UriArgNameEmpty];
        // We need to check URI args. May be caller don't want load last page from config and navigate custom page
        // E.G. asv:shell.page.settings?empty=true  
        if (loadLastPageString == null || !bool.TryParse(loadLastPageString, out var loadEmpty) || !loadEmpty)
        {
            LoadLastPage();
        }

        base.SetArgs(args);
    }

    public override Task<bool> TryClose()
    {
        if (Settings.SelectedMenu != null)
        {
            var config = _cfg.Get<SettingsPageViewModelConfig>();
            config.LastPageUri = Settings.SelectedMenu?.Id.ToString();    
            _cfg.Set(config);
        }
        return base.TryClose();
    }
}