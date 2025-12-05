using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Asv.Avalonia;
using Asv.Common;
using Asv.Mavlink;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using R3;

namespace Asv.Drones;

public sealed class MotorItemViewModel : RoutableViewModel
{
    public const string BaseId = "motor-item";

    private readonly SynchronizedReactiveProperty<double> _throttle;
    private readonly SynchronizedReactiveProperty<bool> _isEnabled;

    public MotorItemViewModel()
        : base(new NavigationId(BaseId, "1"), NullLoggerFactory.Instance)
    {
        DesignTime.ThrowIfNotDesignMode();

        IsEnabled = new BindableReactiveProperty<bool>(true);
        Pwm = new BindableReactiveProperty<ushort>(1000);
        ServoChannel = 1;
        IsTestIdle = new BindableReactiveProperty<bool>(true);
    }

    public MotorItemViewModel(
        ITestMotor motor,
        ReactiveProperty<int> timeout,
        IUnitService unitService,
        ILoggerFactory loggerFactory
    )
        : base(new NavigationId(BaseId, motor.Id.ToString()), loggerFactory)
    {
        Motor = motor;
        Timeout = timeout;

        _isEnabled = new SynchronizedReactiveProperty<bool>().DisposeItWith(Disposable);
        IsEnabled = _isEnabled.ToBindableReactiveProperty().DisposeItWith(Disposable);

        var unit = unitService.Units[ProgressBase.Id];

        _throttle = new SynchronizedReactiveProperty<double>(0).DisposeItWith(Disposable);
        Throttle = new HistoricalUnitProperty(nameof(Throttle), _throttle, unit, loggerFactory)
            .SetRoutableParent(this)
            .DisposeItWith(Disposable);

        Pwm = motor.Pwm.ToBindableReactiveProperty().DisposeItWith(Disposable);
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

        // TODO: Refactor to ThrottleUnit
        Throttle
            .Unit.CurrentUnitItem.Subscribe(item =>
            {
                switch (item.UnitItemId)
                {
                    case PercentProgressUnit.Id:
                        Throttle.ViewValue.EnableValidation(PercentUnitValidate);
                        break;
                    case InPartsProgressUnit.Id:
                        Throttle.ViewValue.EnableValidation(InPartsUnitValidate);
                        break;
                }

                Throttle.ForceValidate();
            })
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

    public ReactiveProperty<int> Timeout { get; }
    public BindableReactiveProperty<bool> IsEnabled { get; }

    public override IEnumerable<IRoutable> GetRoutableChildren()
    {
        yield return Throttle;
    }

    private async Task RunMotorTest(ITestMotor motor, CancellationToken cts)
    {
        if (IsTestIdle.Value)
        {
            await motor.StartTest((int)Throttle.ModelValue.CurrentValue, Timeout.Value, cts);
            return;
        }

        await motor.StopTest(cts);
    }

    private Exception? PercentUnitValidate(string? value)
    {
        if (!double.TryParse(value, out var throttle))
        {
            _isEnabled.Value = false;
            return new NotNumberValidationException();
        }

        if (throttle is < 0 or > 100)
        {
            _isEnabled.Value = false;
            return new ValidationException("Value is not in range");
        }

        _isEnabled.Value = true;

        return null;
    }

    private Exception? InPartsUnitValidate(string? value)
    {
        if (!double.TryParse(value, out var throttle))
        {
            _isEnabled.Value = false;
            return new NotNumberValidationException();
        }

        if (throttle is < 0 or > 1)
        {
            _isEnabled.Value = false;
            return new ValidationException("Value is not in range");
        }

        _isEnabled.Value = true;

        return null;
    }
}
