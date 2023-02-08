namespace Asv.Drones.Gui.Core
{
    public interface IShellPage : IDisposable
    {
        void SetArgs(Uri link);
    }
}