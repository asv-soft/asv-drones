using System.Collections.Generic;
using System.Composition;
using Asv.Drones.Gui.Api;
using DynamicData;

namespace Asv.Drones.Gui;

[Export(WellKnownUri.ShellPageQuickParamsArduPlaneVehicle, typeof(IViewModelProvider<ITreePageMenuItem>))]
[Shared]
public class ArduPlaneVehicleQuickParamItemsProvider : ViewModelProviderBase<ITreePageMenuItem>
{
    [ImportingConstructor]
    public ArduPlaneVehicleQuickParamItemsProvider([ImportMany(WellKnownUri.ShellPageQuickParamsArduPlaneVehicle)] IEnumerable<ITreePageMenuItem> items)
    {
        Source.AddOrUpdate(items);
    }
}