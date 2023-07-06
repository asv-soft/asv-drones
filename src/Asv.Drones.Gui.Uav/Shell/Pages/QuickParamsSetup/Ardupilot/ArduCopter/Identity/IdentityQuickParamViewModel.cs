using System.ComponentModel.Composition;
using System.Reactive.Linq;
using Asv.Common;
using Asv.Mavlink;
using DynamicData.Alias;
using DynamicData.Binding;
using DynamicData.PLinq;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Asv.Drones.Gui.Uav;

[Export(typeof(IQuickParamsPart))]
[PartCreationPolicy(CreationPolicy.NonShared)]
public class IdentityQuickParamViewModel : QuickParamsPartBase
{
    private static readonly Uri Uri = new(QuickParamsPartBase.Uri, "identity");
    private IVehicleClient _vehicle;
    
    public override int Order => 0;
    
    public override bool IsRebootRequired => false;

    public override bool IsVisibleInAdvancedMode => false;

    
    [ImportingConstructor]
    public IdentityQuickParamViewModel(IVehicleClient vehicle) : base(Uri)
    {
        _vehicle = vehicle;
        
        this.WhenValueChanged(_ => _.SystemId)
            .Subscribe(async _ =>
            {
                IsSynced = _ == (byte) await _vehicle.Params.ReadOnce("SYSID_THISMAV");
            }).DisposeItWith(Disposable);
    }
    
    public byte[] Ids { get; } = Enumerable.Range(1, 254).Select(_ => (byte)_).ToArray();
    
    [Reactive]
    public byte SystemId { get; set; }
    
    public override async Task Write()
    {
        SystemId = await _vehicle.Params.WriteOnce("SYSID_THISMAV", SystemId);
    }

    public override async Task Read()
    {
        SystemId = await _vehicle.Params.ReadOnce("SYSID_THISMAV");
    }
}