using Material.Icons;

namespace Asv.Drones.Gui.Core;

public class FlightLogMessageViewModel
{
    public MaterialIconKind IconKind { get; set; }
    public string Message { get; set; }
    public LogMessageType Type { get; set; }
    public Guid Id { get; }

    public FlightLogMessageViewModel(MaterialIconKind icon, string message, LogMessageType type)
    {
        IconKind = icon;
        Message = message;
        Type = type;
        Id = Guid.NewGuid();
    }
}