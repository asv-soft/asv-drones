using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Asv.Common;
using Asv.Drones.Gui.Api;
using Asv.Mavlink;
using Asv.Mavlink.V2.Common;
using Asv.Mavlink.Vehicle;
using DynamicData;
using DynamicData.Aggregation;
using DynamicData.Binding;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Asv.Drones.Gui;

public class MissionStatusViewModel : ViewModelBase
{
    private readonly IVehicleClient _vehicle;
    private readonly ILogService _log;
    private readonly ReadOnlyObservableCollection<MissionItem> _items;
    private double _passedDistance;
    private double _distanceBeforeMission;

    //private ReadOnlyObservableCollection<RoundWayPointItem> _wayPoints;

    public MissionStatusViewModel() : base(WellKnownUri.UndefinedUri)
    {
        DesignTime.ThrowIfNotDesignMode();
    }

    public MissionStatusViewModel(IVehicleClient vehicle, ILogService log, Uri id, ILocalizationService localization) :
        base(id)
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

        Observable.Timer(TimeSpan.Zero, TimeSpan.FromSeconds(1))
                  .Subscribe(_ => CalculateMissionProgress())
                  .DisposeItWith(Disposable);

        this.WhenValueChanged(vm => vm.CurrentIndex)
            .Subscribe(index =>
            {
                _passedDistance = 0;
                var items = _items.Where(item => item.Index <= index).ToList();
                if (items.Count < 2) return;
                for (var i = 1; i < items.Count; i++)
                {
                    _passedDistance += GeoMath.Distance(items[i - 1].Location.Value, items[i].Location.Value);
                }
                _distanceBeforeMission = GeoMath.Distance(items[0].Location.Value, items[1].Location.Value);
            }).DisposeItWith(Disposable);

        _vehicle.Gnss.Main.GroundVelocity
                .Select(localization.Velocity.ConvertFromSi)
                .Select(_ => Math.Round(_))
                .DistinctUntilChanged()
                .Subscribe()
                .DisposeItWith(Disposable);

        _vehicle.Missions.AllMissionsDistance.Subscribe(d =>
                {
                    var totalDistance = d * 1000;
                    MissionDistance =
                        localization.Distance.FromSiToStringWithUnits(
                            totalDistance);
                    var start = _items.FirstOrDefault();
                    var stop = _items.LastOrDefault(
                        _ => _.Command.Value != MavCmd.MavCmdNavReturnToLaunch);
                    if (start != null && stop != null)
                    {
                        totalDistance +=
                            GeoMath.Distance(
                                start.Location.Value,
                                _vehicle.Position.Home.Value);
                        totalDistance +=
                            GeoMath.Distance(
                                stop.Location.Value,
                                _vehicle.Position.Home.Value);
                    }

                    TotalDistance =
                        localization.Distance.FromSiToStringWithUnits(
                            totalDistance);
                })
                .DisposeItWith(Disposable);

        _vehicle.Missions.Current.Subscribe(i => CurrentIndex = i)
                .DisposeItWith(Disposable);

        _vehicle.Missions.Reached.Subscribe(i => ReachedIndex = i)
                .DisposeItWith(Disposable);

        _vehicle.Missions.MissionItems.Filter(item => item.Command.Value != MavCmd.MavCmdNavReturnToLaunch)
                .Count()
                .Subscribe(count => WayPointsCount = count)
                .DisposeItWith(Disposable);

        //_vehicle.MissionItems
        //    .Filter(_ => _.Command.Value != MavCmd.MavCmdNavReturnToLaunch)
        //    .Transform(_ => new RoundWayPointItem(_))
        //    .Bind(out _wayPoints)
        //    .DisposeMany()
        //    .Subscribe()
        //    .DisposeItWith(Disposable);
    }

    private void CalculateMissionProgress()
    {
        var toTargetDistance = GeoMath.Distance(_vehicle.Position.Target.Value, _vehicle.Position.Current.Value);
        var missionDistance = _vehicle.Missions.AllMissionsDistance.Value * 1000;
        var distance = Math.Abs(missionDistance - _passedDistance + toTargetDistance);
        if (_vehicle is ArduVehicle)
        {
            if (ReachedIndex >= 1)
                PathProgress = Math.Abs((missionDistance - distance - _distanceBeforeMission) / (missionDistance - _distanceBeforeMission));
        }
        else
        {
            PathProgress = Math.Abs((missionDistance - distance) / missionDistance);
        }

        var time = distance / _vehicle.Gnss.Main.GroundVelocity.Value;
        if (time is double.NaN or double.PositiveInfinity)
        {
            MissionFlightTime = $"- {RS.MissionStatusViewModel_FlightMissionTime_Minutes}";
            return;
        }
        var minute = Math.Round(time / 60);
        if (minute < 1)
        {
            MissionFlightTime = $"<1 {RS.MissionStatusViewModel_FlightMissionTime_Minutes}";
            return;
        }
        MissionFlightTime = $"{minute} {RS.MissionStatusViewModel_FlightMissionTime_Minutes}";
    }

    public ReactiveCommand<Unit, Unit> DisableAll { get; set; }

    #region Download

    public ReactiveCommand<Unit, Unit> Download { get; set; }

    private async Task DownloadImpl(CancellationToken cancel)
    {
        try
        {
            await _vehicle.Missions.Download(cancel, p => DownloadProgress = p * 100);

            double homeAlt = 0;

            if (_vehicle.Position.Home.Value != null)
            {
                homeAlt = _vehicle.Position.Home.Value.Value.Altitude;
            }

            for (var i = 0; i < _items.Count; i++)
            {
                if (i == 0 && _vehicle is ArduVehicle) continue;

                var item = _items[i];

                if (item.Command.Value == MavCmd.MavCmdNavWaypoint && item.Location.Value.Altitude <= homeAlt)
                {
                    _log.Warning("MissionStatus",
                                 string.Format(RS.MissionStatusViewModel_PointLowerThanHome_Warning, i,
                                               item.Location.Value.Altitude));
                }
            }
        }
        catch (Exception e)
        {
            _log.Error("MissionStatus", e.Message);
        }
    }

    #endregion

    [Reactive] public string MissionFlightTime { get; set; } = $"- {RS.MissionStatusViewModel_FlightMissionTime_Minutes}";

    [Reactive] public double DownloadProgress { get; set; }

    [Reactive] public bool EnablePolygon { get; set; } = true;

    [Reactive] public bool EnableAnchors { get; set; } = true;

    [Reactive] public double Current { get; set; }

    [Reactive] public string MissionDistance { get; set; } = RS.UavRttItem_ValueNotAvailable;

    [Reactive] public string TotalDistance { get; set; } = RS.UavRttItem_ValueNotAvailable;

    [Reactive] public double PathProgress { get; set; }

    [Reactive] public ushort CurrentIndex { get; set; }

    [Reactive] public ushort ReachedIndex { get; set; }

    [Reactive] public int WayPointsCount { get; set; }

    //public ReadOnlyObservableCollection<RoundWayPointItem> WayPoints => _wayPoints;
}