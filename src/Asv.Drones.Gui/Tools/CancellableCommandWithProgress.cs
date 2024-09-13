using System;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Asv.Common;
using Asv.Drones.Gui.Api;
using Microsoft.Extensions.Logging;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using ZLogger;

namespace Asv.Drones.Gui;

public static class CoreDesignTime
{
    public static CancellableCommandWithProgress<TArg, TResult> CancellableCommand<TArg, TResult>() =>
        new((_, _, _) => Task.FromResult(default(TResult)), "Default", NullLogService.Instance);
}

public delegate Task<TResult> CommandExecuteDelegate<TArg, TResult>(TArg arg, IProgress<double> progress,
    CancellationToken cancel);

public class CancellableCommandWithProgress<TArg, TResult> : DisposableReactiveObject, IProgress<double>
{
    private readonly CommandExecuteDelegate<TArg, TResult> _execute;
    private readonly ILogService _log;
    private readonly IScheduler _scheduler;
    private double _progress;
    private bool _canExecute = true;
    private readonly ReactiveCommand<Unit, Unit> _cancelCommand;
    private readonly ReactiveCommand<TArg, TResult> _command;
    private readonly ILogger<CancellableCommandWithProgress<TArg,TResult>> _logger;

    public CancellableCommandWithProgress(CommandExecuteDelegate<TArg, TResult> execute, string name, ILogService log,
        IScheduler? scheduler = null)
    {
        _logger = log.CreateLogger<CancellableCommandWithProgress<TArg, TResult>>();
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(name));
        Title = name;

        _execute = execute ?? throw new ArgumentNullException(nameof(execute));
        _log = log ?? throw new ArgumentNullException(nameof(log));
        _scheduler = scheduler ?? RxApp.MainThreadScheduler;

        _command = ReactiveCommand.CreateFromObservable<TArg, TResult>(arg => Observable
            .StartAsync(token => Task.Run(() => InternalExecute(arg, token), token), _scheduler)
            .TakeUntil(_cancelCommand!)).DisposeItWith(Disposable);
        _command.ThrownExceptions.ObserveOn(_scheduler).Subscribe(InternalError).DisposeItWith(Disposable);
        _cancelCommand = ReactiveCommand.Create(InternalCancel, _command.IsExecuting, _scheduler)
            .DisposeItWith(Disposable);
        _command.IsExecuting.Subscribe(x => IsExecuting = x).DisposeItWith(Disposable);
    }

    [Reactive] public bool IsExecuting { get; set; }
    [Reactive] public string Title { get; set; }

    private void InternalError(Exception exception)
    {
        _logger.ZLogError(exception,$"Error to execute command {Title}:{exception.Message}");
        _log.Error(Title, exception.Message);
    }

    private void InternalCancel()
    {
        _logger.ZLogWarning($"Command '{Title}' was cancelled");
    }

    private async Task<TResult> InternalExecute(TArg arg, CancellationToken ct)
    {
        var lastState = CanExecute;
        _scheduler.Schedule(() => CanExecute = false);
        _logger.ZLogInformation($"Start command '{Title}' with args {arg}");
        using var cancel = CancellationTokenSource.CreateLinkedTokenSource(DisposeCancel, ct);
        try
        {
            _scheduler.Schedule(() => Progress = 0);
            var result = await _execute(arg, this, cancel.Token);
            _scheduler.Schedule(() => Progress = 0);
            _logger.ZLogInformation($"Command '{Title}' with args {arg} completed with result {result}");
            return result;
        }
        finally
        {
            _scheduler.Schedule(() => CanExecute = lastState);
        }
    }

    public bool CanExecute
    {
        get => _canExecute;
        set => this.RaiseAndSetIfChanged(ref _canExecute, value);
    }

    public ICommand Cancel => _cancelCommand;

    public ICommand Command => _command;

    public double Progress
    {
        get => _progress;
        private set => this.RaiseAndSetIfChanged(ref _progress, value);
    }


    public void Report(double value)
    {
        _scheduler.Schedule(() => Progress = value);
    }

    public async void ExecuteSync()
    {
        var val = await _command.Execute();
    }

    public async Task<TResult> ExecuteAsync(TArg arg, CancellationToken cancel)
    {
        using var sub = cancel.Register(() => _cancelCommand.Execute());
        return await _command.Execute(arg).ToTask(cancel);
    }
}