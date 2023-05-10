using System.ComponentModel.Composition;
using Asv.Mavlink;
using Asv.Mavlink.V2.AsvSdr;

namespace Asv.Drones.Gui.Sdr;

[Export(typeof(ISdrRttItem))]
[PartCreationPolicy(CreationPolicy.NonShared)]
public class SdrRttItemLlzSdmViewModel : SdrRttItemLlzViewModel
{
    public SdrRttItemLlzSdmViewModel(ISdrClientDevice device) : base(device, "total/sdm")
    {
    }

    public override double GetValue(AsvSdrRecordDataLlzPayload payload)
    {
        return (payload.TotalAm150 + payload.TotalAm90)*100.0;
    }
    
    public override string Title => "CRS+CLR";
    public override string Units => "% SDM";
    public override string FormatString => "F2";
}

[Export(typeof(ISdrRttItem))]
[PartCreationPolicy(CreationPolicy.NonShared)]
public class SdrRttItemLlzCrsSdmViewModel : SdrRttItemLlzViewModel
{
    public SdrRttItemLlzCrsSdmViewModel(ISdrClientDevice device) : base(device, "crs/sdm")
    {
    }

    public override double GetValue(AsvSdrRecordDataLlzPayload payload)
    {
        return (payload.CrsAm150 + payload.CrsAm90)*100.0;
    }
    
    public override string Title => "CRS";
    public override string Units => "% SDM";
    public override string FormatString => "F2";
}

[Export(typeof(ISdrRttItem))]
[PartCreationPolicy(CreationPolicy.NonShared)]
public class SdrRttItemLlzClrSdmViewModel : SdrRttItemLlzViewModel
{
    public SdrRttItemLlzClrSdmViewModel(ISdrClientDevice device) : base(device, "clr/sdm")
    {
    }

    public override double GetValue(AsvSdrRecordDataLlzPayload payload)
    {
        return (payload.ClrAm150 + payload.ClrAm90)*100.0;
    }
    
    public override string Title => "CLR";
    public override string Units => "% SDM";
    public override string FormatString => "F2";
}