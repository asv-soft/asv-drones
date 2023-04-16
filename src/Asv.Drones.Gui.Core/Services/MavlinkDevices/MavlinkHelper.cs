using Asv.Mavlink;
using Asv.Mavlink.V2.Common;
using Material.Icons;

namespace Asv.Drones.Gui.Core
{
    public static class MavlinkHelper
    {
        public static string GetTitle(this MavCmd cmd)
        {
            return cmd.ToString("G").Replace("MavCmd", "");
        }
        
        public static MaterialIconKind GetIcon(MavType type)
        {
            return type switch
            {
                MavType.MavTypeFixedWing => MaterialIconKind.Airplane,
                MavType.MavTypeGeneric => MaterialIconKind.Quadcopter,
                MavType.MavTypeQuadrotor => MaterialIconKind.Quadcopter,
                MavType.MavTypeHexarotor => MaterialIconKind.Quadcopter,
                MavType.MavTypeOctorotor => MaterialIconKind.Quadcopter,
                MavType.MavTypeTricopter => MaterialIconKind.Quadcopter,
                MavType.MavTypeHelicopter => MaterialIconKind.Helicopter,
                MavType.MavTypeAntennaTracker => MaterialIconKind.Antenna,
                MavType.MavTypeGcs => MaterialIconKind.Computer,
                _ => MaterialIconKind.HelpNetworkOutline
            };
        }

        public static string GetTypeName(MavType type)
        {
            // TODO: Localize
            return type switch
            {
                MavType.MavTypeFixedWing => "Fixed wing",
                MavType.MavTypeGeneric => "Quadrotor",
                MavType.MavTypeQuadrotor => "Quadrotor",
                MavType.MavTypeHexarotor => "Hexarotor",
                MavType.MavTypeOctorotor => "Octorotor",
                MavType.MavTypeTricopter => "Tricopter",
                MavType.MavTypeHelicopter => "Helicopter",
                _ => "Unknown type"
            };
        }

        public static MaterialIconKind GetIcon(DeviceClass type)
        {
            return type switch
            {
                DeviceClass.Plane => MaterialIconKind.Plane,
                DeviceClass.Copter => MaterialIconKind.Navigation,
                DeviceClass.Unknown => MaterialIconKind.Navigation,
                _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
            };
        }
    }
}