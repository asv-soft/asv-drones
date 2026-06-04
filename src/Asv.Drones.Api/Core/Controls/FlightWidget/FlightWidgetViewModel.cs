using Asv.Avalonia;
using Asv.Common;
using Asv.Modeling;
using Material.Icons;
using ObservableCollections;
using R3;

namespace Asv.Drones.Api;

public abstract class FlightWidgetViewModel<TSelf> : ViewModel<TSelf>, IFlightWidget
    where TSelf : class, IFlightWidget
{
    protected FlightWidgetViewModel(NavId id, IExtensionService ext)
        : base(id.TypeId, id.Args, ext)
    {
        Menu.SetRoutableParent(this).DisposeItWith(Disposable);
        Menu.DisposeRemovedItems().DisposeItWith(Disposable);
        MenuView = new MenuTree(Menu).DisposeItWith(Disposable);

        Sections = [];
        Sections.SetRoutableParent(this).DisposeItWith(Disposable);
        Sections.DisposeRemovedItems().DisposeItWith(Disposable);
        Sections
            .ObserveAdd()
            .ObserveOnUIThreadDispatcher()
            .Subscribe(_ => Sections.Sort(FlightWidgetSectionsComparer.Instance))
            .DisposeItWith(Disposable);

        Disposable.AddAction(() => Sections.ClearWithItemsDispose());

        SectionsView = Sections.ToNotifyCollectionChangedSlim().DisposeItWith(Disposable);
    }

    public MaterialIconKind? Icon
    {
        get;
        set => SetField(ref field, value);
    }

    public AsvColorKind IconColor
    {
        get;
        set => SetField(ref field, value);
    }

    public string? Header
    {
        get;
        set => SetField(ref field, value);
    }

    public WorkspaceDock Position
    {
        get;
        set => SetField(ref field, value);
    }

    public bool IsExpanded
    {
        get;
        set => SetField(ref field, value);
    }

    public bool CanExpand
    {
        get;
        set => SetField(ref field, value);
    }

    public bool IsVisible
    {
        get;
        set => SetField(ref field, value);
    } = true;

    public ObservableList<IMenuItem> Menu { get; } = [];

    public MenuTree? MenuView { get; }

    public abstract int Order { get; }

    public ObservableList<IFlightWidgetSection> Sections { get; }
    public INotifyCollectionChangedSynchronizedViewList<IFlightWidgetSection> SectionsView { get; }

    public override IEnumerable<IViewModel> GetChildren()
    {
        foreach (var item in SectionsView)
        {
            yield return item;
        }

        foreach (var item in Menu)
        {
            yield return item;
        }
    }

    protected override void AfterLoadExtensions()
    {
        // nothing to do
    }
}
