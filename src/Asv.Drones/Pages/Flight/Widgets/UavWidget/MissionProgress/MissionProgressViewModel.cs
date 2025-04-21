using System;
using System.Collections.Generic;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Asv.Avalonia;
using Asv.Common;
using Asv.IO;
using Asv.Mavlink;
using Asv.Mavlink.Common;
using Microsoft.Extensions.Logging;
using ObservableCollections;
using R3;

namespace Asv.Drones;

public class MissionProgressViewModel : RoutableViewModel
{
    private readonly IClientDevice _device;
    private readonly PositionClientEx _positionClient;
    private readonly MissionClientEx _missionClient;
    private readonly GnssClientEx _gnssClientEx;
    private readonly CancellationTokenSource _cts = new();
    private ReactiveProperty<ushort> CurrentIndex { get; } = new();
    private ReactiveProperty<ushort> ReachedIndex { get; } = new();
    private double _totalMissionDistance;
    private double _passedDistance;
    private double _distanceBeforeMission;
    private bool _isOnMission;

    public MissionProgressViewModel()
        : base(SystemModule.Name)
    {
        IsDownloaded.Value = true;
        MissionDistance.Value = "1000";
        MissionFlightTime.Value = "15 min";
        TotalDistance.Value = "1200";
        TargetDistance.Value = "300";
        HomeDistance.Value = "100";
        PathProgress.Value = 0.7;
    }

