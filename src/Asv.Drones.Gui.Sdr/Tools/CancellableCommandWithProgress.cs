using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using Asv.Common;
using Asv.Drones.Gui.Core;
using NLog;
using ReactiveUI;

namespace Asv.Drones.Gui.Sdr;

public class CancellableCommandWithProgress<TArg,TResult>: DisposableReactiveObject,IProgress<double>
{
    
    private readonly CommandExecuteDelegate<TArg, TResult> _execute;
    private readonly string _name;
    private readonly ILogService _log;
    private readonly IScheduler _scheduler;
    private readonly Logger _logger = LogManager.GetCurrentClassLogger();
    private double _progress;
    private bool _canExecute = true;


    public CancellableCommandWithProgress(CommandExecuteDelegate<TArg, TResult> execute, string name, ILogService log, IScheduler? scheduler = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(name));
        _execute = execute ?? throw new ArgumentNullException(nameof(execute));
        _name = name;
        _log = log ?? throw new ArgumentNullException(nameof(log));
        _scheduler = scheduler ?? RxApp.MainThreadScheduler;

        Command = ReactiveCommand.CreateFromObservable<TArg,TResult>(arg => Observable
            .StartAsync(token=>InternalExecute(arg,token), _scheduler)
            .TakeUntil(Cancel!)).DisposeItWith(Disposable);
        Command.ThrownExceptions.ObserveOn(_scheduler).Subscribe(InternalError).DisposeItWith(Disposable);
        Cancel = ReactiveCommand.Create(InternalCancel, Command.IsExecuting,_scheduler)
            .DisposeItWith(Disposable);
    }

    private void InternalError(Exception exception)
    {
        _logger.Error(exception,$"Error to execute command {_name}:{exception.Message}");
        _log.Error(_name,exception.Message);
    }

    private void InternalCancel()
    {
        _logger.Warn($"Command '{_name}' was cancelled");
    }

    private async Task<TResult> InternalExecute(TArg arg, CancellationToken ct)
    {
        var lastState = CanExecute;
        _scheduler.Schedule(()=>CanExecute = false);
        _logger.Info($"Start command '{_name}' with args {arg}");
        using var cancel = CancellationTokenSource.CreateLinkedTokenSource(DisposeCancel, ct);
        try
        {
            _scheduler.Schedule(()=>Progress = 0);
            var result = await _execute(arg, this, cancel.Token);
            _scheduler.Schedule(()=>Progress = 0);
            _logger.Info($"Command '{_name}' with args {arg} completed with result {result}");
            return result;
        }
        finally
        {
            _scheduler.Schedule(()=>CanExecute = lastState);
        }
    }

    public bool CanExecute
    {
        get => _canExecute;
        set => this.RaiseAndSetIfChanged(ref _canExecute, value);
    }

    public ReactiveCommand<Unit,Unit> Cancel { get; }

    public ReactiveCommand<TArg,TResult> Command { get; }

    public double Progress
    {
        get => _progress;
        private set => this.RaiseAndSetIfChanged(ref _progress, value);
    }

    public void Report(double value)
    {
        _scheduler.Schedule(()=>Progress = value);
    }

    public async void ExecuteSync()
    {
        var val = await Command.Execute();
    }
}