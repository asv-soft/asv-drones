using Asv.Avalonia;
using ObservableCollections;

namespace Asv.Drones.Api;

public interface IMenuActionTarget
{
    ObservableList<IMenuItem> Menu { get; }
}
