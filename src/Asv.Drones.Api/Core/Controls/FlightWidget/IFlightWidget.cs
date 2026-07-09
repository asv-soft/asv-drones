using Asv.Avalonia;
using ObservableCollections;

namespace Asv.Drones.Api;

public interface IFlightWidget : IWorkspaceWidget, IMenuActionTarget
{
    ObservableList<IFlightWidgetSection> Sections { get; }
    INotifyCollectionChangedSynchronizedViewList<IFlightWidgetSection> SectionsView { get; }
}
