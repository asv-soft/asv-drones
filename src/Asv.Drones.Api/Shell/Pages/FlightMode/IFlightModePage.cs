using Asv.Avalonia;
using Asv.Avalonia.GeoMap;
using ObservableCollections;

namespace Asv.Drones.Api;

public interface IFlightModePage : IPage
{
    ObservableList<IFlightWidget> Widgets { get; }
    IMap Map { get; }
}
