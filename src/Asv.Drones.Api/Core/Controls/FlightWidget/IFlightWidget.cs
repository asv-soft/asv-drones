using Asv.Avalonia;
using ObservableCollections;

namespace Asv.Drones.Api;

public interface IFlightWidget : IWorkspaceWidget
{
    ObservableList<IFlightWidgetSection> Sections { get; }
    INotifyCollectionChangedSynchronizedViewList<IFlightWidgetSection> SectionsView { get; }
    ObservableList<IMenuItem> Menu { get; }
}
