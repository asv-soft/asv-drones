namespace Asv.Drones.Gui.Core
{
    /// <summary>
    /// Main view interface
    /// </summary>
    public interface IShell
    {
        IShellPage? CurrentPage { get; set; }
    }
}