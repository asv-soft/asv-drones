using Material.Icons;
using Microsoft.Extensions.Logging;

namespace Asv.Drones.Gui.Api;

public class RemoteLogMessageProxy
{
    public RemoteLogMessageProxy(LogMessage textMessage)
    {
        switch (textMessage.LogLevel)
        {
            case LogLevel.Information:
                IsInfo = true;
                Icon = MaterialIconKind.InformationCircle;
                break;

            case LogLevel.Warning:
                IsWarning = true;
                Icon = MaterialIconKind.Warning;
                break;

            case LogLevel.Error:
                IsError = true;
                Icon = MaterialIconKind.Warning;
                break;

            case LogLevel.Trace:
                IsTrace = true;
                Icon = MaterialIconKind.Exclamation;
                break;
        }

        DateTime = textMessage.Timestamp;
        Sender = textMessage.Category;
        Message = textMessage.Message;
    }

    public bool IsError { get; }
    public bool IsWarning { get; }
    public bool IsTrace { get; }
    public bool IsInfo { get; }

    public DateTime DateTime { get; }
    public MaterialIconKind Icon { get; }
    public string Sender { get; }
    public string Message { get; }
}