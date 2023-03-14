using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Reactive.Linq;
using Asv.Common;
using Asv.Drones.Gui.Core;
using DynamicData.Binding;
using Material.Icons;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Asv.Drones.Gui.Core;

public class FlightLogMessage
{
    public MaterialIconKind IconKind { get; set; }
    public string Message { get; set; }
    public bool IsError { get; set; }
    public bool IsWarning { get; set; }
    public bool IsInfo { get; set; }
    public bool IsTrace { get; set; }
}

public class LoggerViewModel : FlightWidgetBase
{
    public LoggerViewModel(ILogService log) : base(new Uri(FlightWidgetBase.UriString + "logger"))
    {
        Location = WidgetLocation.Bottom;
        Title = "Logger";
        
        log.OnMessage
            .ObserveOn(RxApp.MainThreadScheduler)
            .Subscribe(_ => UpdateLogMessages(Messages, _)).DisposeItWith(Disposable);
        this.WhenValueChanged(_ => Messages).Subscribe().DisposeItWith(Disposable);

#if DEBUG
        log.SendMessage(new LogMessage(DateTime.Now, LogMessageType.Error, "debug", "Test error", "This is a test log message"));
        log.SendMessage(new LogMessage(DateTime.Now, LogMessageType.Warning, "debug", "Test warning", "This is a test log message"));
        log.SendMessage(new LogMessage(DateTime.Now, LogMessageType.Trace, "debug", "Test trace", "This is a test log message"));
#endif        
    }

    [Reactive] 
    public ObservableCollection<FlightLogMessage> Messages { get; set; } = new();

    private static void UpdateLogMessages(ObservableCollection<FlightLogMessage> messages, LogMessage logMessage)
    {
        switch (logMessage.Type)
        {
            case LogMessageType.Info:
                messages.Add(new FlightLogMessage { IconKind = MaterialIconKind.InfoCircle, Message = logMessage.Message, IsInfo = true });
                break;
            case LogMessageType.Error:
                messages.Add(new FlightLogMessage { IconKind = MaterialIconKind.Error, Message = logMessage.Message, IsError = true });
                break;
            case LogMessageType.Warning:
                messages.Add(new FlightLogMessage { IconKind = MaterialIconKind.Warning, Message = logMessage.Message, IsWarning = true });
                break;
            case LogMessageType.Trace:
                messages.Add(new FlightLogMessage { IconKind = MaterialIconKind.Abacus, Message = logMessage.Message, IsTrace = true });
                break;
            default:
                messages.Add(new FlightLogMessage { IconKind = MaterialIconKind.InfoCircle, Message = logMessage.Message, IsInfo = true });
                break;
        }
    }

    protected override void InternalAfterMapInit(IMap map)
    {
    }
}