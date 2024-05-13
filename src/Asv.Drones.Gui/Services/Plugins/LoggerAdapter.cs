using System;
using System.Threading.Tasks;
using NLog;
using NuGet.Common;

namespace Asv.Drones.Gui;

public class LoggerAdapter : LoggerBase
{
    private readonly Logger _logger;

    public LoggerAdapter(Logger logger)
    {
        _logger = logger;
    }

    public override void Log(ILogMessage message)
    {
        switch (message.WarningLevel)
        {
            case WarningLevel.Severe:
                _logger.Error(message.Message);
                break;
            case WarningLevel.Important:
                _logger.Warn(message.Message);
                break;
            case WarningLevel.Minimal:
                _logger.Info(message.Message);
                break;
            case WarningLevel.Default:
                _logger.Debug(message.Message);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    public override Task LogAsync(ILogMessage message)
    {
        Log(message);
        return Task.CompletedTask;
    }
}