    [ImportingConstructor]
    public MissionProgressViewModel(
        string id,
        IClientDevice device,
        IUnitService unitService,
        ILoggerFactory loggerFactory
    )
        : base($"{id}.mission.progress")
    {
        ArgumentNullException.ThrowIfNull(device);
        ArgumentNullException.ThrowIfNull(unitService);
        ArgumentNullException.ThrowIfNull(loggerFactory);

        ILogger log = loggerFactory.CreateLogger<MissionProgressViewModel>();
        _device = device;
        DistanceUnitItem.Value = unitService.Units.Values.First(unit =>
            unit.UnitId == DistanceBase.Id
        );
        _missionClient =
            device.GetMicroservice<MissionClientEx>()
            ?? throw new Exception($"Unable to load {nameof(MissionClientEx)} from {device.Id}");
        _gnssClientEx =
            device.GetMicroservice<GnssClientEx>()
            ?? throw new Exception($"Unable to load {nameof(GnssClientEx)} from {device.Id}");
        _positionClient =
            device.GetMicroservice<PositionClientEx>()
            ?? throw new Exception($"Unable to load {nameof(PositionClientEx)} from {device.Id}");
        var mode =
            device.GetMicroservice<ModeClient>()
            ?? throw new Exception($"Unable to load {nameof(ModeClient)} from {device.Id}");
        UpdateMission = new BindableAsyncCommand(UpdateMissionCommand.Id, this);

        mode.CurrentMode.Subscribe(m =>
            {
                if (m == ArduCopterMode.Auto || m == ArduPlaneMode.Auto)
                {
                    if (_isOnMission)
                    {
                        return;
                    }

                    _isOnMission = true;
                    return;
                }

                if (m == ArduCopterMode.Rtl || m == ArduPlaneMode.Rtl)
                {
                    _isOnMission = false;
                    ReachedIndex.Value = 0;
                    PathProgress.Value = 0;
                }
            })
            .DisposeItWith(Disposable);
        PathProgress
            .Subscribe(p =>
            {
                switch (p)
                {
                    case > 1:
                        PathProgress.Value = 1;
                        return;
                    case < 0:
                        PathProgress.Value = 0;
                        return;
                    default:
                        PathProgress.Value = p;
                        break;
                }
            })
            .DisposeItWith(Disposable);
        _positionClient
            .Current.Subscribe(_ =>
            {
                if (_missionClient.MissionItems.Count == 0)
                {
                    return;
                }

                _totalMissionDistance = _missionClient.AllMissionsDistance.CurrentValue * 1000;
            })
            .DisposeItWith(Disposable);
        _positionClient
            .HomeDistance.Subscribe(d =>
                HomeDistance.Value = DistanceUnitItem.Value.Current.Value.Print(d, "N2")
            )
            .DisposeItWith(Disposable);
        _positionClient
            .TargetDistance.Subscribe(d =>
            {
                TargetDistance.Value = d is double.NaN
                    ? RS.Not_Available
                    : DistanceUnitItem.Value.Current.Value.Print(d, "N2");
            })
            .DisposeItWith(Disposable);
        _missionClient
            .AllMissionsDistance.Subscribe(d =>
            {
                var missionDistance = d * 1000;
                var totalDistance = missionDistance;
                MissionDistance.Value = DistanceUnitItem.Value.Current.Value.Print(
                    totalDistance,
                    "N2"
                );
                var start = _missionClient.MissionItems.FirstOrDefault();
                var stop = _missionClient.MissionItems.LastOrDefault(missionItem =>
                    missionItem.Command.Value != MavCmd.MavCmdNavReturnToLaunch
                );
                if (start != null && stop != null)
                {
                    totalDistance += GeoMath.Distance(
                        start.Location.Value,
                        _positionClient.Home.CurrentValue
                    );
                    totalDistance += GeoMath.Distance(
                        stop.Location.Value,
                        _positionClient.Home.CurrentValue
                    );
                }

                if (totalDistance < 1)
                {
                    totalDistance = missionDistance;
                }

                TotalDistance.Value = DistanceUnitItem.Value.Current.Value.Print(
                    totalDistance,
                    "N2"
                );
            })
            .DisposeItWith(Disposable);

        CurrentIndex
            .Subscribe(c =>
            {
                if (_missionClient.MissionItems.Count == 0)
                {
                    return;
                }

                _passedDistance = 0;
                var items = _missionClient
                    .MissionItems.Where(item =>
                        item.Index <= c && item.Command.Value != MavCmd.MavCmdDoChangeSpeed
                    )
                    .ToList();
                if (items.Count < 2)
                {
                    return;
                }

                for (var i = 1; i < items.Count; i++)
                {
                    _passedDistance += GeoMath.Distance(
                        items[i - 1].Location.Value,
                        items[i].Location.Value
                    );
                }
            })
            .DisposeItWith(Disposable);

        _missionClient.Reached.Subscribe(i => ReachedIndex.Value = i).DisposeItWith(Disposable);
        _missionClient.Current.Subscribe(i => CurrentIndex.Value = i).DisposeItWith(Disposable);
        InitiateMissionPoints(_cts.Token)
            .SafeFireAndForget(ex => log.LogError(ex, "Mission progress error"));

        _distanceBeforeMission =
            _missionClient.MissionItems.Count == 0
                ? 0
                : GeoMath.Distance(
                    _positionClient.Current.CurrentValue,
                    _missionClient.MissionItems[0].Location.Value
                );
        TotalDistance.Value = DistanceUnitItem.Value.Current.Value.Print(
            _totalMissionDistance + (_distanceBeforeMission * 1000),
            "N2"
        );
        var rtl = _missionClient.MissionItems.FirstOrDefault(_ =>
            _.Command.Value == MavCmd.MavCmdNavReturnToLaunch
        );
        if (rtl is not null)
        {
            TotalDistance.Value = DistanceUnitItem.Value.Current.Value.Print(
                _totalMissionDistance + (_distanceBeforeMission * 2000),
                "N2"
            );
        }

        Observable
            .Timer(TimeSpan.Zero, TimeSpan.FromSeconds(1))
            .Subscribe(_ => CalculateMissionProgress())
            .DisposeItWith(Disposable);
    }

    internal async Task InitiateMissionPoints(CancellationToken cancel)
    {
        await DownloadMissionsImpl(cancel);
    }

