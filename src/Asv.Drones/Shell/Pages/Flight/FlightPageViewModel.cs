using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Asv.Avalonia;
using Asv.Avalonia.GeoMap;
using Asv.Common;
using Asv.Drones.Api;
using Asv.Modeling;
using Material.Icons;
using Microsoft.Extensions.Logging;
using ObservableCollections;

namespace Asv.Drones;

public class FlightPageViewModel : PageViewModel<IFlightMode>, IFlightMode
{
    public const string PageId = "flight";
    public const MaterialIconKind PageIcon = MaterialIconKind.MapSearch;

    public FlightPageViewModel()
        : this(
            DesignTime.PageContext,
            NullMapService.Instance,
            DesignTime.LoggerFactory,
            DesignTime.DialogService,
            DesignTime.ExtensionService
        )
    {
        DesignTime.ThrowIfNotDesignMode();
        var drone = new MapAnchor(DesignTime.Id.TypeId)
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
    }

    public FlightPageViewModel(
        IPageContext context,
        IMapService mapService,
        ILoggerFactory loggerFactory,
        IDialogService dialogService,
        IExtensionService ext
    )
        : base(PageId, context, loggerFactory, dialogService, ext)
    {
        Header = RS.FlightPageViewModel_Title;
        Icon = PageIcon;
        MapViewModel = new MapViewModel(nameof(MapViewModel), mapService)
            .SetRoutableParent(this)
            .DisposeItWith(Disposable);

        Widgets = [];
        Widgets.SetRoutableParent(this).DisposeItWith(Disposable);
        Widgets.DisposeRemovedItems().DisposeItWith(Disposable);
        WidgetsView = Widgets.ToNotifyCollectionChangedSlim().DisposeItWith(Disposable);
    }

    public NotifyCollectionChangedSynchronizedViewList<IUavFlightWidget> WidgetsView { get; }
    public ObservableList<IUavFlightWidget> Widgets { get; }
    public IMap MapViewModel { get; }

    public override IEnumerable<IViewModel> GetChildren()
    {
        yield return MapViewModel;

        foreach (var widget in WidgetsView)
        {
            yield return widget;
        }
    }

    protected override void AfterLoadExtensions()
    {
        // nothing to do
    }
}
