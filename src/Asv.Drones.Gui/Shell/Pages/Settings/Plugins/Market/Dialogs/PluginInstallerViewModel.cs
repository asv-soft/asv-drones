using System;
using System.Composition;
using System.Reactive;
using System.Threading;
using System.Threading.Tasks;
using Asv.Cfg;
using Asv.Common;
using Asv.Drones.Gui.Api;
using DynamicData.Binding;
using FluentAvalonia.UI.Controls;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Asv.Drones.Gui;

public class PluginInstallerViewModelConfig
{
    public string NugetPackageFilePath { get; set; }
}

public class PluginInstallerViewModel : ViewModelBaseWithValidation
{
    private readonly IConfiguration _cfg;
    private readonly ILogService _log;
    private readonly PluginInstallerViewModelConfig _config;
    private readonly IPluginManager _manager;
    
    private static readonly string UriString = $"{WellKnownUri
        .ShellPageSettingsPluginsMarketUri}.install-manually";
    private static readonly Uri Uri = new(UriString);

    public PluginInstallerViewModel()
        : base(Uri) { }

    [ImportingConstructor]
    public PluginInstallerViewModel(
        IConfiguration cfg,
        ILogService log,
        IPluginManager manager
    )
        : this()
    {
        _cfg = cfg;
        _log = log;
        _manager = manager;
        _config = _cfg.Get<PluginInstallerViewModelConfig>();
        NugetPackageFilePath = _config.NugetPackageFilePath;
        
        this.WhenPropertyChanged(vm => vm.NugetPackageFilePath, false)
            .Subscribe(_ =>
            {
                _config.NugetPackageFilePath = NugetPackageFilePath;
                _cfg.Set(_config);
            })
            .DisposeItWith(Disposable);
    }
    
    [Reactive]
    public string NugetPackageFilePath { get; set; }

    private async Task<Unit> InstallPluginAsync(IProgress<double> progress, CancellationToken cancel)
    {
        try
        {
            await _manager.InstallManually(NugetPackageFilePath,
                new Progress<ProgressMessage>(
                    m => { progress.Report(m.Progress); }), cancel);
            _log.Info(nameof(PluginManager), RS.PluginInstallerViewModel_InstallPluginAsync_Success);
        }
        catch (Exception e)
        {
            _log.Error(nameof(PluginManager), e.Message);
        }
        
        return Unit.Default;
    }
    
    public void ApplyDialog(ContentDialog dialog)
    {
        ArgumentNullException.ThrowIfNull(dialog);

        dialog.PrimaryButtonCommand = ReactiveCommand.CreateFromTask<IProgress<double>, Unit>(InstallPluginAsync)
            .DisposeItWith(Disposable);
    }
}
