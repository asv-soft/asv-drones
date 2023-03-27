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
    public bool IsChecked { get; set; }

    public PacketFilterViewModel() : base(GenerateUri(Guid.NewGuid().ToString()))
    {
        UpdateRates();
    }

    public PacketFilterViewModel(PacketMessageViewModel pkt, ILocalizationService localizationService) 
        : base(GenerateUri(Guid.NewGuid().ToString()))
    {
        _localization = localizationService;
        Type = pkt.Type;
        Source = pkt.Source;
        IsChecked = true;
    }
    public void UpdateRates()
    {
        Interlocked.Increment(ref _cnt);
        
    }

    public void UpdateRateText()
    {
        var packetRate = Math.Round(_packetRate.Calculate(_cnt),1);
        MessageRateText = _localization.ItemsRate.ConvertToStringWithUnits(packetRate);
    }
}