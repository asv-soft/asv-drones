using Material.Icons;

namespace Asv.Drones.Gui.Api;

public class RemoteLogMessageProxy
{
    public RemoteLogMessageProxy(LogMessage textMessage)
    {
        switch (textMessage.Type)
        {
            case LogMessageType.Info:
                IsInfo = true;
                Icon = MaterialIconKind.InformationCircle;
                break;

            case LogMessageType.Warning:
                IsWarning = true;
                Icon = MaterialIconKind.Warning;
                break;

            case LogMessageType.Error:
                IsError = true;
                Icon = MaterialIconKind.Warning;
                break;

            case LogMessageType.Trace:
                IsTrace = true;
                Icon = MaterialIconKind.Exclamation;
                break;
        }

        DateTime = textMessage.DateTime;
        Sender = textMessage.Source;
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