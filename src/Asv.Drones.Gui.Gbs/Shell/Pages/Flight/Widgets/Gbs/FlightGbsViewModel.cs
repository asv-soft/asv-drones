using System.Collections.ObjectModel;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Windows.Input;
using Asv.Cfg;
using Asv.Common;
using Asv.Drones.Gui.Core;
using Asv.Mavlink;
using Asv.Mavlink.V2.AsvGbs;
using Avalonia.Controls;
using Avalonia.Styling;
using DynamicData;
using FluentAvalonia.UI.Controls;
using Material.Icons;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Asv.Drones.Gui.Gbs;

public class FlightGbsViewModel:FlightGbsWidgetBase
{
    private readonly ReadOnlyObservableCollection<IGbsRttItem> _rttItems;
    private readonly ILogService _logService;
    private readonly ILocalizationService _loc;
    private readonly IConfiguration _configuration;
    public static Uri GenerateUri(IGbsClientDevice gbs) => FlightGbsWidgetBase.GenerateUri(gbs,"gbs");
    
    private Subject<bool> _canExecuteAutoCommand = new();
    private Subject<bool> _canExecuteFixedCommand = new();
    private Subject<bool> _canExecuteIdleCommand = new();
    private Subject<bool> _canExecuteCancelCommand = new();
    
    public FlightGbsViewModel(IGbsClientDevice baseStationDevice, ILogService log, ILocalizationService loc, IConfiguration configuration, IEnumerable<IGbsRttItemProvider> rttItems)
        :base(baseStationDevice, GenerateUri(baseStationDevice))
    {
        _logService = log;
        _loc = loc;
        _configuration = configuration;
        
        Icon = MaterialIconKind.RouterWireless;
        Title = RS.FlightGbsViewModel_Title;
        
        rttItems
            .SelectMany(_ => _.Create(baseStationDevice))
            .OrderBy(_=>_.Order)
            .AsObservableChangeSet()
            .AutoRefresh(_ => _.IsVisible)
            .Filter(_ => _.IsVisible)
            .Bind(out _rttItems)
            .DisposeMany()
            .Subscribe()
            .DisposeItWith(Disposable);

        MinimizedRttItems = _rttItems.Where(_ => _.IsMinimizedVisible);

        BaseStation.Gbs.CustomMode.DistinctUntilChanged().Subscribe(SwitchMode).DisposeItWith(Disposable);
        
        EnableAutoCommand = ReactiveCommand.CreateFromTask(EnableAutoMode, _canExecuteAutoCommand).DisposeItWith(Disposable);
        EnableFixedCommand = ReactiveCommand.CreateFromTask(EnableFixedMode, _canExecuteFixedCommand).DisposeItWith(Disposable);
        EnableIdleCommand = ReactiveCommand.CreateFromTask(EnableIdleMode, _canExecuteIdleCommand).DisposeItWith(Disposable);
        CancelCommand = ReactiveCommand.CreateFromTask(EnableIdleMode, _canExecuteCancelCommand).DisposeItWith(Disposable);
        ChangeStateCommand = ReactiveCommand.Create(() =>
        {
            IsMinimized = !IsMinimized;
        });

        BaseStation.Gbs.BeidouSatellites.Subscribe(_ => BeidouSats = new GridLength(_, GridUnitType.Star)).DisposeItWith(Disposable);
        BaseStation.Gbs.GalSatellites.Subscribe(_ => GalSats = new GridLength(_, GridUnitType.Star)).DisposeItWith(Disposable);
        BaseStation.Gbs.GlonassSatellites.Subscribe(_ => GlonassSats = new GridLength(_, GridUnitType.Star)).DisposeItWith(Disposable);
        BaseStation.Gbs.GpsSatellites.Subscribe(_ => GpsSats = new GridLength(_, GridUnitType.Star)).DisposeItWith(Disposable);
        BaseStation.Gbs.ImesSatellites.Subscribe(_ => ImesSats = new GridLength(_, GridUnitType.Star)).DisposeItWith(Disposable);
        BaseStation.Gbs.QzssSatellites.Subscribe(_ => QzssSats = new GridLength(_, GridUnitType.Star)).DisposeItWith(Disposable);
        BaseStation.Gbs.SbasSatellites.Subscribe(_ => SbasSats = new GridLength(_, GridUnitType.Star)).DisposeItWith(Disposable);
    }

