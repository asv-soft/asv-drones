using ReactiveUI;

namespace Asv.Drones.Gui.Api;

public static class LogHelper
{
    public static IDisposable CatchToLog<TParam, TResult>(this ReactiveCommand<TParam, TResult> cmd, ILogService log, string sender)
    {
        return cmd.ThrownExceptions.Subscribe(ex=>log.Error(sender, ex.Message,ex));
    }
}
    

public interface ILogService
{
    IObservable<LogMessage> OnMessage { get; }
    void SaveMessage(LogMessage logMessage);
    IEnumerable<LogItemViewModel> LoadItemsFromLogFile();
    void DeleteLogFile();

    public IDisposable CatchToLog<TParam, TResult>( ReactiveCommand<TParam, TResult> cmd, string sender)
    {
        return cmd.ThrownExceptions.Subscribe(ex=>Error(sender, ex.Message,ex));
    }
    
    public void Fatal(string sender, string message,
        Exception? ex = default)
    {
        SaveMessage(new LogMessage(DateTime.Now, LogMessageType.Fatal, sender, message, ex?.Message));
    }

    public void Error(string sender, string message,
        Exception? ex = default)
    {
        SaveMessage(new LogMessage(DateTime.Now, LogMessageType.Error, sender, message, ex?.Message));
    }

    public void Info(string sender, string message)
    {
        SaveMessage(new LogMessage(DateTime.Now, LogMessageType.Info, sender, message, default));
    }

    public void Warning(string sender, string message)
    {
        SaveMessage(new LogMessage(DateTime.Now, LogMessageType.Warning, sender, message, default));
    }

    public void Trace(string sender, string message)
    {
        SaveMessage(new LogMessage(DateTime.Now, LogMessageType.Trace, sender, message, default));
    }

    public void Debug(string sender, string message)
    {
        SaveMessage(new LogMessage(DateTime.Now, LogMessageType.Debug, sender, message, default));
    }
}

public enum LogMessageType
{
    Trace,
    Debug,
    Info,
    Warning,
    Error,
    Fatal
}

public class LogMessage
{
    public LogMessage(DateTime dateTime, LogMessageType type, string source, string message, string? description)
    {
        Type = type;
        Source = source;
        Message = message;
        Description = description;
        DateTime = dateTime;
    }

    public DateTime DateTime { get; }
    public LogMessageType Type { get; }
    public string Source { get; internal set; }
    public string Message { get; }
    public string? Description { get; }

    public override string ToString()
    {
        return $"{Type} {Message}";
    }
}