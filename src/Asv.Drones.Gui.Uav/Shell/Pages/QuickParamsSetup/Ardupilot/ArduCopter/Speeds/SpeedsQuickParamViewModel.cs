using System.ComponentModel.Composition;
using Asv.Common;
using Asv.Mavlink;
using DynamicData.Binding;
using ReactiveUI.Fody.Helpers;

namespace Asv.Drones.Gui.Uav;


[Export(typeof(IQuickParamsPart))]
[PartCreationPolicy(CreationPolicy.NonShared)]
public class SpeedsQuickParamViewModel : QuickParamsPartBase
{
    private static readonly Uri Uri = new(QuickParamsPartBase.Uri, "velocity");
    public override int Order => 1;

    public override bool IsRebootRequired => false;

    public override bool IsVisibleInAdvancedMode => false;
    
    [ImportingConstructor]
    public SpeedsQuickParamViewModel(IVehicleClient vehicle) : base(Uri)
    {
        // WPNAV_SPEED in auto and guided mode
        // LOIT_SPEED in loiter mode
        
    }
    
    [Reactive]
    public float Velocity { get; set; }
    
    public override async Task Write()
    {
        await Task.Delay(100);
    }

    public override async Task Read()
    {
        await Task.Delay(100);
    }
}