namespace Asv.Drones.Gui.Api
{
    /// <summary>
    /// Main view interface
    /// </summary>
    public interface IShell
    {
        Task<bool> GoTo(Uri uri);
        IShellPage? CurrentPage { get; set; }
    }
}