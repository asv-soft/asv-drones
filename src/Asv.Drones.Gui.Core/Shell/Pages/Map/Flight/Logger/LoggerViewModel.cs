using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Windows.Input;
using Asv.Common;
using Avalonia.Controls;
using DynamicData;
using DynamicData.Binding;
using Material.Icons;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Asv.Drones.Gui.Core;

public class LoggerViewModel : FlightWidgetBase
{
    private readonly SourceList<FlightLogMessageViewModel> _logSource = new();
    private readonly ReadOnlyObservableCollection<FlightLogMessageViewModel> _logs;

    private readonly Subject<Func<FlightLogMessageViewModel, bool>> _filterUpdate = new();

    private const int MaxLogSize = 30;

    public LoggerViewModel() : base(new Uri(UriString + "logger"))
    {
        if (Design.IsDesignMode)
        {
            _logs = new ReadOnlyObservableCollection<FlightLogMessageViewModel>(
                new ObservableCollection<FlightLogMessageViewModel>(new[]
                {
                    new FlightLogMessageViewModel(MaterialIconKind.Error, "ERROR", LogMessageType.Error),
                    new FlightLogMessageViewModel(MaterialIconKind.InfoCircle, "INFO", LogMessageType.Info),
                    new FlightLogMessageViewModel(MaterialIconKind.Warning, "WARNING", LogMessageType.Warning),
                    new FlightLogMessageViewModel(MaterialIconKind.Abacus, "TRACE", LogMessageType.Trace)
                }));
        }
    }
    
    [ImportingConstructor]
    public LoggerViewModel(ILogService log) : this()
    {
        Location = WidgetLocation.Bottom;
        Title = "Logger";

        _logSource.LimitSizeTo(MaxLogSize).Subscribe().DisposeItWith(Disposable);
        _filterUpdate.OnNext(FilterByTypePredicate);

        log.OnMessage
            .ObserveOn(RxApp.MainThreadScheduler)
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
        //      log.SendMessage(new LogMessage(DateTime.Now, LogMessageType.Error, "debug", "Test error", "This is a test log message"));
        //      log.SendMessage(new LogMessage(DateTime.Now, LogMessageType.Warning, "debug", "Test warning", "This is a test log message"));
        //      log.SendMessage(new LogMessage(DateTime.Now, LogMessageType.Trace, "debug", "Test trace", "This is a test log message"));
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
        return logMessage.Type switch
        {
            LogMessageType.Info => new FlightLogMessageViewModel(MaterialIconKind.InfoCircle, logMessage.Message, LogMessageType.Info),
            LogMessageType.Error => new FlightLogMessageViewModel(MaterialIconKind.Error, logMessage.Message, LogMessageType.Error),
            LogMessageType.Warning => new FlightLogMessageViewModel(MaterialIconKind.Warning, logMessage.Message, LogMessageType.Warning),
            LogMessageType.Trace => new FlightLogMessageViewModel(MaterialIconKind.Abacus, logMessage.Message, LogMessageType.Trace),
            _ => new FlightLogMessageViewModel(MaterialIconKind.InfoCircle, logMessage.Message, LogMessageType.Info)
        };
    }
    
    private bool FilterByTypePredicate(FlightLogMessageViewModel vm)
    {
        return vm.Type switch
        {
            LogMessageType.Error => IsErrorSelected,
            LogMessageType.Info => IsInfoSelected,
            LogMessageType.Warning => IsWarningSelected,
            LogMessageType.Trace => IsTraceSelected,
            _ => false
        };
    }

    protected override void InternalAfterMapInit(IMap map)
    {
    }
}