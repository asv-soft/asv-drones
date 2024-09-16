using System.Reactive.Subjects;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Asv.Drones.Gui.Api;

public class NullLogService : ILogService
{
    public static NullLogService Instance { get; } = new();

    public NullLogService()
    {
        OnMessage = new Subject<LogMessage>();
    }

    public IObservable<LogMessage> OnMessage { get; }

    public void SaveMessage(LogMessage logMessage)
    {
    }

    public IEnumerable<LogItemViewModel> LoadItemsFromLogFile()
    {
        return Array.Empty<LogItemViewModel>();
    }

    public void DeleteLogFile()
    {
    }

    public void Dispose()
    {
        
    }

    public ILogger CreateLogger(string categoryName)
    {
        return NullLoggerFactory.Instance.CreateLogger(categoryName);
    }

    public void AddProvider(ILoggerProvider provider)
    {
        
    }
}