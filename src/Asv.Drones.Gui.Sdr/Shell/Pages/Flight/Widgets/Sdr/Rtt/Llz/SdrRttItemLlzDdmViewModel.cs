using System.ComponentModel.Composition;
using Asv.Mavlink;
using Asv.Mavlink.V2.AsvSdr;

namespace Asv.Drones.Gui.Sdr;

[Export(typeof(ISdrRttItem))]
[PartCreationPolicy(CreationPolicy.NonShared)]
public class SdrRttItemLlzDdmViewModel : SdrRttItemLlzViewModel
{
    public SdrRttItemLlzDdmViewModel(ISdrClientDevice device) : base(device, "total/ddm")
    {
    }

    public override double GetValue(AsvSdrRecordDataLlzPayload payload)
    {
        return (payload.TotalAm150 - payload.TotalAm90)*100.0;
    }

    public override string Title => "CRS+CLR";
    public override string Units => "% DDM";
    public override string FormatString => "F2";
}

[Export(typeof(ISdrRttItem))]
[PartCreationPolicy(CreationPolicy.NonShared)]
public class SdrRttItemLlzCrsDdmViewModel : SdrRttItemLlzViewModel
{
    public SdrRttItemLlzCrsDdmViewModel(ISdrClientDevice device) : base(device, "crs/ddm")
    {
    }

    public override double GetValue(AsvSdrRecordDataLlzPayload payload)
    {
        return (payload.CrsAm150 - payload.CrsAm90)*100.0;
    }

    public override string Title => "CRS";
    public override string Units => "% DDM";
    public override string FormatString => "F2";
}

[Export(typeof(ISdrRttItem))]
[PartCreationPolicy(CreationPolicy.NonShared)]
public class SdrRttItemLlzClrDdmViewModel : SdrRttItemLlzViewModel
{
    public SdrRttItemLlzClrDdmViewModel(ISdrClientDevice device) : base(device, "clr/ddm")
    {
    }

    public override double GetValue(AsvSdrRecordDataLlzPayload payload)
    {
        return (payload.ClrAm150 - payload.ClrAm90)*100.0;
    }

    public override string Title => "CLR";
    public override string Units => "% DDM";
    public override string FormatString => "F2";
}