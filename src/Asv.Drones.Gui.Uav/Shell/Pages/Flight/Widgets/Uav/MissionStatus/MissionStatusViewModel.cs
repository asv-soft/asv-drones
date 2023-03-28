using System.Collections.ObjectModel;
using System.Reactive;
using System.Reactive.Linq;
using System.Windows.Input;
using Asv.Common;
using Asv.Drones.Gui.Core;
using Asv.Mavlink;
using Asv.Mavlink.V2.Common;
using Avalonia.Controls;
using DynamicData;
using DynamicData.Aggregation;
using DynamicData.Binding;
using FluentAvalonia.Core;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Asv.Drones.Gui.Uav.Uav;

public class MissionStatusViewModel : ViewModelBase
{
    private readonly IVehicle _vehicle;
    private readonly ILogService _log; 
    private ReadOnlyObservableCollection<RoundWayPointItem> _wayPoints;

    public MissionStatusViewModel() : base(new Uri("designTime://missionstatus"))
    {
        
    }

    public MissionStatusViewModel(IVehicle vehicle, ILogService log, Uri id, ILocalizationService localization) : base(id)
    {
        _vehicle = vehicle;

        _vehicle.AllMissionsDistance.Subscribe(_ => Total = _ * 1000)
            .DisposeItWith(Disposable);
        
        _log = log;

        _vehicle.MissionCurrent.Subscribe(_ => CurrentIndex = _)
            .DisposeItWith(Disposable);

        _vehicle.MissionReached.Subscribe(_ => ReachedIndex = _)
            .DisposeItWith(Disposable);

        this.WhenValueChanged(_ => _.ReachedIndex, false)
            .Subscribe(_ => PathProgress = (double)ReachedIndex / _wayPoints.Count)
            .DisposeItWith(Disposable);
        
        _vehicle.MissionItems
            .ObserveOn(RxApp.MainThreadScheduler)
            .Filter(_ => _.Command.Value != MavCmd.MavCmdNavReturnToLaunch)
            .Transform(_ => new RoundWayPointItem(_))
            .Bind(out _wayPoints)
            .DisposeMany()
            .Subscribe()
            .DisposeItWith(Disposable);

        _download = ReactiveCommand.CreateFromObservable(
                () => Observable.FromAsync(DownloadImpl).SubscribeOn(RxApp.TaskpoolScheduler).TakeUntil(_cancelDownload), 
                this.WhenAnyValue(_ => _.IsInDownloadProgress).Select(_ => !_))
            .DisposeItWith(Disposable);
        
        _download.IsExecuting.ToProperty(this, _ => _.IsDownloading, out _isDownloading)
            .DisposeItWith(Disposable);
        
        _download.ThrownExceptions.Subscribe(OnDownloadError)
            .DisposeItWith(Disposable);
        
        _cancelDownload = ReactiveCommand.Create(() => { }, _download.IsExecuting)
            .DisposeItWith(Disposable);
        
        this.WhenAnyValue(_=>_.IsDownloading)
            .Subscribe(_=> IsInDownloadProgress = _)
            .DisposeItWith(Disposable);
    }
    
    #region Download
    private readonly ObservableAsPropertyHelper<bool> _isDownloading;
    
    public bool IsDownloading => _isDownloading.Value;
    
    public readonly ReactiveCommand<Unit,Unit> _download;
    
    public ICommand Download => _download;
    
    private readonly ReactiveCommand<Unit,Unit> _cancelDownload;
    
    public ICommand CancelDownload => _cancelDownload;

    private void OnDownloadError(Exception exception)
    {   
        //TODO: Localize
        _log.Error("MissionStatus", $"Download mission error {_vehicle.Name.Value}", exception);
    }

    private async Task DownloadImpl(CancellationToken cancel)
    {
        await _vehicle.DownloadMission(3, cancel,_ => DownloadProgress = _ * 100);
    }
    #endregion

    [Reactive]
    public bool IsInDownloadProgress { get; set; }
    
    [Reactive]
    public double DownloadProgress { get; set; }
    
    [Reactive]
    public bool DisableAll { get; set; }
    
    [Reactive]
    public bool EnablePolygons { get; set; }
    
    [Reactive]
    public bool EnablePolygonsAndAnchors { get; set; }
    
    [Reactive]
    public double Current { get; set; }
    
    [Reactive]
    public double Total { get; set; }

    [Reactive] 
    public double PathProgress { get; set; }
    
    [Reactive]
    public ushort CurrentIndex { get; set; }

    [Reactive]
    public ushort ReachedIndex { get; set; }
    
    public ReadOnlyObservableCollection<RoundWayPointItem> WayPoints => _wayPoints;
}