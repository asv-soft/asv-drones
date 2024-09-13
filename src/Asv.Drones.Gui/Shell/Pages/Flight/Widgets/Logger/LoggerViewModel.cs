using System;
using System.Collections.ObjectModel;
using System.Composition;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Windows.Input;
using Asv.Common;
using Asv.Drones.Gui.Api;
using DynamicData;
using DynamicData.Binding;
using Material.Icons;
using Microsoft.Extensions.Logging;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Asv.Drones.Gui;

[Export(WellKnownUri.ShellPageMapFlight, typeof(IMapWidget))]
public class LoggerViewModel : MapWidgetBase
{
    private readonly SourceList<FlightLogMessageViewModel> _logSource = new();
    private readonly ReadOnlyObservableCollection<FlightLogMessageViewModel> _logs;

    private readonly Subject<Func<FlightLogMessageViewModel, bool>> _filterUpdate = new();

    private const int MaxLogSize = 30;

    public LoggerViewModel() : base(WellKnownUri.UndefinedUri)
    {
        DesignTime.ThrowIfNotDesignMode();
        _logs = new ReadOnlyObservableCollection<FlightLogMessageViewModel>(
            new ObservableCollection<FlightLogMessageViewModel>(new[]
            {
                new FlightLogMessageViewModel(MaterialIconKind.Error, "ERROR", LogLevel.Error),
                new FlightLogMessageViewModel(MaterialIconKind.InfoCircle, "INFO", LogLevel.Information),
                new FlightLogMessageViewModel(MaterialIconKind.Warning, "WARNING", LogLevel.Warning),
                new FlightLogMessageViewModel(MaterialIconKind.Abacus, "TRACE", LogLevel.Trace)
            }));
    }

    [ImportingConstructor]
    public LoggerViewModel(ILogService log) : base(WellKnownUri.ShellPageMapFlightWidgetLogger)
    {
        Order = 100;
        Location = WidgetLocation.Bottom;
        Title = RS.LoggerViewModel_Title;
        Icon = MaterialIconKind.Text;
        _logSource.LimitSizeTo(MaxLogSize).Subscribe().DisposeItWith(Disposable);
        _filterUpdate.OnNext(FilterByTypePredicate);

        log.OnMessage
            .Select(ConvertLogToMessage)
            .Subscribe(_logSource.Add)
            .DisposeItWith(Disposable);

        _logSource
            .Connect()
            .Filter(_filterUpdate)
            .Bind(out _logs)
            .Subscribe()
            .DisposeItWith(Disposable);

        this.WhenValueChanged(_ => _.IsTraceSelected)
            .Subscribe(_ => _filterUpdate.OnNext(FilterByTypePredicate))
            .DisposeItWith(Disposable);
        this.WhenValueChanged(_ => _.IsErrorSelected)
            .Subscribe(_ => _filterUpdate.OnNext(FilterByTypePredicate))
            .DisposeItWith(Disposable);
        this.WhenValueChanged(_ => _.IsInfoSelected)
            .Subscribe(_ => _filterUpdate.OnNext(FilterByTypePredicate))
            .DisposeItWith(Disposable);
        this.WhenValueChanged(_ => _.IsWarningSelected)
            .Subscribe(_ => _filterUpdate.OnNext(FilterByTypePredicate))
            .DisposeItWith(Disposable);

        ClearLogs = ReactiveCommand.Create(() => { _logSource.Clear(); }).DisposeItWith(Disposable);

        //  #if DEBUG
        //      log.SaveMessage(new LogMessage(DateTime.Now, LogMessageType.Error, "debug", "Test error", "This is a test log message"));
        //      log.SaveMessage(new LogMessage(DateTime.Now, LogMessageType.Warning, "debug", "Test warning", "This is a test log message"));
        //      log.SaveMessage(new LogMessage(DateTime.Now, LogMessageType.Trace, "debug", "Test trace", "This is a test log message"));
        //  #endif
    }

    public ReadOnlyObservableCollection<FlightLogMessageViewModel> Logs => _logs;

    [Reactive] public bool IsWarningSelected { get; set; } = true;
    [Reactive] public bool IsErrorSelected { get; set; } = true;
    [Reactive] public bool IsInfoSelected { get; set; } = true;
    [Reactive] public bool IsTraceSelected { get; set; } = true;
    public ICommand ClearLogs { get; }

    private static FlightLogMessageViewModel ConvertLogToMessage(LogMessage logMessage)
    {
        return logMessage.LogLevel switch
        {
            LogLevel.Information => new FlightLogMessageViewModel(MaterialIconKind.InfoCircle, logMessage.Message,
                LogLevel.Information),
            LogLevel.Error => new FlightLogMessageViewModel(MaterialIconKind.Error, logMessage.Message,
                LogLevel.Error),
            LogLevel.Warning => new FlightLogMessageViewModel(MaterialIconKind.Warning, logMessage.Message,
                LogLevel.Warning),
            LogLevel.Trace => new FlightLogMessageViewModel(MaterialIconKind.Abacus, logMessage.Message,
                LogLevel.Trace),
            _ => new FlightLogMessageViewModel(MaterialIconKind.InfoCircle, logMessage.Message, LogLevel.Information)
        };
    }

    private bool FilterByTypePredicate(FlightLogMessageViewModel vm)
    {
        return vm.Type switch
        {
            LogLevel.Error => IsErrorSelected,
            LogLevel.Information => IsInfoSelected,
            LogLevel.Warning => IsWarningSelected,
            LogLevel.Trace => IsTraceSelected,
            _ => false
        };
    }

    protected override void InternalAfterMapInit(IMap context)
    {
    }
}