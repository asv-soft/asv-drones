using Asv.Avalonia;
using Asv.Avalonia.Map;
using Asv.IO;
using ObservableCollections;
using R3;

namespace Asv.Drones.Api;

public interface IFlightMode : IPage
{
    ObservableList<IMapWidget> Widgets { get; }
    ObservableList<IMapAnchor> Anchors { get; }
    BindableReactiveProperty<IMapAnchor?> SelectedAnchor { get; }
}

public interface IUavFlightWidget : IMapWidget
{
    IClientDevice Device { get; }
}