using System;
using System.Collections.Generic;
using System.Composition;
using System.Threading.Tasks;
using Asv.Avalonia;
using Asv.Common;
using Asv.Drones.Api;
using Asv.IO;
using Asv.Mavlink;
using Material.Icons;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using ObservableCollections;
using R3;

namespace Asv.Drones;

[ExportSetup(PageId)]
public sealed class SetupMotorsViewModel : SetupSubpage
{
    public const string PageId = "motor-test";
    public const MaterialIconKind Icon = MaterialIconKind.Motor;
    private const int DefaultTestDurationInSeconds = 3;
    private readonly ILoggerFactory _loggerFactory;
    private readonly IUnitService _unit;
    private readonly ReactiveProperty<double> _duration;
    private readonly SynchronizedReactiveProperty<bool> _isEnabled;

    private ISynchronizedView<ITestMotor, MotorItemViewModel>? _motorsView;
    private IMotorTestClient? _motorTestClient;

    public SetupMotorsViewModel()
        : this(NullLoggerFactory.Instance, NullUnitService.Instance) { }

    [ImportingConstructor]
    public SetupMotorsViewModel(ILoggerFactory loggerFactory, IUnitService unit)
        : base(PageId, loggerFactory)
    {
        _loggerFactory = loggerFactory;
        _unit = unit;

        _isEnabled = new SynchronizedReactiveProperty<bool>().DisposeItWith(Disposable);
        IsEnabled = _isEnabled.ToBindableReactiveProperty().DisposeItWith(Disposable);

        _duration = new ReactiveProperty<double>(DefaultTestDurationInSeconds).DisposeItWith(
            Disposable
        );
        Duration = new HistoricalUnitProperty(
            nameof(Duration),
            _duration,
            unit.Units[TimeSpanBase.Id],
            loggerFactory
        )
            .SetRoutableParent(this)
            .DisposeItWith(Disposable);

        Duration
            .ViewValue.Subscribe(_ => _isEnabled.Value = !Duration.ViewValue.HasErrors)
            .DisposeItWith(Disposable);

        DurationSymbol = Duration
            .Unit.CurrentUnitItem.Select(item => item.Symbol)
            .ToReadOnlyBindableReactiveProperty<string>()
            .DisposeItWith(Disposable);
    }

    public HistoricalUnitProperty Duration { get; }
    public IReadOnlyBindableReactiveProperty<string> DurationSymbol { get; }

    public BindableReactiveProperty<bool> IsEnabled { get; }

    public INotifyCollectionChangedSynchronizedViewList<MotorItemViewModel>? Motors
    {
        get;
        private set;
    }

    public override ValueTask Init(ISetupPage context)
    {
        _motorTestClient =
            context.Target.CurrentValue?.Device.GetMicroservice<IMotorTestClient>()
            ?? throw new Exception($"{nameof(IMotorTestClient)} should not be null");

        _motorsView = _motorTestClient
            .TestMotors.CreateView(motor => new MotorItemViewModel(
                motor,
                _duration,
                _unit,
                _loggerFactory
            ))
            .DisposeItWith(Disposable);

        _motorsView.SetRoutableParent(this).DisposeItWith(Disposable);
        _motorsView.DisposeMany().DisposeItWith(Disposable);

        Motors = _motorsView
            .ToNotifyCollectionChanged(SynchronizationContextCollectionEventDispatcher.Current)
            .DisposeItWith(Disposable);

        return ValueTask.CompletedTask;
    }

    public override IEnumerable<IRoutable> GetRoutableChildren()
    {
        foreach (var childRoutable in base.GetRoutableChildren())
        {
            yield return childRoutable;
        }

        if (Motors is null)
        {
            yield break;
        }

        foreach (var vm in Motors)
        {
            yield return vm;
        }

        yield return Duration;
    }

    public override IExportInfo Source => SystemModule.Instance;
}
