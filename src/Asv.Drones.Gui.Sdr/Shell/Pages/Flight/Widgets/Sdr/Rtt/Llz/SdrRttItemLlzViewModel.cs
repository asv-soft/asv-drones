using System.ComponentModel.Composition;
using System.Reactive.Linq;
using Asv.Common;
using Asv.Drones.Gui.Core;
using Asv.Mavlink;
using Asv.Mavlink.V2.AsvSdr;
using Avalonia.Controls;
using ReactiveUI.Fody.Helpers;

namespace Asv.Drones.Gui.Sdr;

[Export(typeof(ISdrRttItem))]
[PartCreationPolicy(CreationPolicy.NonShared)]
public class SdrRttItemLlzViewModel:SdrRttItem
{
    public SdrRttItemLlzViewModel()
    {
        if (Design.IsDesignMode)
        {
            Value = 40.02;
        }
    }
    
    [ImportingConstructor]
    public SdrRttItemLlzViewModel(ISdrClientDevice device, ILocalizationService localizationService)
    :base(device, SdrRttItem.GenerateUri(device,"llz"))
    {
        IsVisible = true;
        device.Sdr.Base.OnRecordData.Where(_ => _.MessageId == AsvSdrRecordDataLlzPacket.PacketMessageId)
            .Cast<AsvSdrRecordDataLlzPacket>()
            .Subscribe(_=>Value = (_.Payload.TotalAm90 - _.Payload.TotalAm150)*100.0)
            .DisposeItWith(Disposable);
    }

    public string Title { get; } = "DDM";

    public string Units { get; } = "%";

    [Reactive] 
    public double Value { get; set; } = 0.1;
}