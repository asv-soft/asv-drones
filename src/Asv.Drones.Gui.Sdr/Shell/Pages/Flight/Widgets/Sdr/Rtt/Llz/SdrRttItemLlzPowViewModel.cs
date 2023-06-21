using System.ComponentModel.Composition;
using Asv.Mavlink;
using Asv.Mavlink.V2.AsvSdr;

namespace Asv.Drones.Gui.Sdr;

[Export(typeof(ISdrRttItem))]
[PartCreationPolicy(CreationPolicy.NonShared)]
public class SdrRttItemLlzPowViewModel : SdrRttItemLlzViewModel
{
    public SdrRttItemLlzPowViewModel(ISdrClientDevice device) : base(device, "total/pow")
    {
    }

    public override double GetValue(AsvSdrRecordDataLlzPayload payload)
    {
        return payload.TotalPower;
    }
    
    public override string Title => "Pow";
    public override string Units => "dBm";
    public override string FormatString => "F0";
}
