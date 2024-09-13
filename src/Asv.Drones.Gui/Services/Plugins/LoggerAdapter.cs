using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NuGet.Common;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace Asv.Drones.Gui;

public class LoggerAdapter : LoggerBase
{
    private readonly ILogger _logger;

    public LoggerAdapter(ILogger logger)
    {
        _logger = logger;
    }

    public override void Log(ILogMessage message)
    {
        switch (message.WarningLevel)
        {
            case WarningLevel.Severe:
                _logger.LogError(message.Message);
                break;
            case WarningLevel.Important:
                _logger.LogWarning(message.Message);
                break;
            case WarningLevel.Minimal:
                _logger.LogDebug(message.Message);
                break;
            case WarningLevel.Default:
                _logger.LogInformation(message.Message);
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