using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Reactive;
using System.Reactive.Linq;
using Asv.Common;
using Asv.Drones.Gui.Core;
using Asv.Mavlink;
using Asv.Mavlink.V2.Common;
using Asv.Mavlink.Vehicle;
using DynamicData;
using DynamicData.Aggregation;
using DynamicData.Alias;
using DynamicData.Binding;
using DynamicData.Kernel;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Asv.Drones.Gui.Uav.MissionStatus;

public class MissionStatusViewModel : ViewModelBase
{
    private readonly IVehicleClient _vehicle;
    private readonly ILogService _log;
    private ReadOnlyObservableCollection<MissionItem> _items;

    //private ReadOnlyObservableCollection<RoundWayPointItem> _wayPoints;

    public MissionStatusViewModel() : base(new Uri("designTime://missionstatus"))
    {
        
    }

    public MissionStatusViewModel(IVehicleClient vehicle, ILogService log, Uri id, ILocalizationService localization) : base(id)
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
        
        _vehicle.Missions.MissionItems
            .Bind(out _items)
            .Subscribe()
            .DisposeItWith(Disposable);
        
        _vehicle.Missions.AllMissionsDistance.Subscribe(_ =>
            {
                var totalDistance = _ * 1000;
                MissionDistance = localization.Distance.FromSiToStringWithUnits(totalDistance);
                var start = _items.FirstOrDefault();
                var stop = _items.LastOrDefault(_ => _.Command.Value != MavCmd.MavCmdNavReturnToLaunch);
                if (start != null && stop != null)
                {
                    totalDistance += GeoMath.Distance(start.Location.Value, _vehicle.Position.Home.Value);
                    totalDistance += GeoMath.Distance(stop.Location.Value, _vehicle.Position.Home.Value);
                }
                TotalDistance = localization.Distance.FromSiToStringWithUnits(totalDistance);
            })
            .DisposeItWith(Disposable);

        _vehicle.Missions.Current.Subscribe(_ => CurrentIndex = _)
            .DisposeItWith(Disposable);

        _vehicle.Missions.Reached.Subscribe(_ => ReachedIndex = _)
            .DisposeItWith(Disposable);

        _vehicle.Missions.MissionItems.Filter(_ => _.Command.Value != MavCmd.MavCmdNavReturnToLaunch)
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
        try
        {
            await _vehicle.Missions.Download(cancel, _ => DownloadProgress = _ * 100);
        }
        catch (Exception e)
        {
            _log.Error("MissionStatus", e.Message);
        }
    }
    #endregion

    [Reactive]
    public double DownloadProgress { get; set; }
    
    [Reactive] 
    public bool EnablePolygon { get; set; } = true;
    
    [Reactive]
    public bool EnableAnchors { get; set; } = true;
    
    [Reactive]
    public double Current { get; set; }

    [Reactive] 
    public string MissionDistance { get; set; } = RS.UavRttItem_ValueNotAvailable;

    [Reactive] 
    public string TotalDistance { get; set; } = RS.UavRttItem_ValueNotAvailable;

    
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