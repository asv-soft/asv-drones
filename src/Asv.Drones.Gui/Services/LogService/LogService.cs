using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;
using Asv.Common;
using Asv.Drones.Gui.Api;
using DynamicData;
using NLog;
using NLog.Layouts;
using NLog.Targets;
using ReactiveUI;

namespace Asv.Drones.Gui;

public class LogService : DisposableOnceWithCancel, ILogService
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
    private readonly Subject<LogMessage> _onMessage;
    private readonly string _logFile;

    public LogService(string logsFolder)
    {
        _logFile = Path.Combine(logsFolder, "log.log");
        ArgumentNullException.ThrowIfNull(logsFolder);
        if (!Directory.Exists(logsFolder))
        {
            Directory.CreateDirectory(logsFolder);
        }

        LogManager.Setup().LoadConfiguration(builder =>
        {
            var layout =
                Layout.FromString("${longdate}|${threadid}|${level}|${logger}|${message} ${exception:format=tostring}");
#if DEBUG
            builder.ForLogger().FilterMinLevel(LogLevel.Trace).WriteToDebug(layout: layout);
            builder.ForLogger().FilterLevels(LogLevel.Trace, LogLevel.Fatal).WriteToFile(
                fileName: _logFile,
                lineEnding: LineEndingMode.CRLF,
                maxArchiveDays: 30,
                maxArchiveFiles: 30,
                layout: layout);
#endif
            builder.ForLogger().FilterMinLevel(LogLevel.Trace).WriteToColoredConsole(layout: layout);
            builder.ForLogger().FilterLevels(LogLevel.Trace, LogLevel.Fatal).WriteToFile(
                fileName: _logFile,
                lineEnding: LineEndingMode.CRLF,
                maxArchiveDays: 30,
                maxArchiveFiles: 30,
                layout: layout);
        });

        var errorHandler = new Subject<Exception>().DisposeItWith(Disposable);
        errorHandler.Subscribe(err =>
        {
            Logger.Fatal(err, "Unhandled exception in RxApp.");
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
        Logger.Log(LogLevel.FromString(logMessage.Type.ToString()), logMessage.Message );
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
}