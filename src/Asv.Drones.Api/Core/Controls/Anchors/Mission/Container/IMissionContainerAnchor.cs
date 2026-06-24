using Asv.Avalonia.GeoMap;
using Asv.IO;
using ObservableCollections;
using R3;

namespace Asv.Drones.Api;

public interface IMissionContainerAnchor : IMapAnchor, IDisposable
{
    DeviceId DeviceId { get; }

    IReadOnlyObservableList<IMissionAnchor> Anchors { get; }

    ReadOnlyReactiveProperty<bool> IsMissionVisible { get; }

    ReadOnlyReactiveProperty<bool> IsAnchorsVisible { get; }

    ReadOnlyReactiveProperty<bool> IsPathVisible { get; }

    void SwitchAllVisibility();

    void SwitchAnchorsVisibility();

    void SwitchPathVisibility();
}
