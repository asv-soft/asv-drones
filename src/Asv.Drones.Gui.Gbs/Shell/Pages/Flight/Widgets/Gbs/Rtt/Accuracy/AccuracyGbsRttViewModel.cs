using Asv.Common;
using Asv.Drones.Gui.Core;
using ReactiveUI.Fody.Helpers;

namespace Asv.Drones.Gui.Gbs;

public class AccuracyGbsRttViewModel: GbsRttItem
{
    public AccuracyGbsRttViewModel()
    {
        Accuracy = "5 m";
    }

    public AccuracyGbsRttViewModel(IGbsDevice gbs, ILocalizationService localizationService) : base(gbs, GenerateUri(gbs,"accuracy"))
    {
        Order = 1;
        Gbs.DeviceClient.AccuracyMeter
            .Subscribe(_ => Accuracy = localizationService.Distance.FromSiToStringWithUnits(_))
            .DisposeItWith(Disposable);
    }
    
    [Reactive]
    public string Accuracy { get; set; } = RS.GbsRttItem_ValueNotAvailable;
   
}