using Asv.Avalonia;
using Asv.Avalonia.GeoMap;
using Asv.Common;
using Asv.Modeling;
using Microsoft.Extensions.Logging;
using ObservableCollections;

namespace Asv.Drones;

public class PlaningPageViewModel : PageViewModel<IPlaningPage>, IPlaningPage
{
    public const string PageId = "planing";

    public PlaningPageViewModel()
        : this(NullPageContext.Instance, DesignTime.LoggerFactory, DesignTime.DialogService, DesignTime.ExtensionService, NullMapService.Instance)
    {
        DesignTime.ThrowIfNotDesignMode();
    }
    
    public PlaningPageViewModel(
        IPageContext context, 
        ILoggerFactory loggerFactory, 
        IDialogService dialogService, 
        IExtensionService ext,
        IMapService mapService)
        : base(PageId, context, loggerFactory, dialogService, ext)
    {
        Widgets = [];
        Widgets.SetRoutableParent(this).DisposeItWith(Disposable);
        Widgets.DisposeRemovedItems().DisposeItWith(Disposable);

        WidgetsView = Widgets.ToNotifyCollectionChangedSlim().DisposeItWith(Disposable);

        Map = new MapViewModel(nameof(Map), mapService)
            .SetRoutableParent(this)
            .DisposeItWith(Disposable);
    }

    public IMap Map { get; }

    public NotifyCollectionChangedSynchronizedViewList<IWorkspaceWidget> WidgetsView { get; }

    public ObservableList<IWorkspaceWidget> Widgets { get; }

    protected override void AfterLoadExtensions()
    {
        // do nothing
    }
}