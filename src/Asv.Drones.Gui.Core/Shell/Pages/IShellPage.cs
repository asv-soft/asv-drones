using Material.Icons;

namespace Asv.Drones.Gui.Core
{
    /// <summary>
    /// All pages in shell must implement this interface
    /// </summary>
    public interface IShellPage : IViewModel
    {
        MaterialIconKind Icon { get; }
        string Title { get; }
        /// <summary>
        /// Addition arguments for page
        /// </summary>
        /// <param name="link"></param>
        void SetArgs(Uri link);
    }
}