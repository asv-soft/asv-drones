using Asv.IO;
using ObservableCollections;

namespace Asv.Drones;

public interface IDroneFlightWidget : IFlightWidget
{
    ObservableList<IDashboardWidget> DashboardWidgets { get; }

    void Attach(DeviceId deviceId);
}
