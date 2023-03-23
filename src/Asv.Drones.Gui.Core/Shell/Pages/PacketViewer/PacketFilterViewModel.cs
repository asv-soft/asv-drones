using Asv.Common;
using ReactiveUI.Fody.Helpers;

namespace Asv.Drones.Gui.Core;

public class PacketFilterViewModel : ViewModelBase
{
    private volatile int _cnt;
    
    private readonly IncrementalRateCounter _packetRate = new(3);
    private readonly ILocalizationService _localization;
    
    public const string UriString = ShellPage.UriString + ".packetFilter";
    public static readonly Uri Uri = new Uri(UriString);
    
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

    public PacketFilterViewModel() : base(Uri)
    {
    }

    public PacketFilterViewModel(string type, string source, ILocalizationService localizationService) : this()
    {
        _localization = localizationService;
        
        Type = type;
        Source = source;
        IsChecked = true;
    }

    public void UpdateRates()
    {
        Interlocked.Increment(ref _cnt);

        var packetRate = _packetRate.Calculate(_cnt);
        MessageRateText = _localization.ItemsRate.ConvertToString(packetRate);
        MessageRateUnitText = _localization.ItemsRate.GetUnit(packetRate);
        
        Interlocked.Exchange(ref _cnt, _cnt);
    }
}