using System.Reactive.Linq;
using Asv.Common;
using Asv.Drones.Gui.Core;
using Asv.Mavlink;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Asv.Drones.Gui.Gbs;

public class AccuracyGbsRttViewModel: GbsRttItem
{
    public AccuracyGbsRttViewModel()
    {
        Accuracy = "5 m";
    }

    public AccuracyGbsRttViewModel(IGbsClientDevice baseStation, ILocalizationService localizationService) : base(baseStation, GenerateUri(baseStation,"accuracy"))
    {
        Order = 1;
        BaseStation.Gbs.AccuracyMeter
            .ObserveOn(RxApp.MainThreadScheduler)
            .Subscribe(_ => Accuracy = localizationService.Distance.FromSiToStringWithUnits(_))
            .DisposeItWith(Disposable);
    }
    
    [Reactive]
    public string Accuracy { get; set; } = RS.GbsRttItem_ValueNotAvailable;
   
}