using Asv.Avalonia;
using Asv.Avalonia.GeoMap;
using ObservableCollections;
using R3;

namespace Asv.Drones.Api;

public interface IFlightMode : IPage
{
    ObservableList<IUavFlightWidget> Widgets { get; }
    IMap MapViewModel { get; }
}
