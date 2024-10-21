using Asv.Mavlink;
using Asv.Mavlink.V2.Common;
using Asv.Mavlink.V2.Minimal;
using Material.Icons;

namespace Asv.Drones.Gui.Api
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
            // DONE: Localize
            return type switch
            {
                MavType.MavTypeFixedWing => RS.MavlinkHelper_GetTypeName_FixedWing,
                MavType.MavTypeGeneric => RS.MavlinkHelper_GetTypeName_QuadRotor,
                MavType.MavTypeQuadrotor => RS.MavlinkHelper_GetTypeName_QuadRotor,
                MavType.MavTypeHexarotor => RS.MavlinkHelper_GetTypeName_HexaRotor,
                MavType.MavTypeOctorotor => RS.MavlinkHelper_GetTypeName_OctoRotor,
                MavType.MavTypeTricopter => RS.MavlinkHelper_GetTypeName_TriCopter,
                MavType.MavTypeHelicopter => RS.MavlinkHelper_GetTypeName_Helicopter,
                _ => RS.MavlinkHelper_GetTypeName_UnknownType
            };
        }

        public static MaterialIconKind GetIcon(DeviceClass type)
        {
            return type switch
            {
                DeviceClass.Plane => MaterialIconKind.Plane,
                DeviceClass.Copter => MaterialIconKind.Navigation,
                DeviceClass.SdrPayload => MaterialIconKind.Radio,
                DeviceClass.GbsRtk => MaterialIconKind.RouterWireless,
                DeviceClass.Adsb => MaterialIconKind.Radar,
                DeviceClass.Rfsa => MaterialIconKind.Waveform,
                DeviceClass.Rsga => MaterialIconKind.CellphoneWireless,
                _ => MaterialIconKind.HelpNetworkOutline,
            };
        }
    }
}