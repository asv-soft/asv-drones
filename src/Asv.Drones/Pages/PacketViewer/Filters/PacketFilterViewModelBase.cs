using System;
using System.Threading;
using Asv.Avalonia;
using Asv.Common;
using Microsoft.Extensions.Logging;
using R3;

namespace Asv.Drones;

public abstract class PacketFilterViewModelBase<TFilter> : RoutableViewModel
    where TFilter : PacketFilterViewModelBase<TFilter>
{
    public static string BaseId => $"packet-filter.{typeof(TFilter).Name}";
    private const int BaseMovingAverageSize = 3;

    private readonly IUnit _unit;
    private readonly IncrementalRateCounter _packetRate = new(BaseMovingAverageSize);
    private volatile int _cnt;

    public abstract BindableReactiveProperty<string> FilterValue { get; }

    public BindableReactiveProperty<string> MessageRateText { get; }
    public BindableReactiveProperty<bool> IsChecked { get; }

    public PacketFilterViewModelBase(
        string idArg,
        IUnitService unitService,
        ILoggerFactory loggerFactory
    )
        : base(new NavigationId(BaseId, idArg), loggerFactory)
    {
        _unit = unitService.Units[FrequencyBase.Id];
        MessageRateText = new BindableReactiveProperty<string>(string.Empty).DisposeItWith(
            Disposable
        );
        IsChecked = new BindableReactiveProperty<bool>(false).DisposeItWith(Disposable);
        IncreaseRatesCounterSafe();
        UpdateRateText();

        IsChecked.Value = true;

        Observable
            .Timer(TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1))
            .Subscribe(_ => UpdateRateText())
            .DisposeItWith(Disposable);
    }

    public void IncreaseRatesCounterSafe()
    {
        Interlocked.Increment(ref _cnt);
    }

    private void UpdateRateText()
    {
        var packetRate = Math.Round(_packetRate.Calculate(_cnt), 1);
        MessageRateText.Value = _unit.CurrentUnitItem.Value.PrintWithUnits(packetRate, "F1");
    }
}
