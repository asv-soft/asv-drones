﻿using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using Asv.Common;
using Asv.Drones.Gui.Api;
using Asv.Mavlink.V2.Common;
using Asv.Mavlink.Vehicle;
using Avalonia.Controls;
using DynamicData;
using DynamicData.Binding;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using ReactiveUI.Validation.Extensions;

namespace Asv.Drones.Gui;

public class PlaningMissionViewModel : DisposableReactiveObjectWithValidation
{
    private readonly Guid _id;
    private readonly IPlaningMission _svc;
    private readonly IPlaningMissionContext _context;
    private readonly SourceCache<PlaningMissionPointModel, int> _source;
    private readonly ReadOnlyObservableCollection<PlaningMissionPointViewModel> _points;

    public PlaningMissionViewModel()
    {
        DesignTime.ThrowIfNotDesignMode();
    }

    public PlaningMissionViewModel(Guid id, string? name, ILogService log, IPlaningMissionPointFactory pointFactory,
        IPlaningMission svc, IPlaningMissionContext context)
    {
        _id = id;
        _context = context;
        _svc = svc;

        _source = new SourceCache<PlaningMissionPointModel, int>(x => x.Index)
            .DisposeItWith(Disposable);
        _source
            .Connect()
            .Transform(x =>
            {
                var point = pointFactory.Create(x, this);
                if (_context.IsInAnchorEditMode && point.MissionAnchor != null)
                {
                    point.MissionAnchor.IsInEditMode = true;
                }

                return point;
            })
            .SortBy(x => x.Point.Index)
            .Bind(out _points)
            .DisposeMany()
            .Subscribe(_ =>
            {
                TotalDistance = 0;
                if (Points.Count <= 1) return;
                for (int i = 1; i < Points.Count; i++)
                {
                    TotalDistance = GeoMath.Distance(Points[i - 1].Point.Location, Points[i].Point.Location);
                }
            })
            .DisposeItWith(Disposable);

        _points.ToObservableChangeSet(_ => _.Index)
            .WhenValueChanged(x => x.IsChanged)
            .Where(x => x)
            .Subscribe(_ => IsChanged = true)
            .DisposeItWith(Disposable);

        this.WhenAnyValue(_ => _.Name).Subscribe(_ => IsChanged = true).DisposeItWith(Disposable);
        this.ValidationRule(x => x.Name, name => !string.IsNullOrWhiteSpace(name),
            RS.PlaningMissionViewModel_NameMustBeNotEmpty);

        this.WhenValueChanged(_ => _.SelectedPoint)
            .Subscribe(_ =>
            {
                if (_ == null)
                {
                    context.SelectedItem = null;
                    return;
                }

                context.SelectedItem = _.MissionAnchor;

                if (_.MissionAnchor != null && !_.MissionAnchor.IsInEditMode)
                {
                    context.Center = _.MissionAnchor.Location;
                }
            })
            .DisposeItWith(Disposable);

        context.WhenValueChanged(_ => _.SelectedItem)
            .Subscribe(anchor =>
            {
                if (anchor == null) return;

                SelectedPoint = _points.FirstOrDefault(point => point.MissionAnchor == anchor);
            })
            .DisposeItWith(Disposable);

        _source.CountChanged.Subscribe(_ => IsChanged = true).DisposeItWith(Disposable);
        AddPointCmd = ReactiveCommand.CreateFromTask<MavCmd>(AddPointImpl).DisposeItWith(Disposable);
        ReplacePointCmd = ReactiveCommand.Create<MavCmd>(ReplacePointImpl).DisposeItWith(Disposable);
        SaveCmd = ReactiveCommand.Create(SaveImpl, this.IsValid()).DisposeItWith(Disposable);
        SaveCmd.ThrownExceptions.Subscribe(ex => log.Error("Planing", ex.Message, ex)).DisposeItWith(Disposable);
        MoveTop = ReactiveCommand.Create(() =>
        {
            var previousPoint = Points.LastOrDefault(_ => _.Index < SelectedPoint.Index);
            if (previousPoint == null) return;
            (previousPoint.Index, SelectedPoint.Index) = (SelectedPoint.Index, previousPoint.Index);
            var selectedPointIndex = SelectedPoint.Index;
            _source.AddOrUpdate(new[]
            {
                previousPoint.Point,
                SelectedPoint.Point
            });
            SelectedPoint = Points.FirstOrDefault(_ => _.Index == selectedPointIndex);
        }).DisposeItWith(Disposable);
        MoveDown = ReactiveCommand.Create(() =>
        {
            var nextPoint = Points.FirstOrDefault(_ => _.Index > SelectedPoint.Index);
            if (nextPoint == null) return;
            (nextPoint.Index, SelectedPoint.Index) = (SelectedPoint.Index, nextPoint.Index);
            var selectedPointIndex = SelectedPoint.Index;
            _source.AddOrUpdate(new[]
            {
                nextPoint.Point,
                SelectedPoint.Point
            });
            SelectedPoint = Points.FirstOrDefault(_ => _.Index == selectedPointIndex);
        }).DisposeItWith(Disposable);
        IsChanged = false;
        Name = name;
    }