    private void SwitchMode(AsvGbsCustomMode mode)
    {
        IsProgressShown = false;
        IsDisableShown = false;
        
        switch (mode)
        {
            case AsvGbsCustomMode.AsvGbsCustomModeLoading:
                _canExecuteAutoCommand.OnNext(false);
                _canExecuteFixedCommand.OnNext(false);
                _canExecuteIdleCommand.OnNext(false);
                _canExecuteCancelCommand.OnNext(false);
                
                IsProgressShown = true;
                break;
            case AsvGbsCustomMode.AsvGbsCustomModeIdle:
                _canExecuteAutoCommand.OnNext(true);
                _canExecuteFixedCommand.OnNext(true);
                _canExecuteIdleCommand.OnNext(false);
                _canExecuteCancelCommand.OnNext(false);
                break;
            case AsvGbsCustomMode.AsvGbsCustomModeError:
                //TODO: Implement error state
                break;
            case AsvGbsCustomMode.AsvGbsCustomModeAutoInProgress:
                _canExecuteAutoCommand.OnNext(false);
                _canExecuteFixedCommand.OnNext(false);
                _canExecuteIdleCommand.OnNext(false);
                _canExecuteCancelCommand.OnNext(true);
                
                IsProgressShown = true;
                break;
            case AsvGbsCustomMode.AsvGbsCustomModeAuto:
                _canExecuteAutoCommand.OnNext(false);
                _canExecuteFixedCommand.OnNext(false);
                _canExecuteIdleCommand.OnNext(true);
                _canExecuteCancelCommand.OnNext(false);

                IsDisableShown = true;
                break;
            case AsvGbsCustomMode.AsvGbsCustomModeFixedInProgress:
                _canExecuteAutoCommand.OnNext(false);
                _canExecuteFixedCommand.OnNext(false);
                _canExecuteIdleCommand.OnNext(false);
                _canExecuteCancelCommand.OnNext(true);
                
                IsProgressShown = true;
                break;
            case AsvGbsCustomMode.AsvGbsCustomModeFixed:
                _canExecuteAutoCommand.OnNext(false);
                _canExecuteFixedCommand.OnNext(false);
                _canExecuteIdleCommand.OnNext(true);
                _canExecuteCancelCommand.OnNext(false);
                
                IsDisableShown = true;
                break;
        }
    }

    private async Task EnableAutoMode(CancellationToken ctx)
    {
        var dialog = new ContentDialog()
        {
            Title = RS.FlightGbsViewModel_AutoMode_Title,
            PrimaryButtonText = RS.FlightGbsViewModel_AutoMode_PrimaryButtonText,
            IsSecondaryButtonEnabled = true,
            CloseButtonText = RS.FlightGbsViewModel_AutoMode_CloseButtonText
        };

        var viewModel = new AutoModeViewModel(BaseStation, _logService, _loc, _configuration, ctx);
        viewModel.ApplyDialog(dialog);
        dialog.Content = viewModel;
        var result = await dialog.ShowAsync();
    }

    private async Task EnableFixedMode(CancellationToken ctx)
    {
        var dialog = new ContentDialog()
        {
            Title = RS.FlightGbsViewModel_FixedMode_Title,
            PrimaryButtonText = RS.FlightGbsViewModel_FixedMode_PrimaryButtonText,
            IsSecondaryButtonEnabled = true,
            CloseButtonText = RS.FlightGbsViewModel_FixedMode_CloseButtonText
        };

        var viewModel = new FixedModeViewModel(BaseStation, _logService, _loc, _configuration, ctx);
        viewModel.ApplyDialog(dialog);
        dialog.Content = viewModel;
        var result = await dialog.ShowAsync();
    }

    private async Task EnableIdleMode(CancellationToken ctx)
    {
        await BaseStation.Gbs.StartIdleMode(ctx);
    }

    protected override void InternalAfterMapInit(IMap map)
    {
        base.InternalAfterMapInit(map);
        LocateBaseStationCommand = ReactiveCommand.Create(() =>
        {
            Map.Center = BaseStation.Gbs.Position.Value;
            var selectedGbs = Map.Markers.Where(_=>_ is GbsAnchor).Cast<GbsAnchor>().FirstOrDefault(_=>_.Device.FullId == BaseStation.FullId);
            if (selectedGbs != null)
            {
                selectedGbs.IsSelected = true;
            }

        }).DisposeItWith(Disposable);
    }

    public ICommand LocateBaseStationCommand { get; set; }
    public ICommand EnableAutoCommand { get; set; }
    public ICommand EnableFixedCommand { get; set; }
    public ICommand EnableIdleCommand { get; set; }
    public ICommand CancelCommand { get; set; }
    public ICommand ChangeStateCommand { get; set; }
    
    public ReadOnlyObservableCollection<IGbsRttItem> RttItems => _rttItems;
    public IEnumerable<IGbsRttItem> MinimizedRttItems { get; set; }

    [Reactive]
    public bool IsProgressShown { get; set; }
    [Reactive]
    public bool IsDisableShown { get; set; }
    [Reactive] 
    public bool IsMinimized { get; set; } = false;
    
    [Reactive]
    public GridLength BeidouSats { get; set; }
    [Reactive]
    public GridLength GalSats { get; set; }
    [Reactive]
    public GridLength GlonassSats { get; set; }
    [Reactive]
    public GridLength GpsSats { get; set; }
    [Reactive]
    public GridLength ImesSats { get; set; }
    [Reactive]
    public GridLength QzssSats { get; set; }
    [Reactive]
    public GridLength SbasSats { get; set; }
   
}