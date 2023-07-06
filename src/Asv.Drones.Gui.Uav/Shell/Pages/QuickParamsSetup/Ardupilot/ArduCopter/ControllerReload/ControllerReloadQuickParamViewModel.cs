using System.ComponentModel.Composition;
using Asv.Mavlink;

namespace Asv.Drones.Gui.Uav;

[Export(typeof(IQuickParamsPart))]
[PartCreationPolicy(CreationPolicy.NonShared)]
public class ControllerReloadQuickParamViewModel : QuickParamsPartBase
{
    private static readonly Uri Uri = new(QuickParamsPartBase.Uri, "controllerreset");
    public override int Order => 2;

    public override bool IsRebootRequired => false;

    public override bool IsVisibleInAdvancedMode => false;

    [ImportingConstructor]
    public ControllerReloadQuickParamViewModel(IVehicleClient vehicle) : base(Uri)
    {
        
    }
    
    public override async Task Write()
    {
        await Task.Delay(100);
    }

    public override async Task Read()
    {
        await Task.Delay(100);
    }
}