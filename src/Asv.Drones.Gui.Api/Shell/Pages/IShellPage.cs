using System.Collections.Specialized;
using DynamicData;
using Material.Icons;

namespace Asv.Drones.Gui.Api
{
    /// <summary>
    /// All pages in shell must implement this interface
    /// </summary>
    public interface IShellPage : IViewModel
    {
        MaterialIconKind Icon { get; }
        string Title { get; }
        IObservable<IChangeSet<IMenuItem, Uri>> HeaderItems { get; }
        IObservable<IChangeSet<IShellStatusItem, Uri>> StatusItems { get; }

        /// <summary>
        /// Addition arguments for page
        /// </summary>
        void SetArgs(NameValueCollection args);

        Task<bool> TryClose();
    }
}