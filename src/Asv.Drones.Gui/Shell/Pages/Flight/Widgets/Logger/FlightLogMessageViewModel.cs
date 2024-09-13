using System;
using Asv.Drones.Gui.Api;
using Material.Icons;
using Microsoft.Extensions.Logging;

namespace Asv.Drones.Gui;

public class FlightLogMessageViewModel(MaterialIconKind icon, string message, LogLevel type)
{
    public MaterialIconKind IconKind { get; set; } = icon;
    public string Message { get; set; } = message;
    public LogLevel Type { get; set; } = type;
    public Guid Id { get; } = Guid.NewGuid();
}