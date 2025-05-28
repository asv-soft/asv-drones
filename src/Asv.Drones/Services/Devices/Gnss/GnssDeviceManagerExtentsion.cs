using System.Composition;
using Asv.Avalonia.IO;
using Asv.Gnss;
using Asv.IO;
using Avalonia.Media;
using Material.Icons;

namespace Asv.Drones;

[Export(typeof(IDeviceManagerExtension))]
[Shared]
public class GnssDeviceManagerExtension : IDeviceManagerExtension
{
    public void Configure(IProtocolBuilder builder)
    {
        builder.Features.RegisterEndpointIdTagFeature();
        
        builder.Protocols.RegisterNmeaProtocol();
    }

    public void Configure(IDeviceExplorerBuilder builder)
    {
        builder.Factories.RegisterGnssDevice();
    }

    public bool TryGetIcon(DeviceId id, out MaterialIconKind? icon)
    {
        if (id is GnssDeviceId)
        {
            icon = MaterialIconKind.Satellite;
            return true;
        }

        icon = null;
        return false;
    }

    public bool TryGetDeviceBrush(DeviceId id, out IBrush? brush)
    {
        brush = null;
        return false;
    }
}
