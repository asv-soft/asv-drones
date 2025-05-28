using Asv.Avalonia.GeoMap;
using Asv.IO;
using Asv.Mavlink;
using Material.Icons;

namespace Asv.Drones;

public static class DeviceIconMixin
{
    public static MaterialIconKind? GetIcon(DeviceId deviceId)
    {
        return deviceId.DeviceClass switch
        {
            Vehicles.PlaneDeviceClass => MaterialIconKind.Plane,
            Vehicles.CopterDeviceClass => MaterialIconKind.Navigation,
            GbsClientDevice.DeviceClass => MaterialIconKind.RouterWireless,
            _ => null,
        };
    }

    public static HorizontalOffset GetIconCenterX(DeviceId deviceId)
    {
        switch (deviceId.DeviceClass)
        {
            case Vehicles.PlaneDeviceClass:
                return HorizontalOffset.Default;
            case Vehicles.CopterDeviceClass:
                return HorizontalOffset.Default;
            case GbsClientDevice.DeviceClass:
                return HorizontalOffset.Default;
            default:
                return HorizontalOffset.Default;
        }
    }

    public static VerticalOffset GetIconCenterY(DeviceId deviceId)
    {
        switch (deviceId.DeviceClass)
        {
            case Vehicles.PlaneDeviceClass:
                return VerticalOffset.Default;
            case Vehicles.CopterDeviceClass:
                return VerticalOffset.Default;
            case GbsClientDevice.DeviceClass:
                return VerticalOffset.Default;
            default:
                return VerticalOffset.Default;
        }
    }
}
