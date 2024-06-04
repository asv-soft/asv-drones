using Material.Icons;

namespace Asv.Drones.Gui.Api;

public class LogItemViewModel(
    int itemIndex,
    string threadId,
    LogMessageType itemLevel,
    DateTime itemTimeStamp,
    string itemClass,
    string itemMessage) : DisposableReactiveObject
{
    public int Index { get; } = itemIndex;

    public MaterialIconKind Kind { get; } = itemLevel switch
    {
        LogMessageType.Debug => MaterialIconKind.Bug,
        LogMessageType.Trace => MaterialIconKind.Tractor,
        LogMessageType.Info => MaterialIconKind.Info,
        LogMessageType.Warning => MaterialIconKind.Bullhorn,
        LogMessageType.Error => MaterialIconKind.Fire,
        LogMessageType.Fatal => MaterialIconKind.FlashAlert,
        _ => MaterialIconKind.QuestionMark
    };

    public DateTime Timestamp { get; } = itemTimeStamp;
    public LogMessageType Level { get; } = itemLevel;
    public string Class { get; } = itemClass;
    public string Message { get; set; } = itemMessage;
    public string ThreadId { get; } = threadId;

    public bool IsTrace { get; } = itemLevel == LogMessageType.Trace;
    public bool IsDebug { get; } = itemLevel == LogMessageType.Debug;
    public bool IsInfo { get; } = itemLevel == LogMessageType.Info;
    public bool IsWarning { get; } = itemLevel == LogMessageType.Warning;
    public bool IsError { get; } = itemLevel == LogMessageType.Error;
    public bool IsFatal { get; } = itemLevel == LogMessageType.Fatal;
}