    public Guid MissionId => _id;
    [Reactive] public string? Name { get; set; }
    [Reactive] public bool IsChanged { get; set; }
    [Reactive] public double TotalDistance { get; set; }
    public ReadOnlyObservableCollection<PlaningMissionPointViewModel> Points => _points;
    public ReactiveCommand<MavCmd, Unit> AddPointCmd { get; }
    public ReactiveCommand<MavCmd, Unit> ReplacePointCmd { get; }
    public ReactiveCommand<Unit, Unit> MoveTop { get; }
    public ReactiveCommand<Unit, Unit> MoveDown { get; }
    public ReactiveCommand<Unit, Unit> SaveCmd { get; }
    public ReactiveCommand<Unit, Unit> SaveAsCmd { get; }
    [Reactive] public PlaningMissionPointViewModel SelectedPoint { get; set; }


    public void AddOrUpdatePoint(PlaningMissionPointModel model)
    {
        _source.AddOrUpdate(model);
    }

    public void RemovePoint(PlaningMissionPointModel model)
    {
        _source.Remove(model);
    }

    public void RemovePoint(int index)
    {
        _source.Remove(index);
    }

    public void ClearAllPoints()
    {
        _source.Clear();
    }

    public void RemovePoint(PlaningMissionPointViewModel item)
    {
        _source.Remove(item.Point);
    }

    private void Load(PlaningMissionModel model, SemVersion fileVersion)
    {
        model.Points.ForEach(x => _source.AddOrUpdate(x));
        _points.ForEach(_ => _.IsChanged = false);
    }

    private PlaningMissionModel Save(SemVersion fileVersion)
    {
        var model = new PlaningMissionModel();
        model.Points.AddRange(_points.Select(x => x.SaveToJson(fileVersion)));
        return model;
    }

    public void SaveImpl()
    {
        var needRename = false;
        using (var handle = _svc.MissionStore.OpenFile(_id))
        {
            handle.File.Save(Save(handle.File.FileVersion));
            if (handle.Name.Equals(Name, StringComparison.InvariantCulture) == false)
            {
                needRename = true;
            }
        }

        if (needRename)
        {
            _svc.MissionStore.RenameFile(_id, Name);
        }

        Points.ForEach(_ => _.IsChanged = false);
        IsChanged = false;
    }

    private void ReplacePointImpl(MavCmd type)
    {
        var selectedPoint = SelectedPoint;
        selectedPoint.Point.Type = type;
        _source.Remove(SelectedPoint.Index);
        _source.AddOrUpdate(selectedPoint.Point);
    }

    private async Task AddPointImpl(MavCmd type, CancellationToken cancel)
    {
        //TODO: Make a possibility to insert points
        var indexToAdd = _source.Count == 0 ? 1 : _source.Keys.Max() + 1;

        var model = new PlaningMissionPointModel
        {
            Type = type,
            Index = indexToAdd
        };

        if (type == MavCmd.MavCmdDoChangeSpeed)
        {
            model.Location = GeoPoint.Zero;
        }
        else
        {
            var point = await _context.ShowTargetDialog(RS.PlaningMissionViewModel_SelectTargetLocation, cancel);
            if (!point.Equals(GeoPoint.NaN))
                model.Location = point;
        }

        _source.AddOrUpdate(model);
    }

    public void Load(PlaningMissionFile file)
    {
        var model = file.Load();
        Load(model, file.FileVersion);
    }
}