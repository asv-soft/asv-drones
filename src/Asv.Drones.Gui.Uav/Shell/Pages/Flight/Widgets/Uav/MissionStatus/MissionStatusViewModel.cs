using System.Reactive;
using Asv.Common;
using Asv.Drones.Gui.Core;
using Asv.Mavlink;
using Asv.Mavlink.V2.Common;
using DynamicData;
using DynamicData.Aggregation;
using DynamicData.Binding;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Asv.Drones.Gui.Uav.MissionStatus;

public class MissionStatusViewModel : ViewModelBase
{
    private readonly IVehicle _vehicle;
    private readonly ILogService _log; 
    //private ReadOnlyObservableCollection<RoundWayPointItem> _wayPoints;

    public MissionStatusViewModel() : base(new Uri("designTime://missionstatus"))
    {
        
    }

    public MissionStatusViewModel(IVehicle vehicle, ILogService log, Uri id, ILocalizationService localization) : base(id)
    {
        _vehicle = vehicle;

        _log = log;

        Download = ReactiveCommand.CreateFromTask(DownloadImpl)
            .DisposeItWith(Disposable);

        DisableAll = ReactiveCommand.Create(() =>
        {
            EnablePolygon = false;
            EnableAnchors = false;
        }).DisposeItWith(Disposable);
        
        _vehicle.AllMissionsDistance.Subscribe(_ => Total = _ * 1000)
            .DisposeItWith(Disposable);

        _vehicle.MissionCurrent.Subscribe(_ => CurrentIndex = _)
            .DisposeItWith(Disposable);

        _vehicle.MissionReached.Subscribe(_ => ReachedIndex = _)
            .DisposeItWith(Disposable);

        _vehicle.MissionItems.Filter(_ => _.Command.Value != MavCmd.MavCmdNavReturnToLaunch)
            .Count()
            .Subscribe(_ => WayPointsCount = _)
            .DisposeItWith(Disposable);
        
        //_vehicle.MissionItems
        //    .Filter(_ => _.Command.Value != MavCmd.MavCmdNavReturnToLaunch)
        //    .Transform(_ => new RoundWayPointItem(_))
        //    .Bind(out _wayPoints)
        //    .DisposeMany()
        //    .Subscribe()
        //    .DisposeItWith(Disposable);
        
        this.WhenValueChanged(_ => _.ReachedIndex, false)
            .Subscribe(_ => PathProgress = (double)ReachedIndex / WayPointsCount)
            .DisposeItWith(Disposable);
    }
    
    public ReactiveCommand<Unit, Unit> DisableAll { get; set; }
    
    #region Download
    public ReactiveCommand<Unit, Unit> Download { get; set; }

    private async Task DownloadImpl(CancellationToken cancel)
    {
        await _vehicle.DownloadMission(3, cancel,_ => DownloadProgress = _ * 100);
    }
    #endregion

    [Reactive]
    public double DownloadProgress { get; set; }
    
    [Reactive] public bool EnablePolygon { get; set; } = true;
    
    [Reactive]
    public bool EnableAnchors { get; set; } = true;
    
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
    
    [Reactive]
    public int WayPointsCount { get; set; }
    
    //public ReadOnlyObservableCollection<RoundWayPointItem> WayPoints => _wayPoints;
}