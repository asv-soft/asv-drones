using System.ComponentModel.Composition;
using Asv.Drones.Gui.Core;
using Asv.Mavlink;
using Asv.Mavlink.V2.Ardupilotmega;
using Avalonia.Controls;
using Material.Icons;
using ReactiveUI.Fody.Helpers;

namespace Asv.Drones.Gui.Uav;

public class VehicleModeWithIcons
{
    public IVehicleMode Mode { get; set; }
    public MaterialIconKind Icon { get; set; }

    public VehicleModeWithIcons(IVehicleMode mode)
    {
        Mode = mode;

        Icon = Mode.Name switch
        {
            #region Plane modes
            "Manual" => MaterialIconKind.CarManualTransmission,
            "Training" => MaterialIconKind.AirplaneTrain,
            "Fly by wire A" => MaterialIconKind.Butterfly,
            "Fly by wire B" => MaterialIconKind.ButterflyOutline,
            "Cruise" => MaterialIconKind.ChartLine,
            "Autotune"  => MaterialIconKind.Automatic,
            "Quad Stabilize" => MaterialIconKind.VideoStabilization,
            "Quad Hover" => MaterialIconKind.Helicopter,
            "Quad Loiter" => MaterialIconKind.CrosshairsGps,
            "Quad Land" => MaterialIconKind.FlightLand,
            "Quad RTL" => MaterialIconKind.Launch,
            "Qautotune" => MaterialIconKind.Autonomous,
            #endregion

            #region Copter modes
            "Stabilize" => MaterialIconKind.VideoStabilisation,
            "Acro" => MaterialIconKind.WifiArrowUpDown,
            "AltHold" => MaterialIconKind.ArrowUpDown,
            "Auto" => MaterialIconKind.Automatic,
            "Guided" => MaterialIconKind.GpsFixed,
            "Loiter" => MaterialIconKind.CrosshairsGps,
            "RTL" => MaterialIconKind.HomeCircleOutline,
            "Circle" => MaterialIconKind.Circle,
            "Land" => MaterialIconKind.FlightLand,
            "Drift" => MaterialIconKind.VideoStabilization,
            "Sport" => MaterialIconKind.CarSports,
            "Flip" => MaterialIconKind.FlipHorizontal,
            "AutoTune"  => MaterialIconKind.Automatic,
            "PosHold" => MaterialIconKind.GestureTapHold,
            "Brake" => MaterialIconKind.CarBrakeHold,
            "Throw" => MaterialIconKind.ArrowUpBold,
            "SmartRtl" => MaterialIconKind.RocketLaunch,
            "GuidedNoGps" => MaterialIconKind.GpsUnknown,
            #endregion
            _ => MaterialIconKind.About
        };
    }
}

[ExportShellPage(UriString)]
[PartCreationPolicy(CreationPolicy.Shared)]
public class SelectModeViewModel : ViewModelBase
{
    public const string UriString = UavAnchor.BaseUriString + ".actions.selectmode";

    public SelectModeViewModel() : base(new Uri(UriString))
    {
        if (Design.IsDesignMode)
        {
            AvailableModes.Add(new VehicleModeWithIcons(new ArdupilotCopterMode("Stabilize", "Stabilize vehicle", CopterMode.CopterModeStabilize)));
            AvailableModes.Add(new VehicleModeWithIcons(new ArdupilotCopterMode("Acro", "Perform 360 roll", CopterMode.CopterModeAcro)));
            AvailableModes.Add(new VehicleModeWithIcons(new ArdupilotCopterMode("AltHold", "Hold altitude", CopterMode.CopterModeAltHold)));
            AvailableModes.Add(new VehicleModeWithIcons(new ArdupilotCopterMode("Auto", "Stabilize vehicle", CopterMode.CopterModeStabilize)));
            AvailableModes.Add(new VehicleModeWithIcons(new ArdupilotCopterMode("Guided", "Perform 360 roll", CopterMode.CopterModeAcro)));
            AvailableModes.Add(new VehicleModeWithIcons(new ArdupilotCopterMode("RTL", "Hold altitude", CopterMode.CopterModeAltHold)));
            AvailableModes.Add(new VehicleModeWithIcons(new ArdupilotCopterMode("Circle", "Stabilize vehicle", CopterMode.CopterModeStabilize)));
            AvailableModes.Add(new VehicleModeWithIcons(new ArdupilotCopterMode("Land", "Perform 360 roll", CopterMode.CopterModeAcro)));
            AvailableModes.Add(new VehicleModeWithIcons(new ArdupilotCopterMode("Drift", "Hold altitude", CopterMode.CopterModeAltHold)));
            AvailableModes.Add(new VehicleModeWithIcons(new ArdupilotCopterMode("Sport", "Hold altitude", CopterMode.CopterModeAltHold)));
            AvailableModes.Add(new VehicleModeWithIcons(new ArdupilotCopterMode("Flip", "Hold altitude", CopterMode.CopterModeAltHold)));
            AvailableModes.Add(new VehicleModeWithIcons(new ArdupilotCopterMode("AutoTune", "Hold altitude", CopterMode.CopterModeAltHold)));
            AvailableModes.Add(new VehicleModeWithIcons(new ArdupilotCopterMode("PosHold", "Hold altitude", CopterMode.CopterModeAltHold)));
            AvailableModes.Add(new VehicleModeWithIcons(new ArdupilotCopterMode("Brake", "Hold altitude", CopterMode.CopterModeAltHold)));
            AvailableModes.Add(new VehicleModeWithIcons(new ArdupilotCopterMode("Throw", "Hold altitude", CopterMode.CopterModeAltHold)));
            AvailableModes.Add(new VehicleModeWithIcons(new ArdupilotCopterMode("SmartRtl", "Hold altitude", CopterMode.CopterModeAltHold)));
        }
    }
    
    [ImportingConstructor]
    public SelectModeViewModel(IVehicleClient vehicle) : this()
    {
        foreach (var availableMode in vehicle.AvailableModes)
        {
            if (availableMode.InternalMode) continue;
            
            AvailableModes.Add(new VehicleModeWithIcons(availableMode));

            if (availableMode == vehicle.CurrentMode.Value)
            {
                SelectedMode = AvailableModes.Last();
            }
        }
    }

    public List<VehicleModeWithIcons> AvailableModes { get; set; } = new ();
    [Reactive]
    public VehicleModeWithIcons SelectedMode { get; set; }
}