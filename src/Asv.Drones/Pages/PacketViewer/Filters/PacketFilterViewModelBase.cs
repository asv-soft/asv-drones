using System;
using System.Collections.Generic;
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
    private readonly ReactiveProperty<bool> _isChecked;
    private readonly ReactiveProperty<double> _messageRate;
    private readonly IncrementalRateCounter _packetRate = new(BaseMovingAverageSize);
    private volatile int _cnt;

    public abstract string FilterValue { get; }

    public IReadOnlyBindableReactiveProperty<string> MessageRateTextUnit { get; }
    public HistoricalUnitProperty MessageRateText { get; }
    public HistoricalBoolProperty IsChecked { get; }

    protected PacketFilterViewModelBase(
        string idArg,
        IUnitService unitService,
        ILoggerFactory loggerFactory
    )
        : base(new NavigationId(BaseId, idArg), loggerFactory)
    {
        _unit = unitService.Units[FrequencyBase.Id];
        _isChecked = new ReactiveProperty<bool>(true).DisposeItWith(Disposable);
        _messageRate = new ReactiveProperty<double>().DisposeItWith(Disposable);
        MessageRateText = new HistoricalUnitProperty(
            nameof(MessageRateText),
            _messageRate,
            _unit,
            loggerFactory,
            "F1"
        )
        {
            Parent = this,
            
        }.DisposeItWith(Disposable);
        IsChecked = new HistoricalBoolProperty(
            nameof(IsChecked),
            _isChecked,
            loggerFactory
        ) {
            Parent = this,
            
        }.DisposeItWith(Disposable);
        MessageRateTextUnit = MessageRateText
            .Unit.CurrentUnitItem.Select(item => item.Symbol)
            .ToBindableReactiveProperty<string>()
            .DisposeItWith(Disposable);
        IncreaseRatesCounterSafe();

        Observable
            .Timer(TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1))
            .Subscribe(_ => UpdateRateText())
            .DisposeItWith(Disposable);
    }

    public void IncreaseRatesCounterSafe()
    {
        Interlocked.Increment(ref _cnt);
    }

    public override IEnumerable<IRoutable> GetRoutableChildren()
    {
        yield return IsChecked;
    }

    private void UpdateRateText()
    {
        MessageRateText.ModelValue.Value = Math.Round(_packetRate.Calculate(_cnt), 1);
    }
}