    private async ValueTask DownloadMissionsImpl(CancellationToken cancel)
    {
        await _missionClient.Download(cancel, p => DownloadProgress.Value = p * 100);

        double homeAlt = 0;

        if (_positionClient.Home.CurrentValue != null)
        {
            homeAlt = _positionClient.Home.CurrentValue.Value.Altitude;
        }

        IsDownloaded.Value = true;
        for (var i = 0; i < _missionClient.MissionItems.Count; i++)
        {
            if (i == 0 && _device is ArduPlaneClientDevice)
            {
                continue;
            }

            var item = _missionClient.MissionItems[i];

            if (
                item.Command.Value == MavCmd.MavCmdNavWaypoint
                && item.Location.Value.Altitude <= homeAlt
            )
            {
                // TODO: Notify user on alt lower than start value
            }
        }
    }

    private void CalculateMissionProgress()
    {
        if (!_isOnMission)
        {
            return;
        }

        var toTargetDistance = GeoMath.Distance(
            _positionClient.Target.CurrentValue,
            _positionClient.Current.CurrentValue
        );
        var missionDistance = _missionClient.AllMissionsDistance.CurrentValue * 1000;
        var distance = Math.Abs(missionDistance - _passedDistance + toTargetDistance);
        var time = distance / _gnssClientEx.Main.GroundVelocity.CurrentValue;

        PathProgress.Value = CalculatePathProgressValue(missionDistance, distance);
        MissionFlightTime.Value = CalculateMissionFlightTime(time);
    }

    private string CalculateMissionFlightTime(double time)
    {
        if (time is double.NaN or double.PositiveInfinity || !_isOnMission)
        {
            return $"- {RS.MissionProgressViewModel_MissionFlightTime_Symbol}";
        }

        var minute = Math.Round(time / 60);

        if (minute < 1)
        {
            return $"<1 {RS.MissionProgressViewModel_MissionFlightTime_Symbol}";
        }

        return $"{minute} {RS.MissionProgressViewModel_MissionFlightTime_Symbol}";
    }

    private double CalculatePathProgressValue(double missionDistance, double distance) // TODO: extend logic for plane clients
    {
        if (!_isOnMission)
        {
            return Math.Abs((missionDistance - distance) / missionDistance);
        }

        switch (_device)
        {
            case ArduCopterClientDevice:
                if (_isOnMission && ReachedIndex.Value > 0)
                {
                    return Math.Abs((missionDistance - distance) / missionDistance);
                }

                if (CurrentIndex.Value == _missionClient.MissionItems.Count)
                {
                    return 1;
                }

                return 0;
            default:
                return Math.Abs((missionDistance - distance) / missionDistance);
        }
    }

    public override IEnumerable<IRoutable> GetRoutableChildren()
    {
        return [];
    }

    public BindableAsyncCommand UpdateMission { get; set; }

    public BindableReactiveProperty<string> MissionFlightTime { get; } =
        new($"- {RS.MissionProgressViewModel_MissionFlightTime_Symbol}");

    public BindableReactiveProperty<IUnit> DistanceUnitItem { get; } = new();
    public BindableReactiveProperty<double> DownloadProgress { get; } = new();
    public BindableReactiveProperty<string> MissionDistance { get; } = new(RS.Not_Available);
    public BindableReactiveProperty<string> TotalDistance { get; } = new(RS.Not_Available);
    public BindableReactiveProperty<string> HomeDistance { get; } = new(RS.Not_Available);
    public BindableReactiveProperty<string> TargetDistance { get; } = new(RS.Not_Available);
    public BindableReactiveProperty<bool> IsDownloaded { get; } = new(false);
    public BindableReactiveProperty<double> PathProgress { get; } = new(0);

    #region Dispose

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _cts.Cancel();
            MissionFlightTime.Dispose();
            DistanceUnitItem.Dispose();
            DownloadProgress.Dispose();
            MissionDistance.Dispose();
            TargetDistance.Dispose();
            TotalDistance.Dispose();
            HomeDistance.Dispose();
            IsDownloaded.Dispose();
            PathProgress.Dispose();
            CurrentIndex.Dispose();
            ReachedIndex.Dispose();
        }

        base.Dispose(disposing);
    }

    #endregion
}
