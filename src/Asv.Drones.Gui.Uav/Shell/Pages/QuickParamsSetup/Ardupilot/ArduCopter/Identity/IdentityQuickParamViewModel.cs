using System.ComponentModel.Composition;
using System.Reactive.Linq;
using Asv.Common;
using Asv.Drones.Gui.Core;
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
    private readonly IVehicleClient _vehicle;
    private readonly ILogService _log;

    public override int Order => 0;
    
    public override bool IsRebootRequired => false;

    public override bool IsVisibleInAdvancedMode => false;

    
    [ImportingConstructor]
    public IdentityQuickParamViewModel(IVehicleClient vehicle, ILogService log) : base(Uri)
    {
        _vehicle = vehicle;
        _log = log;
    }
    
    public byte[] Ids { get; } = Enumerable.Range(1, 254).Select(_ => (byte)_).ToArray();
    
    [Reactive]
    public byte SystemId { get; set; }
    
    public override async Task Write()
    {
        try
        {
            SystemId = await _vehicle.Params.WriteOnce("SYSID_THISMAV", SystemId);
        }
        catch (Exception e)
        {
            _log.Error("IdentityQuickParam", e.Message, e);
        }
    }

    public override async Task Read()
    {
        try
        {
            SystemId = await _vehicle.Params.ReadOnce("SYSID_THISMAV");
        
            this.WhenValueChanged(_ => _.SystemId)
                .Subscribe(_ =>
                {
                    SyncCheck(_);
                }).DisposeItWith(Disposable);
        }
        catch (Exception e)
        {
            _log.Error("SpeedsQuickParam", e.Message, e);
        }
    }

    public async Task SyncCheck(byte systemId)
    {
        IsSynced = systemId == (byte) await _vehicle.Params.ReadOnce("SYSID_THISMAV");
    }
}