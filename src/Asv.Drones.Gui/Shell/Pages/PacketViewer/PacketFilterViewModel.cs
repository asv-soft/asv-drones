using System;
using System.Threading;
using Asv.Common;
using Asv.Drones.Gui.Api;
using ReactiveUI.Fody.Helpers;

namespace Asv.Drones.Gui;

public class PacketFilterViewModel : DisposableReactiveObject
{
    private volatile int _cnt;

    private readonly IncrementalRateCounter _packetRate = new(3);
    private readonly ILocalizationService _localization;
    //public static Uri GenerateUri(string filterId) => new($"{WellKnownUri.ShellPagePacketViewer}.filter.{filterId}");


    [Reactive] public string Type { get; set; }
    [Reactive] public string Source { get; set; }
    [Reactive] public string MessageRateText { get; set; }

    [Reactive] public bool IsChecked { get; set; }

    public PacketFilterViewModel()
    {
        DesignTime.ThrowIfNotDesignMode();
        UpdateRates();
    }

    public PacketFilterViewModel(PacketMessageViewModel pkt, ILocalizationService localizationService)
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
        var packetRate = Math.Round(_packetRate.Calculate(_cnt), 1);
        MessageRateText = _localization.ItemsRate.ConvertToStringWithUnits(packetRate);
    }
}