namespace Asv.Drones.Gui.Core
{
    /// <summary>
    /// Represents a logging service.
    /// </summary>
    public interface ILogService
    {
        /// <summary>
        /// Saves a log message.
        /// </summary>
        /// <param name="message">The log message to be saved.</param>
        void SaveMessage(LogMessage message);

        /// <summary>
        /// Gets the observable sequence of <see cref="LogMessage"/> objects.
        /// </summary>
        /// <value>
        /// The observable sequence of <see cref="LogMessage"/> objects.
        /// </value>
        IObservable<LogMessage> OnMessage { get; }

        /// <summary>
        /// Clears all the data stored in the current object. </summary> <remarks>
        /// This method is used to remove all data from the object and reset it to its initial state. </remarks>
        /// /
        void ClearAll();

        /// <summary>
        /// Returns the count of items.
        /// </summary>
        /// <returns>The count of items.</returns>
        int Count();

        /// <summary>
        /// Finds log messages based on the specified query.
        /// </summary>
        /// <param name="query">The query used to filter the log messages.</param>
        /// <returns>An IEnumerable of LogMessage objects that match the query.</returns>
        /// <remarks>
        /// The Find method searches for log messages based on the specified query.
        /// The query can include various filter criteria such as date range, log level,
        /// log source, log message content, etc. By applying the query, only the log messages
        /// that satisfy the specified criteria will be returned.
        /// </remarks>
        /// <seealso cref="LogMessage"/>
        /// <seealso cref="LogQuery"/>
        IEnumerable<LogMessage> Find(LogQuery query);

        /// <summary>
        /// Returns the number of logs that match the given query.
        /// </summary>
        /// <param name="query">The query used to filter the logs.</param>
        /// <returns>The number of logs that match the query.</returns>
        int Count(LogQuery query);
    }

    /// <summary>
    /// Represents a query to filter log records.
    /// </summary>
    public class LogQuery
    {
        /// <summary>
        /// Gets or sets the number of elements to take from a collection.
        /// </summary>
        /// <value>
        /// The number of elements to take.
        /// </value>
        public int Take { get; set; }

        /// <summary>
        /// Gets or sets the number of elements to skip.
        /// </summary>
        /// <value>
        /// The number of elements to skip.
        /// </value>
        public int Skip { get; set; }

        /// <summary>
        /// Gets or sets the search text.
        /// </summary>
        /// <value>
        /// The search text entered by the user.
        /// </value>
        public string Search { get; set; }
    }

    /// <summary>
    /// The LogServiceHelper class provides extension methods to simplify logging operations.
    /// </summary>
    public static class LogServiceHelper
    {
        /// <summary>
        /// Logs an error message with optional exception information using the specified log service.
        /// </summary>
        /// <param name="src">The <see cref="ILogService"/> to use for logging.</param>
        /// <param name="sender">The sender of the error message.</param>
        /// <param name="message">The error message to be logged.</param>
        /// <param name="ex">The optional <see cref="Exception"/> containing additional information about the error.</param>
        /// <remarks>
        /// This method saves a new <see cref="LogMessage"/> with the current timestamp, the type of <see cref="LogMessageType.Error"/>,
        /// the specified sender, the given error message, and the exception message (if provided) using the specified log service.
        /// </remarks>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="src"/>, <paramref name="sender"/>, or <paramref name="message"/> is null.</exception>
        /// <example>
        /// <code>
        /// ILogService logService = new LogService();
        /// logService.Error("MySender", "An error occurred");
        /// logService.Error("MySender", "An error occurred", new Exception("Sample exception message"));
        /// </code>
        /// </example>
        public static void Error(this ILogService src, string sender, string message,
            Exception ex = default)
        {
            src.SaveMessage(new LogMessage(DateTime.Now, LogMessageType.Error, sender, message, ex?.Message));
        }

        /// <summary>
        /// Logs an informational message.
        /// </summary>
        /// <param name="src">The instance of the logging service.</param>
        /// <param name="sender">The sender of the message.</param>
        /// <param name="message">The message content.</param>
        public static void Info(this ILogService src, string sender, string message)
        {
            src.SaveMessage(new LogMessage(DateTime.Now, LogMessageType.Info, sender, message, default));
        }

