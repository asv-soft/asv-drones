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
    private readonly ReactiveProperty<string?> _duration;
    private readonly SynchronizedReactiveProperty<bool> _isEnabled;
    private readonly SynchronizedReactiveProperty<int> _timeout;

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

        _timeout = new SynchronizedReactiveProperty<int>().DisposeItWith(Disposable);
        Timeout = _timeout.ToBindableReactiveProperty().DisposeItWith(Disposable);

        _duration = new ReactiveProperty<string?>(
            DefaultTestDurationInSeconds.ToString()
        ).DisposeItWith(Disposable);
        Duration = new HistoricalStringProperty(nameof(Duration), _duration, loggerFactory)
            .SetRoutableParent(this)
            .DisposeItWith(Disposable);
        Duration.AddValidationRule(Validate);
        Duration.ForceValidate();
    }

    public HistoricalStringProperty Duration { get; }

    public BindableReactiveProperty<bool> IsEnabled { get; }

    public INotifyCollectionChangedSynchronizedViewList<MotorItemViewModel>? Motors
    {
        get;
        private set;
    }

    public BindableReactiveProperty<int> Timeout { get; }

    public override ValueTask Init(ISetupPage context)
    {
        _motorTestClient =
            context.Target.CurrentValue?.Device.GetMicroservice<IMotorTestClient>()
            ?? throw new Exception($"{nameof(IMotorTestClient)} should not be null");

        _motorsView = _motorTestClient
            .TestMotors.CreateView(motor => new MotorItemViewModel(
                motor,
                Timeout,
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

    private ValidationResult Validate(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            _isEnabled.Value = false;
            return ValidationResult.FailAsNullOrWhiteSpace;
        }

        _isEnabled.Value = true;

        if (!int.TryParse(value, out var i))
        {
            _isEnabled.Value = false;
            return ValidationResult.FailAsNotNumber;
        }

        if (i < 0)
        {
            _isEnabled.Value = false;
            return ValidationResult.FailAsOutOfRange(
                ushort.MinValue.ToString(),
                ushort.MaxValue.ToString()
            );
        }

        _timeout.Value = i;

        return ValidationResult.Success;
    }

    public override IExportInfo Source => SystemModule.Instance;
}
