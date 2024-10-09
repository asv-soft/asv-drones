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
using DynamicData.Binding;
using Microsoft.Extensions.Logging;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using ZLogger;

namespace Asv.Drones.Gui;

public class MissionStatusViewModel : ViewModelBase
{
    private readonly IVehicleClient _vehicle;
    private readonly ILogger _log;
    private readonly ReadOnlyObservableCollection<MissionItem> _items;
    private double _passedDistance;
    private double _distanceBeforeMission;
    private bool _isOnMission = false;

    public MissionStatusViewModel() : base(WellKnownUri.UndefinedUri)
    {
        DesignTime.ThrowIfNotDesignMode();
    }

    public MissionStatusViewModel(IVehicleClient vehicle, ILoggerFactory log, Uri id, ILocalizationService localization) :
        base(id)
    {
        _vehicle = vehicle;

        _log = log.CreateLogger<MissionStatusViewModel>();

        _vehicle.CurrentMode.Subscribe(m =>
        {
            if (m == ArdupilotCopterMode.Auto || m == ArdupilotPlaneMode.Auto)
            {
                if (_isOnMission)
                {
                    return;
                }
                
                Task.Run(async () => await InitiateMissionPoints(new CancellationToken())); // call before anything else
                _isOnMission = true;
            }
            else
            {
                _isOnMission = false;
                ReachedIndex = 0;
            }
        });

        DownloadMissions = ReactiveCommand.CreateFromTask(DownloadMissionsImpl)
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

        this.WhenAnyValue(p => p.PathProgress).Subscribe(v =>
        {
            if (v > 1)
            {
                PathProgress = 1;
                return;
            }

            if (v < 0)
            {
                PathProgress = 0;
                return;
            }

            PathProgress = v;
        });

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
    }

    private async Task InitiateMissionPoints(CancellationToken cancel)
    {
        await DownloadMissionsImpl(cancel);
    }
    
    private void CalculateMissionProgress()
    {
        var toTargetDistance = GeoMath.Distance(_vehicle.Position.Target.Value, _vehicle.Position.Current.Value);
        var missionDistance = _vehicle.Missions.AllMissionsDistance.Value * 1000;
        var distance = Math.Abs(missionDistance - _passedDistance + toTargetDistance);
        var time = distance / _vehicle.Gnss.Main.GroundVelocity.Value;

        PathProgress = CalculatePathProgressValue(missionDistance, distance);
        MissionFlightTime = CalculateMissionFlightTime(time);
    }

    private static string CalculateMissionFlightTime(double time)
    {
        if (time is double.NaN or double.PositiveInfinity)
        {
            return $"- {RS.MissionStatusViewModel_FlightMissionTime_Minutes}";
        }
        var minute = Math.Round(time / 60);
        
        if (minute < 1)
        {
            return $"<1 {RS.MissionStatusViewModel_FlightMissionTime_Minutes}";
        }
        
        return $"{minute} {RS.MissionStatusViewModel_FlightMissionTime_Minutes}";
    }

    private double CalculatePathProgressValue(double missionDistance, double distance) // TODO: сделать расширение логики для самолетов
    {
        switch (_vehicle)
        {
            case ArduVehicle:
                if (_isOnMission && ReachedIndex > 0)
                {
                    return Math.Abs((missionDistance - distance - _distanceBeforeMission) / (missionDistance - _distanceBeforeMission));
                }
                
                return 0;
            default:
                return Math.Abs((missionDistance - distance) / missionDistance);
        }
    }

    public ReactiveCommand<Unit, Unit> DisableAll { get; set; }

    #region Download

    public ReactiveCommand<Unit, Unit> DownloadMissions { get; set; }

    private async Task DownloadMissionsImpl(CancellationToken cancel)
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
                    _log.ZLogWarning($"MissionStatus {string.Format(RS.MissionStatusViewModel_PointLowerThanHome_Warning, i,
                        item.Location.Value.Altitude)}");
                }
            }
        }
        catch (Exception e)
        {
            _log.ZLogError($"MissionStatus {e.Message}");
        }
    }

    #endregion

    [Reactive] public string MissionFlightTime { get; set; } = $"- {RS.MissionStatusViewModel_FlightMissionTime_Minutes}";

    [Reactive] public double DownloadProgress { get; set; }

    [Reactive] public bool EnablePolygon { get; set; } = true;

    [Reactive] public bool EnableAnchors { get; set; } = true;

    [Reactive] public string MissionDistance { get; set; } = RS.UavRttItem_ValueNotAvailable;

    [Reactive] public string TotalDistance { get; set; } = RS.UavRttItem_ValueNotAvailable;

    /// <summary>
    /// Represents progress of the mission.
    /// Changes from 0 to 1
    /// </summary>
    [Reactive] public double PathProgress { get; set; }
    
    [Reactive] public ushort CurrentIndex { get; set; }

    [Reactive] public ushort ReachedIndex { get; set; }
}