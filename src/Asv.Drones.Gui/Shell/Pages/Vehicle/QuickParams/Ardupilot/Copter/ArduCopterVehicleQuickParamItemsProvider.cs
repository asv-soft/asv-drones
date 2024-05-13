using System.Collections.Generic;
using System.Composition;
using Asv.Drones.Gui.Api;
using DynamicData;

namespace Asv.Drones.Gui;

[Export(WellKnownUri.ShellPageQuickParamsArduCopterVehicle, typeof(IViewModelProvider<ITreePageMenuItem>))]
[Shared]
public class ArduCopterVehicleQuickParamItemsProvider : ViewModelProviderBase<ITreePageMenuItem>
{
    [ImportingConstructor]
    public ArduCopterVehicleQuickParamItemsProvider(
        [ImportMany(WellKnownUri.ShellPageQuickParamsArduCopterVehicle)] IEnumerable<ITreePageMenuItem> items)
    {
        Source.AddOrUpdate(items);
    }
}