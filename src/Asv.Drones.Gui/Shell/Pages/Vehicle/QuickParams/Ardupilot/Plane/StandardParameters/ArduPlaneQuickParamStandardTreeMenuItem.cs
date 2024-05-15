using System;
using System.Composition;
using Asv.Drones.Gui.Api;
using Material.Icons;

namespace Asv.Drones.Gui;

[Export(WellKnownUri.ShellPageQuickParamsArduPlaneVehicle, typeof(ITreePageMenuItem))]
[method: ImportingConstructor]
public class ArduPlaneQuickParamStandardTreeMenuItem(IMavlinkDevicesService svc) : TreePageMenuItem(WellKnownUri.ShellPageQuickParamsArduPlaneVehicleStandard)
{
    public override int Order => 500;
    public override Uri ParentId => WellKnownUri.UndefinedUri;
    public override MaterialIconKind Icon => MaterialIconKind.Wrench;
    public override string? Name => RS.VehicleQuickParamStandardTreeMenuItem_Name;
    public override string? Description => RS.VehicleQuickParamStandardTreeMenuItem_Description;
    public override ITreePage? CreatePage(ITreePageContext context)
    {
        return new ArduPlaneQuickParamStandardTreePageViewModel(context);
    }
}