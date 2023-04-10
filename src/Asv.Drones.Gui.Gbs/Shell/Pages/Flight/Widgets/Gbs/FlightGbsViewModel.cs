using System.Collections.ObjectModel;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Windows.Input;
using Asv.Common;
using Asv.Drones.Gui.Core;
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
    public static Uri GenerateUri(IGbsDevice gbs) => FlightGbsWidgetBase.GenerateUri(gbs,"gbs");
    
    private Subject<bool> _canExecuteAutoCommand = new();
    private Subject<bool> _canExecuteFixedCommand = new();
    private Subject<bool> _canExecuteIdleCommand = new();
    private Subject<bool> _canExecuteCancelCommand = new();
    
    public FlightGbsViewModel(IGbsDevice gbsDevice, ILogService log, ILocalizationService loc, IEnumerable<IGbsRttItemProvider> rttItems)
        :base(gbsDevice, GenerateUri(gbsDevice))
    {
        _logService = log;
        _loc = loc;
        
        Icon = MaterialIconKind.RouterWireless;
        Title = RS.FlightGbsViewModel_Title;
        
        rttItems
            .SelectMany(_ => _.Create(gbsDevice))
            .OrderBy(_=>_.Order)
            .AsObservableChangeSet()
            .AutoRefresh(_=>_.IsVisible)
            .Filter(_=>_.IsVisible)
            .Bind(out _rttItems)
            .DisposeMany()
            .Subscribe()
            .DisposeItWith(Disposable);

        Gbs.DeviceClient.CustomMode.DistinctUntilChanged().Subscribe(SwitchMode).DisposeItWith(Disposable);
        
        EnableAutoCommand = ReactiveCommand.CreateFromTask(EnableAutoMode, _canExecuteAutoCommand).DisposeItWith(Disposable);
        EnableFixedCommand = ReactiveCommand.CreateFromTask(EnableFixedMode, _canExecuteFixedCommand).DisposeItWith(Disposable);
        EnableIdleCommand = ReactiveCommand.CreateFromTask(EnableIdleMode, _canExecuteIdleCommand).DisposeItWith(Disposable);
        CancelCommand = ReactiveCommand.CreateFromTask(EnableIdleMode, _canExecuteCancelCommand).DisposeItWith(Disposable);

        Gbs.DeviceClient.BeidouSatellites.Subscribe(_ => BeidouSats = new GridLength(_, GridUnitType.Star)).DisposeItWith(Disposable);
        Gbs.DeviceClient.GalSatellites.Subscribe(_ => GalSats = new GridLength(_, GridUnitType.Star)).DisposeItWith(Disposable);
        Gbs.DeviceClient.GlonassSatellites.Subscribe(_ => GlonassSats = new GridLength(_, GridUnitType.Star)).DisposeItWith(Disposable);
        Gbs.DeviceClient.GpsSatellites.Subscribe(_ => GpsSats = new GridLength(_, GridUnitType.Star)).DisposeItWith(Disposable);
        Gbs.DeviceClient.ImesSatellites.Subscribe(_ => ImesSats = new GridLength(_, GridUnitType.Star)).DisposeItWith(Disposable);
        Gbs.DeviceClient.QzssSatellites.Subscribe(_ => QzssSats = new GridLength(_, GridUnitType.Star)).DisposeItWith(Disposable);
        Gbs.DeviceClient.SbasSatellites.Subscribe(_ => SbasSats = new GridLength(_, GridUnitType.Star)).DisposeItWith(Disposable);
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

        var viewModel = new AutoModeViewModel(Gbs, _logService, _loc, ctx);
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

        var viewModel = new FixedModeViewModel(Gbs, _logService, _loc, ctx);
        viewModel.ApplyDialog(dialog);
        dialog.Content = viewModel;
        var result = await dialog.ShowAsync();
    }

    private async Task EnableIdleMode(CancellationToken ctx)
    {
        await Gbs.DeviceClient.StartIdleMode(ctx);
    }

    protected override void InternalAfterMapInit(IMap map)
    {
        base.InternalAfterMapInit(map);
        LocateBaseStationCommand = ReactiveCommand.Create(() =>
        {
            Map.Center = Gbs.DeviceClient.Position.Value;
        }).DisposeItWith(Disposable);
    }

    public ICommand LocateBaseStationCommand { get; set; }
    public ICommand EnableAutoCommand { get; set; }
    public ICommand EnableFixedCommand { get; set; }
    public ICommand EnableIdleCommand { get; set; }
    public ICommand CancelCommand { get; set; }
    
    public ReadOnlyObservableCollection<IGbsRttItem> RttItems => _rttItems;
    
    [Reactive]
    public bool IsProgressShown { get; set; }
    [Reactive]
    public bool IsDisableShown { get; set; }
    
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