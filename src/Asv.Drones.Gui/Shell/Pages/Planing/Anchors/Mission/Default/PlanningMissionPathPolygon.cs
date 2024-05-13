using System;
using System.Collections.ObjectModel;
using Asv.Avalonia.Map;
using Asv.Common;
using Asv.Drones.Gui.Api;
using Avalonia.Collections;
using DynamicData;

namespace Asv.Drones.Gui;

public class PlanningMissionPathPolygon : MapAnchorBase
{
    public const string UriString = WellKnownUri.ShellPageMapPlaning + "/layer/{0}";

    private ReadOnlyObservableCollection<GeoPoint> _points;
    //private ReadOnlyObservableCollection<MapAnchorActionViewModel> _actions;

    public PlanningMissionPathPolygon(string name) : base(new Uri(UriString.FormatWith(name)))
    {
        //TODO: Need to fix a problem with polygon, when not redrawing polygon line when changing point indices. 
        //TODO: Make possibility to act with a polygon to add points on free edges of polygon.
        Points = new SourceCache<PlaningMissionAnchor, int>(_ => _.Index).DisposeItWith(Disposable);
        Points.Connect()
            .AutoRefresh(_ => _.Index)
            .SortBy(_ => _.Index)
            .Transform(_ => _.Location)
            .Bind(out _points)
            .Subscribe()
            .DisposeItWith(Disposable);

        IsEditable = true;
        IsVisible = true;
        OffsetX = OffsetXEnum.Center;
        OffsetY = OffsetYEnum.Bottom;
        StrokeDashArray = new AvaloniaList<double> { 2, 2 };
        StrokeThickness = 4;
        PathOpacity = 0.6;
        Path = _points;
    }

    public override ReadOnlyObservableCollection<GeoPoint> Path { get; }
    public SourceCache<PlaningMissionAnchor, int> Points { get; }

    //public override ReadOnlyObservableCollection<MapAnchorActionViewModel> Actions => _actions;
}