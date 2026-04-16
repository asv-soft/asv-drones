using Asv.Avalonia;
using Asv.Avalonia.GeoMap;
using ObservableCollections;
using R3;

namespace Asv.Drones;

public interface IFlightModePage : IPage
{
    ObservableList<IFlightWidget> Widgets { get; }
    ObservableList<IMapAnchor> Anchors { get; }
    BindableReactiveProperty<IMapAnchor?> SelectedAnchor { get; }
}
