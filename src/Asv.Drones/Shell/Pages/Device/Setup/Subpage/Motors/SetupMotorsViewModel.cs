using Asv.Avalonia;
using Asv.Common;
using Asv.Drones.Api;
using Asv.IO;
using Asv.Mavlink;
using Asv.Modeling;
using Material.Icons;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using ObservableCollections;
using R3;

namespace Asv.Drones;

public sealed class SetupMotorsViewModel : SetupSubpage
{
    public const string PageId = "motorTest";
    public const MaterialIconKind Icon = MaterialIconKind.Motor;

    private const int DefaultTestDurationInSeconds = 3;

    private readonly ReactiveProperty<double> _duration;
    private readonly SynchronizedReactiveProperty<bool> _isEnabled;
    private readonly ISynchronizedView<ITestMotor, MotorItemViewModel> _motorsView;

    public SetupMotorsViewModel()
        : this(
            DesignTimeSetupSubPageContext.Instance,
            NullLoggerFactory.Instance,
            CreateDesignTimeUnitService(),
            new ObservableList<ITestMotor>
            {
                new NullTestMotor(1, 1, true, 1100),
                new NullTestMotor(2, 2, false, 1500),
                new NullTestMotor(3, 3, true, 1900),
                new NullTestMotor(4, 4, false, 1000),
            }
        )
    {
        DesignTime.ThrowIfNotDesignMode();
    }

    public SetupMotorsViewModel(
        ITreeSubPageContext<ISetupPage> context,
        ILoggerFactory loggerFactory,
        IUnitService unitService
    )
        : this(
            context,
            loggerFactory,
            unitService,
            context
                .Context.Target.CurrentValue?.Device.GetMicroservice<IMotorTestClient>()
                ?.TestMotors
            ?? throw new Exception($"{nameof(IMotorTestClient)} should not be null")
        ) { }

    private SetupMotorsViewModel(
        ITreeSubPageContext<ISetupPage> context,
        ILoggerFactory loggerFactory,
        IUnitService unitService,
        IReadOnlyObservableList<ITestMotor> motors
    )
        : base(PageId, context, loggerFactory)
    {
        _isEnabled = new SynchronizedReactiveProperty<bool>().DisposeItWith(Disposable);
        IsEnabled = _isEnabled.ToBindableReactiveProperty().DisposeItWith(Disposable);

        _duration = new ReactiveProperty<double>(DefaultTestDurationInSeconds).DisposeItWith(
            Disposable
        );
        Duration = new HistoricalUnitProperty(
            nameof(Duration),
            _duration,
            unitService.Units[TimeSpanUnit.Id],
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

        _motorsView = motors
            .CreateView(motor => new MotorItemViewModel(
                motor,
                _duration,
                unitService,
                loggerFactory
            ))
            .DisposeItWith(Disposable);

        _motorsView.SetRoutableParent(this).DisposeItWith(Disposable);
        _motorsView.DisposeMany().DisposeItWith(Disposable);

        Motors = _motorsView
            .ToNotifyCollectionChanged(SynchronizationContextCollectionEventDispatcher.Current)
            .DisposeItWith(Disposable);
    }

    public HistoricalUnitProperty Duration { get; }
    public IReadOnlyBindableReactiveProperty<string> DurationSymbol { get; }

    public BindableReactiveProperty<bool> IsEnabled { get; }

    public INotifyCollectionChangedSynchronizedViewList<MotorItemViewModel> Motors { get; }

    public override IEnumerable<IViewModel> GetChildren()
    {
        foreach (var childRoutable in base.GetChildren())
        {
            yield return childRoutable;
        }

        foreach (var vm in Motors)
        {
            yield return vm;
        }

        yield return Duration;
    }

    private static IUnitService CreateDesignTimeUnitService()
    {
        NullUnitService.Instance.Extend(
            new TimeSpanUnit(DesignTime.Configuration, [new TimeSpanSecondUnitItem()])
        );
        NullUnitService.Instance.Extend(
            new ThrottleUnit(DesignTime.Configuration, [new ThrottlePercentUnitItem()])
        );
        return NullUnitService.Instance;
    }
}
