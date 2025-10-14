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
using R3;

namespace Asv.Drones;

public class MissionProgressViewModel : RoutableViewModel
{
    public const string ViewModelId = "mission.progress";

    private readonly IClientDevice _device;
    private readonly IPositionClientEx _positionClient;
    private readonly IMissionClientEx _missionClient;
    private readonly IGnssClientEx _gnssClientEx;
    private readonly IUnit _distanceUnit;
    private readonly CancellationTokenSource _cts;
    private readonly ReactiveProperty<ushort> _currentIndex;
    private readonly ReactiveProperty<ushort> _reachedIndex;
    private double _passedDistance;
    private bool _isOnMission;

    public MissionProgressViewModel()
        : base(ViewModelId, DesignTime.LoggerFactory)
    {
        DesignTime.ThrowIfNotDesignMode();
        _cts = new CancellationTokenSource().DisposeItWith(Disposable);
        _currentIndex = new ReactiveProperty<ushort>(0).DisposeItWith(Disposable);
        _reachedIndex = new ReactiveProperty<ushort>(0).DisposeItWith(Disposable);
        var missionDistance = new ReactiveProperty<double>(1000).DisposeItWith(Disposable);
        var targetDistance = new ReactiveProperty<double>(100).DisposeItWith(Disposable);
        var totalDistance = new ReactiveProperty<double>(1100).DisposeItWith(Disposable);
        var homeDistance = new ReactiveProperty<double>(100).DisposeItWith(Disposable);

        PathProgress = new BindableReactiveProperty<double>(0).DisposeItWith(Disposable);
        IsDownloaded = new BindableReactiveProperty<bool>(false).DisposeItWith(Disposable);
        DownloadProgress = new BindableReactiveProperty<double>().DisposeItWith(Disposable);

        var unitService = NullUnitService.Instance;
        var nullUnit = unitService.Units[NullUnitBase.Id];

        MissionDistance = new HistoricalUnitProperty(
            nameof(MissionDistance),
            missionDistance,
            nullUnit,
            DesignTime.LoggerFactory,
            "N2"
        )
            .SetRoutableParent(this)
            .DisposeItWith(Disposable);
        MissionDistance.ForceValidate();
        HomeDistance = new HistoricalUnitProperty(
            nameof(HomeDistance),
            homeDistance,
            nullUnit,
            DesignTime.LoggerFactory,
            "N2"
        )
            .SetRoutableParent(this)
            .DisposeItWith(Disposable);
        HomeDistance.ForceValidate();
        TargetDistance = new HistoricalUnitProperty(
            nameof(TargetDistance),
            targetDistance,
            nullUnit,
            DesignTime.LoggerFactory,
            "N2"
        )
            .SetRoutableParent(this)
            .DisposeItWith(Disposable);
        TargetDistance.ForceValidate();
        TotalDistance = new HistoricalUnitProperty(
            nameof(TotalDistance),
            totalDistance,
            nullUnit,
            DesignTime.LoggerFactory,
            "N2"
        )
            .SetRoutableParent(this)
            .DisposeItWith(Disposable);
        TotalDistance.ForceValidate();

        IsDownloaded.Value = true;
        MissionFlightTime.Value = "15 min";
        PathProgress.Value = 0.7;
    }

