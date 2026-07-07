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

public class FlightModePageViewModel : PageViewModel<IFlightModePage>, IFlightModePage
{
    public const string PageId = "flight-mode";
    public const MaterialIconKind PageIcon = MaterialIconKind.MapSearch;

    public FlightModePageViewModel()
        : this(
            DesignTime.PageContext,
            NullMapService.Instance,
            DesignTime.LoggerFactory,
            DesignTime.DialogService,
            DesignTime.ExtensionService
        )
    {
        DesignTime.ThrowIfNotDesignMode();
    }

    public FlightModePageViewModel(
        IPageContext context,
        IMapService mapService,
        ILoggerFactory loggerFactory,
        IDialogService dialogService,
        IExtensionService ext
    )
        : base(PageId, context, loggerFactory, dialogService, ext)
    {
        Header = "Flight (BETA)";
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

        Map = new MapViewModel(nameof(Map), mapService)
            .SetRoutableParent(this)
            .DisposeItWith(Disposable);
    }

    public ObservableList<IFlightWidget> Widgets { get; }
    public NotifyCollectionChangedSynchronizedViewList<IFlightWidget> WidgetsView { get; }
    public IMap Map { get; }

    public override IEnumerable<IViewModel> GetChildren()
    {
        yield return Map;

        foreach (var widget in WidgetsView)
        {
            yield return widget;
        }
    }

    protected override void AfterLoadExtensions()
    {
        Layout.Register(
            nameof(Map.CenterMap),
            x => Map.CenterMap.Value = x,
            () => Map.CenterMap.Value,
            Map.CenterMap.Skip(1)
        );
        Layout.Register(
            nameof(Map.Zoom),
            x => Map.Zoom.Value = x,
            () => Map.Zoom.Value,
            Map.Zoom.Skip(1)
        );
        Layout.Register(
            nameof(Map.Rotation),
            x => Map.Rotation.Value = x,
            () => Map.Rotation.Value,
            Map.Rotation.Skip(1)
        );
        Layout.LoadWhenRootAttached(RootTracking).AddTo(ref DisposableBag);
    }
}
