namespace Asv.Drones.Gui.Api;

public class ProgressMessage(double progress, string message)
{
    public string Message => message;
    public double Progress => progress;
}