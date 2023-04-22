using System.Reactive.Linq;
using Asv.Common;
using Asv.Drones.Gui.Core;
using Asv.Mavlink;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Asv.Drones.Gui.Gbs;

public class DGpsRateGbsRttViewModel : GbsRttItem
{
    public DGpsRateGbsRttViewModel()
    {
        DGpsRate = "30Kb/s";
    }

    public DGpsRateGbsRttViewModel(IGbsClientDevice baseStation, ILocalizationService localizationService) : base(baseStation, GenerateUri(baseStation,"dgpsrate"))
    {
        Order = 2;

        BaseStation.Gbs.DgpsRate
            .ObserveOn(RxApp.MainThreadScheduler)
            .Subscribe(_ => DGpsRate = localizationService.ByteRate.ConvertToStringWithUnits(_))
            .DisposeItWith(Disposable);
    }
    
    [Reactive]
    public string DGpsRate { get; set; } = RS.GbsRttItem_ValueNotAvailable;
}