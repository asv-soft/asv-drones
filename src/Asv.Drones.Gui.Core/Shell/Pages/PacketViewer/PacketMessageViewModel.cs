namespace Asv.Drones.Gui.Core;

public class PacketMessageViewModel
{
    public DateTime DateTime { get; set; }
    public string Source { get; set; }
    public string Message { get; set; }
    public string Type { get; set; }

    public PacketMessageViewModel(DateTime dateTime, string source, string message, string type)
    {
        DateTime = dateTime;
        Source = source;
        Message = message;
        Type = type;
    }
}