namespace Asv.Drones.Gui.Core
{
    public interface ILogService
    {
        void SaveMessage(LogMessage message);
        IObservable<LogMessage> OnMessage { get; }
        void ClearAll();
        int Count();
        IEnumerable<LogMessage> Find(LogQuery query);
        int Count(LogQuery query);
    }

    public class LogQuery
    {
        public int Take { get; set; }
        public int Skip { get; set; }
        public string Search { get; set; }
    }

    public static class LogServiceHelper
    {
        public static void Error(this ILogService src, string sender, string message,
            Exception ex = default)
        {
            src.SaveMessage(new LogMessage(DateTime.Now, LogMessageType.Error, sender, message, ex?.Message));
        }
        public static void Info(this ILogService src, string sender, string message)
        {
            src.SaveMessage(new LogMessage(DateTime.Now, LogMessageType.Info, sender, message, default));
        }
        public static void Warning(this ILogService src, string sender, string message)
        {
            src.SaveMessage(new LogMessage(DateTime.Now, LogMessageType.Warning, sender, message, default));
        }
        public static void Trace(this ILogService src, string sender, string message)
        {
            src.SaveMessage(new LogMessage(DateTime.Now, LogMessageType.Trace, sender, message, default));
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
        public string Source { get; internal set; }
        public string Message { get; }
        public string Description { get; }

        public override string ToString()
        {
            return $"{Type} {Message}";
        }
    }
}