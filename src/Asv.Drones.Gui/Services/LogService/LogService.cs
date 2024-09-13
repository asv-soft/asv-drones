using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;
using Asv.Common;
using Asv.Drones.Gui.Api;
using Microsoft.Extensions.Logging;
using ReactiveUI;
using ZLogger;

namespace Asv.Drones.Gui;

public class LogService : DisposableOnceWithCancel, ILogService, ILoggerFactory
{
    
    private readonly Subject<LogMessage> _onMessage;
    private readonly string _logFile;
    private readonly ILogger<LogService> _logger;
    private readonly ILoggerFactory _factory;

    public LogService(string logsFolder)
    {
        ArgumentNullException.ThrowIfNull(logsFolder);
        _factory = LoggerFactory.Create(builder =>
        {
            builder.ClearProviders();
            builder.SetMinimumLevel(LogLevel.Trace);
            builder.AddZLoggerRollingFile(options =>
            {
                options.FilePathSelector = (dt, index) => $"{logsFolder}/{dt:yyyy-MM-dd}_{index}.logs";
                options.UseJsonFormatter();
            });
        }).DisposeItWith(Disposable);
        _logFile = Path.Combine(logsFolder, "log.log");
        _logger = _factory.CreateLogger<LogService>();
        
        if (!Directory.Exists(logsFolder))
        {
            Directory.CreateDirectory(logsFolder);
        }


        var errorHandler = new Subject<Exception>().DisposeItWith(Disposable);
        errorHandler.Subscribe(err =>
        {
            _logger.LogError(err, "Unhandled exception in RxApp.");
            if (Debugger.IsAttached) Debugger.Break();
            SaveMessage(new LogMessage(DateTime.Now, LogMessageType.Error, "Core", err.Message, err.ToString()));
            //RxApp.MainThreadScheduler.Schedule(() => throw err);
        });
        RxApp.DefaultExceptionHandler = errorHandler;
        TaskScheduler.UnobservedTaskException += (sender, args) =>
        {
            errorHandler.OnNext(args.Exception);
            args.SetObserved();
        };
        _onMessage = new Subject<LogMessage>().DisposeItWith(Disposable);
    }

    public IObservable<LogMessage> OnMessage => _onMessage;

    public void SaveMessage(LogMessage logMessage)
    {
        _onMessage.OnNext(logMessage);
        switch (logMessage.Type)
        {
            case LogMessageType.Trace:
                _logger.LogTrace(logMessage.Message);
                break;
            case LogMessageType.Debug:
                _logger.LogDebug(logMessage.Message);
                break;
            case LogMessageType.Info:
                _logger.LogInformation(logMessage.Message);
                break;
            case LogMessageType.Warning:
                _logger.LogWarning(logMessage.Message);
                break;
            case LogMessageType.Error:
                _logger.LogError(logMessage.Message);
                break;
            case LogMessageType.Fatal:
                _logger.LogCritical(logMessage.Message);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
        
    }

    public void DeleteLogFile()
    {
        if (File.Exists(_logFile)) File.Delete(_logFile);
    }

    public IEnumerable<LogItemViewModel> LoadItemsFromLogFile()
    {
        var logsFilePath = _logFile;

        if (File.Exists(logsFilePath))
        {
            var index = 0;
            using var fs = new FileStream(logsFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            using var sr = new StreamReader(fs);
            LogItemViewModel currentItem = null;
            var messageBuilder = new StringBuilder();
            while (!sr.EndOfStream)
            {
                var line = sr.ReadLine();
                if (line == null) continue;

                var parts = line.Split('|');
                if (parts.Length < 4)
                {
                    if (currentItem != null)
                    {
                        messageBuilder.Append(line);
                        currentItem.Message = messageBuilder.ToString();
                    }

                    continue;
                }

                messageBuilder.Clear();

                var timestamp = DateTime.TryParse(parts[0], out var ts) ? ts : default;
                var thread = parts[1];
                var level = Enum.TryParse<LogMessageType>(parts[2], out var lvl) ? lvl : default;
                var itemClass = parts[3];
                messageBuilder.Append(parts[4]);

                currentItem =
                    new LogItemViewModel(index, thread, level, timestamp, itemClass, messageBuilder.ToString());
                yield return currentItem;
                index++;
            }
        }
    }

    public ILogger CreateLogger(string categoryName)
    {
        return _factory.CreateLogger(categoryName);
    }

    public void AddProvider(ILoggerProvider provider)
    {
        _factory.AddProvider(provider);
    }
}