using Avalonia.Controls;

namespace Asv.Drones.Gui.Api;

public static class DesignTime
{
    public static void ThrowIfNotDesignMode()
    {
        if (Design.IsDesignMode == false)
            throw new InvalidOperationException("This method is for design mode only");
    }

    public static ILogService Log => NullLogService.Instance;
}