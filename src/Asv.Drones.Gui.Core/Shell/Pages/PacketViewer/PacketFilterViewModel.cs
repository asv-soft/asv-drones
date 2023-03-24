using Asv.Common;
using ReactiveUI.Fody.Helpers;

namespace Asv.Drones.Gui.Core;

public class PacketFilterViewModel : ViewModelBase
{
    private volatile int _cnt;
    
    private readonly IncrementalRateCounter _packetRate = new(3);
    private readonly ILocalizationService _localization;
    public static Uri GenerateUri(string filterId) => new($"{PacketViewerViewModel.UriString}.filter.{filterId}");
   
    
    [Reactive]
    public string Type { get; set; }
    [Reactive]
    public string Source { get; set; }
    [Reactive]
    public string MessageRateText { get; set; }
    [Reactive]
    public string MessageRateUnitText { get; set; }
    [Reactive]
    public bool IsChecked { get; set; }

    public PacketFilterViewModel() : base(GenerateUri(Guid.NewGuid().ToString()))
    {
        UpdateRates();
    }

    public PacketFilterViewModel(PacketMessageViewModel pkt,ILocalizationService localizationService) 
        : base(GenerateUri(pkt.FilterId))
    {
        _localization = localizationService;
        Id = pkt.FilterId;
        Type = pkt.Type;
        Source = pkt.Source;
        IsChecked = true;
    }


    public string Id { get; set; }

    public void UpdateRates()
    {
        Interlocked.Increment(ref _cnt);

        var packetRate = _packetRate.Calculate(_cnt);
        MessageRateText = _localization.ItemsRate.ConvertToString(packetRate);
        MessageRateUnitText = _localization.ItemsRate.GetUnit(packetRate);
    }
}