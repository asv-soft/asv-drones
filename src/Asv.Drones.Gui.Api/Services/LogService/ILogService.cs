using Microsoft.Extensions.Logging;
using ReactiveUI;

namespace Asv.Drones.Gui.Api;

public static class LogHelper
{
    public static IDisposable CatchToLog<TParam, TResult>(this ReactiveCommand<TParam, TResult> cmd, ILogService log, string sender)
    {
        return cmd.ThrownExceptions.Subscribe(ex=>log.Error(sender, ex.Message,ex));
    }
}
    

public interface ILogService: ILoggerFactory
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
        SaveMessage(new LogMessage(DateTime.Now, LogLevel.Critical, sender, message, ex?.Message));
    }

    public void Error(string sender, string message,
        Exception? ex = default)
    {
        SaveMessage(new LogMessage(DateTime.Now, LogLevel.Error, sender, message, ex?.Message));
    }

    public void Info(string sender, string message)
    {
        SaveMessage(new LogMessage(DateTime.Now, LogLevel.Information, sender, message, default));
    }

    public void Warning(string sender, string message)
    {
        SaveMessage(new LogMessage(DateTime.Now, LogLevel.Warning, sender, message, default));
    }

    public void Trace(string sender, string message)
    {
        SaveMessage(new LogMessage(DateTime.Now, LogLevel.Trace, sender, message, default));
    }

    public void Debug(string sender, string message)
    {
        SaveMessage(new LogMessage(DateTime.Now, LogLevel.Debug, sender, message, default));
    }
}



public class LogMessage(DateTime timestamp, LogLevel logLevel, string category, string message, string? description)
{
    public DateTime Timestamp { get; } = timestamp;
    public LogLevel LogLevel { get; } = logLevel;
    public string Category { get; internal set; } = category;
    public string Message { get; } = message;
    public string? Description { get; } = description;

    public override string ToString()
    {
        return $"{Category} {Message}";
    }
}