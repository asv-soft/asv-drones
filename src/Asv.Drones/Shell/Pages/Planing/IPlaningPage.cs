using Asv.Avalonia;
using Asv.Avalonia.GeoMap;
using ObservableCollections;

namespace Asv.Drones;

public interface IPlaningPage : IPage
{
    IMap Map { get; }
    ObservableList<IWorkspaceWidget> Widgets { get; }
}