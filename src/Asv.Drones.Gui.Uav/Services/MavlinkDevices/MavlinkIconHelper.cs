using Asv.Mavlink.V2.Common;
using Material.Icons;

namespace Asv.Drones.Gui.Uav
{
    public static class MavlinkIconHelper
    {
        public static MaterialIconKind GetIcon(MavType type)
        {
            switch (type)
            {
                case MavType.MavTypeFixedWing:
                    return MaterialIconKind.Airplane;
                case MavType.MavTypeGeneric:
                case MavType.MavTypeQuadrotor:
                case MavType.MavTypeHexarotor:
                case MavType.MavTypeOctorotor:
                case MavType.MavTypeTricopter:
                    return MaterialIconKind.Quadcopter;
                case MavType.MavTypeHelicopter:
                    return MaterialIconKind.Helicopter;
                case MavType.MavTypeAntennaTracker:
                    return MaterialIconKind.Antenna;
                case MavType.MavTypeGcs:
                    return MaterialIconKind.Computer;
                default:
                    return MaterialIconKind.HelpNetworkOutline;
            }
        }

        public static string GetTypeName(MavType type)
        {
            switch (type)
            {
                case MavType.MavTypeFixedWing:
                    return "Fixed wing";
                case MavType.MavTypeGeneric:
                case MavType.MavTypeQuadrotor:
                    return "Quadrotor";
                case MavType.MavTypeHexarotor:
                    return "Hexarotor";
                case MavType.MavTypeOctorotor:
                    return "Octorotor";
                case MavType.MavTypeTricopter:
                    return "Tricopter";
                case MavType.MavTypeHelicopter:
                    return "Helicopter";
                default:
                    return "Unknown type";
            }
        }
    }
}