        /// <summary>
        /// Logs a warning message using the provided log service.
        /// </summary>
        /// <param name="src">The log service used to save the warning message.</param>
        /// <param name="sender">The sender of the warning message.</param>
        /// <param name="message">The warning message to be logged.</param>
        public static void Warning(this ILogService src, string sender, string message)
        {
            src.SaveMessage(new LogMessage(DateTime.Now, LogMessageType.Warning, sender, message, default));
        }

        /// <summary>
        /// Logs a trace message using the specified log service.
        /// </summary>
        /// <param name="src">The log service.</param>
        /// <param name="sender">The sender of the message.</param>
        /// <param name="message">The message to be logged.</param>
        public static void Trace(this ILogService src, string sender, string message)
        {
            src.SaveMessage(new LogMessage(DateTime.Now, LogMessageType.Trace, sender, message, default));
        }
    }

    /// <summary>
    /// Defines the types of log messages.
    /// </summary>
    public enum LogMessageType
    {
        /// <summary>
        /// Represents a log message type of 'Info'.
        /// </summary>
        Info,

        /// <summary>
        /// Represents an error message type in the log system.
        /// </summary>
        Error,

        /// <summary>
        /// Represents a warning message in the log.
        /// </summary>
        /// <remarks>
        /// This enum member is used to identify a warning message in the log.
        /// </remarks>
        /// <example>
        /// The following example demonstrates the usage of the <see cref="LogMessageType.Warning"/> enum member.
        /// <code>
        /// LogMessageType messageType = LogMessageType.Warning;
        /// </code>
        /// </example>
        Warning,

        /// <summary>
        /// Represents the severity level of a log message as "Trace".
        /// </summary>
        /// <remarks>
        /// Trace log messages are used to provide detailed information for debugging and troubleshooting purposes.
        /// </remarks>
        /// <example>
        /// This example demonstrates how to use the <see cref="LogMessageType.Trace"/> severity level:
        /// <code>
        /// Logger.Log(LogMessageType.Trace, "Trace log message");
        /// </code>
        /// </example>
        Trace,
    }

    /// <summary>
    /// Represents a log message with various properties such as date and time, log message type, source, message, and description.
    /// </summary>
    public class LogMessage
    {
        /// <summary>
        /// Represents a log message.
        /// </summary>
        /// <param name="dateTime">The date and time when the log message occurred.</param>
        /// <param name="type">The type of log message.</param>
        /// <param name="source">The source of the log message.</param>
        /// <param name="message">The message text.</param>
        /// <param name="description">Additional description of the log message.</param>
        public LogMessage(DateTime dateTime, LogMessageType type, string source, string message, string description)
        {
            Type = type;
            Source = source;
            Message = message;
            Description = description;
            DateTime = dateTime;
        }

        /// <summary>
        /// Gets the current date and time.
        /// </summary>
        /// <returns>
        /// The current date and time.
        /// </returns>
        public DateTime DateTime { get; }

        /// <summary>
        /// Gets the type of the log message.
        /// </summary>
        /// <value>
        /// The type of the log message.
        /// </value>
        public LogMessageType Type { get; }

        /// <summary>
        /// Gets or sets the source of the property.
        /// </summary>
        /// <value>
        /// The source of the property.
        /// </value>
        public string Source { get; internal set; }

        /// <summary>
        /// Gets the message.
        /// </summary>
        /// <remarks>
        /// This property represents the message associated with an object.
        /// </remarks>
        /// <returns>
        /// A string that represents the message.
        /// </returns>
        public string Message { get; }

        /// <summary>
        /// Gets the description of the property.
        /// </summary>
        /// <value>
        /// The description of the property.
        /// </value>
        public string Description { get; }

        /// <summary>
        /// Override of the ToString() method.
        /// </summary>
        /// <returns>A string representation of the object.</returns>
        public override string ToString()
        {
            return $"{Type} {Message}";
        }
    }
}