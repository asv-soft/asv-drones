using Asv.Common;
using Asv.Drones.Gui.Core;
using Asv.Mavlink;
using ReactiveUI.Fody.Helpers;

namespace Asv.Drones.Gui.Gbs;

public class DGpsRateGbsRttViewModel : GbsRttItem
{
    public DGpsRateGbsRttViewModel()
    {
        // DONE: Localize
        DGpsRate = string.Format(RS.DGpsRateGbsRttViewModel_DGpsRate,_rateValue);
    }

    public DGpsRateGbsRttViewModel(IGbsClientDevice baseStation, ILocalizationService localizationService) : base(baseStation, GenerateUri(baseStation,"dgpsrate"))
    {
        Order = 2;

        BaseStation.Gbs.DgpsRate
            .Subscribe(_ => DGpsRate = localizationService.ByteRate.ConvertToStringWithUnits(_))
            .DisposeItWith(Disposable);
    }

    private readonly int _rateValue = 30;
    
    [Reactive]
    public string DGpsRate { get; set; } = RS.GbsRttItem_ValueNotAvailable;
}