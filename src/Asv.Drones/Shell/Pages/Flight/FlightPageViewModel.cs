using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Asv.Avalonia;
using Asv.Avalonia.GeoMap;
using Asv.Common;
using Asv.Drones.Api;
using Asv.IO;
using Material.Icons;
using Microsoft.Extensions.Logging;
using ObservableCollections;
using R3;

namespace Asv.Drones;

public sealed class FlightPageViewModelConfig
{
    public GeoPoint MapCenter { get; set; } = GeoPoint.Zero;
    public int Zoom { get; set; } = IZoomService.MinZoomLevel;
}

public class FlightPageViewModel : PageViewModel<IFlightMode>, IFlightMode
{
    public const string PageId = "flight";
    public const MaterialIconKind PageIcon = MaterialIconKind.MapSearch;

    private FlightPageViewModelConfig? _config;

    public FlightPageViewModel()
        : this(
            NullMapService.Instance,
            DesignTime.CommandService,
            DesignTime.LoggerFactory,
            DesignTime.DialogService,
            DesignTime.ExtensionService
        )
    {
        DesignTime.ThrowIfNotDesignMode();
        var drone = new MapAnchor<IMapAnchor>(DesignTime.Id, DesignTime.LoggerFactory)
        {
            Icon = MaterialIconKind.Navigation,
            Location = new GeoPoint(53, 53, 100),
        };
        Anchors.Add(drone);
        var azimuth = 0;
        TimeProvider.System.CreateTimer(
            x =>
            {
                drone.Azimuth = (azimuth++ * 10) % 360;
            },
            null,
            TimeSpan.FromSeconds(1),
            TimeSpan.FromSeconds(1)
        );
        Widgets.Add(new UavWidgetViewModel { Header = "Device11" });
    }

    public FlightPageViewModel(
        IMapService mapService,
        ICommandService cmd,
        ILoggerFactory loggerFactory,
        IDialogService dialogService,
        IExtensionService ext
    )
        : base(PageId, cmd, loggerFactory, dialogService, ext)
    {
        Title = RS.FlightPageViewModel_Title;
        Icon = PageIcon;
        Anchors = [];
        Anchors.SetRoutableParent(this).DisposeItWith(Disposable);
        Anchors.DisposeRemovedItems().DisposeItWith(Disposable);
        AnchorsView = Anchors.ToNotifyCollectionChangedSlim().DisposeItWith(Disposable);
        Widgets = [];
        Widgets.SetRoutableParent(this).DisposeItWith(Disposable);
        Widgets.DisposeRemovedItems().DisposeItWith(Disposable);

        WidgetsView = Widgets.ToNotifyCollectionChangedSlim().DisposeItWith(Disposable);
        SelectedAnchor = new BindableReactiveProperty<IMapAnchor?>().DisposeItWith(Disposable);
        Zoom = new BindableReactiveProperty<int>(1).DisposeItWith(Disposable);
        MapCenter = new BindableReactiveProperty<GeoPoint>(GeoPoint.Zero).DisposeItWith(Disposable);
        TileProvider = mapService
            .CurrentProvider.ToReadOnlyBindableReactiveProperty<ITileProvider>()
            .DisposeItWith(Disposable);
        Rotation = new BindableReactiveProperty<double>(0.0).DisposeItWith(Disposable);

        Events.Subscribe(InternalCatchEvent).DisposeItWith(Disposable);
    }

    public NotifyCollectionChangedSynchronizedViewList<IUavFlightWidget> WidgetsView { get; }
    public ObservableList<IUavFlightWidget> Widgets { get; }
    public NotifyCollectionChangedSynchronizedViewList<IMapAnchor> AnchorsView { get; }
    public ObservableList<IMapAnchor> Anchors { get; }
    public BindableReactiveProperty<IMapAnchor?> SelectedAnchor { get; }
    public BindableReactiveProperty<int> Zoom { get; }
    public BindableReactiveProperty<GeoPoint> MapCenter { get; }
    public IReadOnlyBindableReactiveProperty<ITileProvider> TileProvider { get; }
    public BindableReactiveProperty<double> Rotation { get; }

    public override IEnumerable<IRoutable> GetChildren()
    {
        foreach (var item in AnchorsView)
        {
            yield return item;
        }

        foreach (var widget in WidgetsView)
        {
            yield return widget;
        }
    }

    private ValueTask InternalCatchEvent(IRoutable src, AsyncRoutedEvent<IRoutable> e)
    {
        switch (e)
        {
            case SaveLayoutEvent saveLayoutEvent:
                if (_config is null)
                {
                    break;
                }

                this.HandleSaveLayout(
                    saveLayoutEvent,
                    _config,
                    cfg =>
                    {
                        cfg.MapCenter = MapCenter.Value;
                        cfg.Zoom = Zoom.Value;
                    },
                    FlushingStrategy.FlushBothViewModelAndView
                );
                break;
            case LoadLayoutEvent loadLayoutEvent:
                _config = this.HandleLoadLayout<FlightPageViewModelConfig>(
                    loadLayoutEvent,
                    cfg =>
                    {
                        MapCenter.Value = cfg.MapCenter;
                        Zoom.Value = cfg.Zoom switch
                        {
                            < IZoomService.MinZoomLevel => IZoomService.MinZoomLevel,
                            > IZoomService.MaxZoomLevel => IZoomService.MaxZoomLevel,
                            _ => cfg.Zoom,
                        };
                    }
                );
                break;
        }

        return ValueTask.CompletedTask;
    }

    protected override void AfterLoadExtensions()
    {
        // nothing to do
    }
}
