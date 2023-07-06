using System.ComponentModel.Composition;
using Asv.Mavlink;

namespace Asv.Drones.Gui.Uav;

[Export(typeof(IQuickParamsPart))]
[PartCreationPolicy(CreationPolicy.NonShared)]
public class FailSafeQuickParamViewModel : QuickParamsPartBase
{
    private static readonly Uri Uri = new(QuickParamsPartBase.Uri, "failsafe");
    public override int Order => 3;

    public override bool IsRebootRequired => false;

    public override bool IsVisibleInAdvancedMode => true;
    
    [ImportingConstructor]
    public FailSafeQuickParamViewModel(IVehicleClient vehicle) : base(Uri)
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