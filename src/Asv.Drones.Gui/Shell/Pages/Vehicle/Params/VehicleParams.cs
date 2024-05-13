using System;
using System.Composition;
using Asv.Cfg;
using Asv.Common;
using Asv.Drones.Gui.Api;
using Asv.Mavlink;

namespace Asv.Drones.Gui;

[ExportShellPage(WellKnownUri.ShellPageParamsVehicle)]
public class VehicleParamsViewModel : ParamPageViewModel
{
    [ImportingConstructor]
    public VehicleParamsViewModel(IMavlinkDevicesService svc, ILogService log, IConfiguration cfg)
        : base(WellKnownUri.ShellPageParamsVehicle, svc, log, cfg)
    {

    }

    public override IParamsClientEx? GetParamsClient(IMavlinkDevicesService svc, ushort fullId, DeviceClass @class)
    {
        var dev = svc.GetVehicleByFullId(fullId);
        if (dev == null) return null;
        dev.Name.Subscribe(n => Title = n).DisposeItWith(Disposable);
        return dev.Params;
    }
}