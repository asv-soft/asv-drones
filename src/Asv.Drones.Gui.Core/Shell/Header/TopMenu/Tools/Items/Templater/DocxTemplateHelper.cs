namespace Asv.Drones.Gui.Core;

internal static class DocxTemplateHelper
{
    public static long Inches(this double size)
    {
        return (long)(size * 5000);
    }
}