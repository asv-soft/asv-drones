using System;
using System.Composition;
using Asv.Drones.Gui.Api;
using Material.Icons;

namespace Asv.Drones.Gui;

[Export(WellKnownUri.ShellPageQuickParamsArduCopterVehicle, typeof(ITreePageMenuItem))]
[method: ImportingConstructor]
public class ArduCopterQuickParamStandardTreeMenuItem(IMavlinkDevicesService svc) : TreePageMenuItem(WellKnownUri.ShellPageQuickParamsArduCopterVehicleStandard)
{
    public override int Order => 500;
    public override Uri ParentId => WellKnownUri.UndefinedUri;
    public override MaterialIconKind Icon => MaterialIconKind.Wrench;
    public override string? Name => RS.VehicleQuickParamStandardTreeMenuItem_Name;
    public override string? Description => RS.VehicleQuickParamStandardTreeMenuItem_Description;
    public override ITreePage? CreatePage(ITreePageContext context)
    {
        return new ArduCopterQuickParamStandardTreePageViewModel(context);
    }
}