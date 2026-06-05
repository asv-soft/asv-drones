using Asv.Avalonia;
using Asv.Common;
using Asv.Mavlink;
using Asv.Modeling;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using R3;

namespace Asv.Drones;

public sealed class MotorItemViewModel : ViewModel
{
    public const string BaseId = "motorItem";

    private readonly SynchronizedReactiveProperty<double> _throttle;
    private readonly SynchronizedReactiveProperty<bool> _isEnabled;

    public MotorItemViewModel()
        : this(
            NullTestMotor.Instance,
            new ReactiveProperty<double>(100),
            CreateDesignTimeUnitService(),
            NullLoggerFactory.Instance
        )
    {
        DesignTime.ThrowIfNotDesignMode();
    }

    public MotorItemViewModel(
        ITestMotor motor,
        ReactiveProperty<double> duration,
        IUnitService unit,
        ILoggerFactory loggerFactory
    )
        : base(BaseId, new NavArgs(("id", motor.Id.ToString())))
    {
        Motor = motor;
        Timeout = duration;

        _isEnabled = new SynchronizedReactiveProperty<bool>().DisposeItWith(Disposable);
        IsEnabled = _isEnabled.ToBindableReactiveProperty().DisposeItWith(Disposable);

        _throttle = new SynchronizedReactiveProperty<double>(0).DisposeItWith(Disposable);
        Throttle = new HistoricalUnitProperty(
            nameof(Throttle),
            _throttle,
            unit.Units[ThrottleUnit.Id],
            loggerFactory
        )
            .SetRoutableParent(this)
            .DisposeItWith(Disposable);

        Throttle
            .ViewValue.Subscribe(_ => _isEnabled.Value = !Throttle.ViewValue.HasErrors)
            .DisposeItWith(Disposable);

        Pwm = motor
            .Pwm.ObserveOnUIThreadDispatcher()
            .ToBindableReactiveProperty()
            .DisposeItWith(Disposable);
        ServoChannel = motor.ServoChannel;
        IsTestIdle = motor
            .IsTestRun.ObserveOnUIThreadDispatcher()
            .Select(x => !x)
            .ToReadOnlyBindableReactiveProperty()
            .DisposeItWith(Disposable);

        ThrottleSymbol = Throttle
            .Unit.CurrentUnitItem.Select(item => item.Symbol)
            .ToReadOnlyBindableReactiveProperty<string>()
            .DisposeItWith(Disposable);

        RunTestCommand = new ReactiveCommand(
            async (_, cts) => await RunMotorTest(motor, cts)
        ).DisposeItWith(Disposable);
    }

    public ITestMotor Motor { get; }
    public BindableReactiveProperty<ushort> Pwm { get; }
    public int ServoChannel { get; }
    public IReadOnlyBindableReactiveProperty<bool> IsTestIdle { get; }
    public ReactiveCommand RunTestCommand { get; }
    public HistoricalUnitProperty Throttle { get; }
    public IReadOnlyBindableReactiveProperty<string> ThrottleSymbol { get; }
    public ReactiveProperty<double> Timeout { get; }
    public BindableReactiveProperty<bool> IsEnabled { get; }

    public override IEnumerable<IViewModel> GetChildren()
    {
        yield return Throttle;
    }

    private static IUnitService CreateDesignTimeUnitService()
    {
        NullUnitService.Instance.Extend(
            new ThrottleUnit(DesignTime.Configuration, [new ThrottlePercentUnitItem()])
        );
        return NullUnitService.Instance;
    }

    private async Task RunMotorTest(ITestMotor motor, CancellationToken cts)
    {
        if (IsTestIdle.Value)
        {
            await motor.StartTest((int)Throttle.ModelValue.CurrentValue, (int)Timeout.Value, cts);
            return;
        }

        await motor.StopTest(cts);
    }
}
