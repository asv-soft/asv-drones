using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using Asv.Common;
using Asv.Drones.Gui.Api;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using ReactiveUI;
using ZLogger;

namespace Asv.Drones.Gui;

public class LogService : DisposableOnceWithCancel, ILogService, ILoggerFactory
{
    private readonly string _logsFolder;

    private readonly Subject<LogMessage> _onMessage;
    
    private readonly ILogger<LogService> _logger;
    private readonly ILoggerFactory _factory;

    public LogService(string logsFolder,LogLevel minLevel)
    {
        _logsFolder = logsFolder;
        ArgumentNullException.ThrowIfNull(logsFolder);
        _factory = LoggerFactory.Create(builder =>
        {
            builder.ClearProviders();
            builder.SetMinimumLevel(minLevel);
#if DEBUG
            builder.AddZLoggerConsole(options =>
            {
                options.IncludeScopes = true;
                options.OutputEncodingToUtf8 = false;
                options.UsePlainTextFormatter(formatter =>
                {
                    formatter.SetPrefixFormatter($"{0:HH:mm:ss.fff} | ={1:short}= | {2,-40} ", (in MessageTemplate template, in LogInfo info) => template.Format(info.Timestamp, info.LogLevel,info.Category));
                    //formatter.SetExceptionFormatter((writer, ex) => Utf8StringInterpolation.Utf8String.Format(writer, $"{ex.Message}"));
                });
            });
#endif
            builder.AddZLoggerRollingFile(options =>
            {
                options.FilePathSelector = (dt, index) => $"{logsFolder}/{dt:yyyy-MM-dd}_{index}.logs";
                options.UseJsonFormatter();
                options.RollingSizeKB = 1024 * 10;
            });
        }).DisposeItWith(Disposable);

        _logger = _factory.CreateLogger<LogService>();
        
        if (!Directory.Exists(logsFolder))
        {
            Directory.CreateDirectory(logsFolder);
        }


        var errorHandler = new Subject<Exception>().DisposeItWith(Disposable);
        errorHandler.Subscribe(err =>
        {
            if (err is AggregateException && err.InnerException != null)
            {
                err = err.InnerException;
            }
            _logger.LogError(err, "Unhandled exception in RxApp.");
            if (Debugger.IsAttached) Debugger.Break();
            SaveMessage(new LogMessage(DateTime.Now, LogLevel.Error, "Core", err.Message, err.ToString()));
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
        switch (logMessage.LogLevel)
        {
            case LogLevel.Trace:
                _logger.LogTrace(logMessage.Message);
                break;
            case LogLevel.Debug:
                _logger.LogDebug(logMessage.Message);
                break;
            case LogLevel.Information:
                _logger.LogInformation(logMessage.Message);
                break;
            case LogLevel.Warning:
                _logger.LogWarning(logMessage.Message);
                break;
            case LogLevel.Error:
                _logger.LogError(logMessage.Message);
                break;
            case LogLevel.Critical:
                _logger.LogCritical(logMessage.Message);
                break;
            case LogLevel.None:
            default:
                throw new ArgumentOutOfRangeException();
        }
        
    }

    public void DeleteLogFile()
    {
        foreach (var logFilePath in Directory.EnumerateFiles(_logsFolder,"*.logs"))
        {
            File.Delete(logFilePath);
        }
    }

    public IEnumerable<LogItemViewModel> LoadItemsFromLogFile()
    {
        
            var index = 0;
            foreach (var logFilePath in Directory.EnumerateFiles(_logsFolder,"*.logs"))
            {
                using var fs = new FileStream(logFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                var rdr = new JsonTextReader(new StreamReader(fs))
                {
                    SupportMultipleContent = true,
                };
                var serializer = new JsonSerializer();
                
                while (rdr.Read())
                {
                    if (rdr.TokenType == JsonToken.StartObject)
                    {
                        var item = serializer.Deserialize<LogMessage>(rdr);
                        if (item != null) yield return new LogItemViewModel(index, item);
                    }
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