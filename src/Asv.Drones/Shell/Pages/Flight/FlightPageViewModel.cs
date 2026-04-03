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
    public double Rotation { get; set; } = 0.0;
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
        MapViewModel.Anchors.Add(drone);
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
        MapViewModel = new MapViewModel(nameof(MapViewModel), loggerFactory, mapService)
            .SetRoutableParent(this)
            .DisposeItWith(Disposable);

        Widgets = [];
        Widgets.SetRoutableParent(this).DisposeItWith(Disposable);
        Widgets.DisposeRemovedItems().DisposeItWith(Disposable);
        WidgetsView = Widgets.ToNotifyCollectionChangedSlim().DisposeItWith(Disposable);

        Events.Subscribe(InternalCatchEvent).DisposeItWith(Disposable);
    }

    public NotifyCollectionChangedSynchronizedViewList<IUavFlightWidget> WidgetsView { get; }
    public ObservableList<IUavFlightWidget> Widgets { get; }
    public IMap MapViewModel { get; }

    public override IEnumerable<IRoutable> GetChildren()
    {
        yield return MapViewModel;

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
                        cfg.MapCenter = MapViewModel.CenterMap.Value;
                        cfg.Zoom = MapViewModel.Zoom.Value;
                        cfg.Rotation = MapViewModel.Rotation.Value;
                    },
                    FlushingStrategy.FlushBothViewModelAndView
                );
                break;
            case LoadLayoutEvent loadLayoutEvent:
                _config = this.HandleLoadLayout<FlightPageViewModelConfig>(
                    loadLayoutEvent,
                    cfg =>
                    {
                        MapViewModel.CenterMap.Value = cfg.MapCenter;
                        MapViewModel.Zoom.Value = cfg.Zoom switch
                        {
                            < IZoomService.MinZoomLevel => IZoomService.MinZoomLevel,
                            > IZoomService.MaxZoomLevel => IZoomService.MaxZoomLevel,
                            _ => cfg.Zoom,
                        };
                        MapViewModel.Rotation.Value = cfg.Rotation;
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