    [ImportingConstructor]
    public MissionProgressViewModel(
        IClientDevice device,
        IUnitService unitService,
        ILoggerFactory loggerFactory,
        IRoutable parent
    )
        : base(ViewModelId, loggerFactory)
    {
        ArgumentNullException.ThrowIfNull(device);
        ArgumentNullException.ThrowIfNull(unitService);
        ArgumentNullException.ThrowIfNull(loggerFactory);
        _device = device;
        Parent = parent;
        _distanceUnit = unitService.Units[DistanceBase.Id];
        _missionClient =
            device.GetMicroservice<IMissionClientEx>()
            ?? throw new Exception($"Unable to load {nameof(IMissionClientEx)} from {device.Id}");
        _gnssClientEx =
            device.GetMicroservice<IGnssClientEx>()
            ?? throw new Exception($"Unable to load {nameof(IGnssClientEx)} from {device.Id}");
        _positionClient =
            device.GetMicroservice<IPositionClientEx>()
            ?? throw new Exception($"Unable to load {nameof(IPositionClientEx)} from {device.Id}");
        var mode =
            device.GetMicroservice<IModeClient>()
            ?? throw new Exception($"Unable to load {nameof(IModeClient)} from {device.Id}");

        UpdateMission = new BindableAsyncCommand(UpdateMissionCommand.Id, this);

        _cts = new CancellationTokenSource().DisposeItWith(Disposable);
        _currentIndex = new ReactiveProperty<ushort>(0).DisposeItWith(Disposable);
        _reachedIndex = new ReactiveProperty<ushort>(0).DisposeItWith(Disposable);
        var missionDistance = new ReactiveProperty<double>().DisposeItWith(Disposable);
        var targetDistance = new ReactiveProperty<double>().DisposeItWith(Disposable);
        var totalDistance = new ReactiveProperty<double>().DisposeItWith(Disposable);
        var homeDistance = new ReactiveProperty<double>().DisposeItWith(Disposable);

        PathProgress = new BindableReactiveProperty<double>(0).DisposeItWith(Disposable);
        IsDownloaded = new BindableReactiveProperty<bool>(false).DisposeItWith(Disposable);
        DownloadProgress = new BindableReactiveProperty<double>().DisposeItWith(Disposable);

        _missionClient
            .AllMissionsDistance.Subscribe(d =>
            {
                missionDistance.Value = d * 1000;
                var start = _missionClient.MissionItems.FirstOrDefault();
                var stop = _missionClient.MissionItems.LastOrDefault(missionItem =>
                    missionItem.Command.Value != MavCmd.MavCmdNavReturnToLaunch
                );
                if (start != null && stop != null)
                {
                    missionDistance.Value += GeoMath.Distance(
                        start.Location.Value,
                        _positionClient.Home.CurrentValue
                    );
                    missionDistance.Value += GeoMath.Distance(
                        stop.Location.Value,
                        _positionClient.Home.CurrentValue
                    );
                }

                if (missionDistance.Value < 1)
                {
                    missionDistance.Value = d * 1000;
                }
            })
            .DisposeItWith(Disposable);
        _positionClient
            .HomeDistance.Subscribe(d => homeDistance.Value = d)
            .DisposeItWith(Disposable);
        _positionClient
            .TargetDistance.Subscribe(d => targetDistance.Value = d)
            .DisposeItWith(Disposable);

        _missionClient
            .AllMissionsDistance.Select(v => v * 1000)
            .Subscribe(v =>
            {
                var distanceBeforeMission =
                    _missionClient.MissionItems.Count == 0
                        ? 0
                        : GeoMath.Distance(
                            _positionClient.Current.CurrentValue,
                            _missionClient.MissionItems[0].Location.Value
                        );

                var rtl = _missionClient.MissionItems.FirstOrDefault(_ =>
                    _.Command.Value == MavCmd.MavCmdNavReturnToLaunch
                );
                if (rtl is not null)
                {
                    totalDistance.Value = v + (distanceBeforeMission * 2000);
                    return;
                }

                totalDistance.Value = v + (distanceBeforeMission * 1000);
            })
            .DisposeItWith(Disposable);

        MissionDistance = new HistoricalUnitProperty(
            nameof(MissionDistance),
            missionDistance,
            _distanceUnit,
            loggerFactory,
            "N2"
        )
            .SetRoutableParent(this)
            .DisposeItWith(Disposable);
        MissionDistance.ForceValidate();
        HomeDistance = new HistoricalUnitProperty(
            nameof(HomeDistance),
            homeDistance,
            _distanceUnit,
            loggerFactory,
            "N2"
        )
            .SetRoutableParent(this)
            .DisposeItWith(Disposable);
        HomeDistance.ForceValidate();
        TargetDistance = new HistoricalUnitProperty(
            nameof(TargetDistance),
            targetDistance,
            _distanceUnit,
            loggerFactory,
            "N2"
        )
            .SetRoutableParent(this)
            .DisposeItWith(Disposable);
        TargetDistance.ForceValidate();
        TotalDistance = new HistoricalUnitProperty(
            nameof(TotalDistance),
            totalDistance,
            _distanceUnit,
            loggerFactory,
            "N2"
        )
            .SetRoutableParent(this)
            .DisposeItWith(Disposable);
        TotalDistance.ForceValidate();

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
                    _reachedIndex.Value = 0;
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

        _currentIndex
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

        _missionClient.Reached.Subscribe(i => _reachedIndex.Value = i).DisposeItWith(Disposable);
        _missionClient.Current.Subscribe(i => _currentIndex.Value = i).DisposeItWith(Disposable);

        DistanceUnitSymbol = _distanceUnit
            .CurrentUnitItem.Select(item => item.Symbol)
            .ToBindableReactiveProperty<string>()
            .DisposeItWith(Disposable);

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
                if (_isOnMission && _reachedIndex.Value > 0)
                {
                    return Math.Abs((missionDistance - distance) / missionDistance);
                }

                if (_currentIndex.Value == _missionClient.MissionItems.Count)
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
        yield return MissionDistance;
        yield return HomeDistance;
        yield return TargetDistance;
        yield return TotalDistance;
    }

    public BindableAsyncCommand UpdateMission { get; set; }

    public BindableReactiveProperty<string> MissionFlightTime { get; } =
        new($"- {RS.MissionProgressViewModel_MissionFlightTime_Symbol}");

    public BindableReactiveProperty<double> DownloadProgress { get; }
    public HistoricalUnitProperty MissionDistance { get; }
    public HistoricalUnitProperty TotalDistance { get; }
    public HistoricalUnitProperty HomeDistance { get; }
    public HistoricalUnitProperty TargetDistance { get; }
    public BindableReactiveProperty<bool> IsDownloaded { get; }
    public BindableReactiveProperty<double> PathProgress { get; }
    public BindableReactiveProperty<string> DistanceUnitSymbol { get; }
}
