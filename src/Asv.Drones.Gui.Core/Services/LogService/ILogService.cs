using Asv.Store;

namespace Asv.Drones.Gui.Core
{
    public interface ILogService
    {
        void SendMessage(LogMessage message);
        IObservable<LogMessage> OnMessage { get; }
        ITextStore LogStore { get; }
    }

    public static class LogServiceHelper
    {
        public static void Error(this ILogService src, string sender, string message,
            Exception ex = null)
        {
            src.SendMessage(new LogMessage(DateTime.Now, LogMessageType.Error, sender, message, ex?.Message));
        }
        public static void Info(this ILogService src, string sender, string message)
        {
            src.SendMessage(new LogMessage(DateTime.Now, LogMessageType.Info, sender, message, null));
        }
        public static void Warning(this ILogService src, string sender, string message)
        {
            src.SendMessage(new LogMessage(DateTime.Now, LogMessageType.Warning, sender, message, null));
        }
        public static void Trace(this ILogService src, string sender, string message)
        {
            src.SendMessage(new LogMessage(DateTime.Now, LogMessageType.Trace, sender, message, null));
        }
    }

    public enum LogMessageType
    {
        Info,
        Error,
        Warning,
        Trace,
    }

    public class LogMessage
    {
        public LogMessage(DateTime dateTime, LogMessageType type, string source, string message, string description)
        {
            Type = type;
            Source = source;
            Message = message;
            Description = description;
            DateTime = dateTime;
        }
        public DateTime DateTime { get; }
        public LogMessageType Type { get; }
        public string Source { get; }
        public string Message { get; }
        public string Description { get; }

        public override string ToString()
        {
            return $"{Type} {Message}";
        }
    }
}