using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Asv.Avalonia;
using Asv.Avalonia.GeoMap;
using Asv.Common;
using Asv.Drones.Api;
using Asv.Modeling;
using Material.Icons;
using Microsoft.Extensions.Logging;
using ObservableCollections;
using R3;

namespace Asv.Drones;

public sealed class FlightModePageViewModelConfig
{
    public GeoPoint MapCenter { get; set; } = GeoPoint.Zero;
    public int Zoom { get; set; } = 0;
}

public class FlightModePageViewModel : PageViewModel<IFlightModePage>, IFlightModePage
{
    public const string PageId = "flight-mode";
    public const MaterialIconKind PageIcon = MaterialIconKind.MapSearch;

    private FlightModePageViewModelConfig? _config;

    public FlightModePageViewModel()
        : this(
            NullMapService.Instance,
            DesignTime.CommandService,
            DesignTime.LoggerFactory,
            DesignTime.DialogService,
            DesignTime.ExtensionService
        ) { }

    public FlightModePageViewModel(
        IMapService mapService,
        ICommandService cmd,
        ILoggerFactory loggerFactory,
        IDialogService dialogService,
        IExtensionService ext
    )
        : base(PageId, cmd, loggerFactory, dialogService, ext)
    {
        Title = "Flight (BETA)";
        Icon = PageIcon;

        Widgets = [];
        Widgets.SetRoutableParent(this).DisposeItWith(Disposable);
        Widgets.DisposeRemovedItems().DisposeItWith(Disposable);
        Widgets
            .ObserveAdd()
            .ObserveOnUIThreadDispatcher()
            .Subscribe(_ => Widgets.Sort(FlightWidgetsComparer.Instance))
            .DisposeItWith(Disposable);

        WidgetsView = Widgets.ToNotifyCollectionChangedSlim().DisposeItWith(Disposable);

        Map = new MapViewModel(nameof(Map), loggerFactory, mapService)
            .SetRoutableParent(this)
            .DisposeItWith(Disposable);

        Events.Subscribe(InternalCatchEvent).DisposeItWith(Disposable);
    }

    public ObservableList<IFlightWidget> Widgets { get; }
    public NotifyCollectionChangedSynchronizedViewList<IFlightWidget> WidgetsView { get; }
    public IMap Map { get; }

    public override IEnumerable<IRoutable> GetChildren()
    {
        yield return Map;

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
                        cfg.MapCenter = Map.CenterMap.Value;
                        cfg.Zoom = Map.Zoom.Value;
                    },
                    FlushingStrategy.FlushBothViewModelAndView
                );
                break;
            case LoadLayoutEvent loadLayoutEvent:
                _config = this.HandleLoadLayout<FlightModePageViewModelConfig>(
                    loadLayoutEvent,
                    cfg =>
                    {
                        Map.CenterMap.Value = cfg.MapCenter;
                        Map.Zoom.Value = cfg.Zoom switch
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
