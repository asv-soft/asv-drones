using Asv.Avalonia;
using ObservableCollections;

namespace Asv.Drones.Api;

public interface IFlightWidget : IWorkspaceWidget
{
    int Order { get; }
    ObservableList<IFlightWidgetSection> Sections { get; }
    INotifyCollectionChangedSynchronizedViewList<IFlightWidgetSection> SectionsView { get; }
}
