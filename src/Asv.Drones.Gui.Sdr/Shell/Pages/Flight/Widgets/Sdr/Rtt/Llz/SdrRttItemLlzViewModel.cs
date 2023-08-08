using System.ComponentModel.Composition;
using System.Reactive.Linq;
using Asv.Common;
using Asv.Mavlink;
using Asv.Mavlink.V2.AsvSdr;
using ReactiveUI.Fody.Helpers;

namespace Asv.Drones.Gui.Sdr;


public class SdrRttItemLlzViewModelDesignMock : SdrRttItemLlzViewModel
{
    public SdrRttItemLlzViewModelDesignMock()
    {
        Title = "DDM";
        Units = "%";
        FormatString = "P2";
        Value = 0.5;
    }

    public override double GetValue(AsvSdrRecordDataLlzPayload payload)
    {
        return 0;
    }

    public override string Title { get; }
    public override string Units { get; }
    public override string FormatString { get; }
}


public abstract class SdrRttItemLlzViewModel:SdrRttItem
{
    protected SdrRttItemLlzViewModel()
    {
       
    }
    
    [ImportingConstructor]
    protected SdrRttItemLlzViewModel(ISdrClientDevice device,string name)
    :base(device, SdrRttItem.GenerateUri(device,$"llz/{name}"))
    {
        device.Sdr.Base.OnRecordData.Where(_ => _.MessageId == AsvSdrRecordDataLlzPacket.PacketMessageId)
            .Cast<AsvSdrRecordDataLlzPacket>()
            .Subscribe(_=>Value = GetValue(_.Payload))
            .DisposeItWith(Disposable);
    }

    public abstract double GetValue(AsvSdrRecordDataLlzPayload payload);
    
    public abstract string Title { get; }

    public abstract string Units { get; }

    [Reactive] 
    public double Value { get; set; } = 0.1;

    public abstract string FormatString { get; }
}