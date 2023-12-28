using DynamicData;
using Material.Icons;

namespace Asv.Drones.Gui.Core
{
    /// <summary>
    /// All pages in the shell must implement this interface.
    /// </summary>
    public interface IShellPage : IViewModel
    {
        /// <summary>
        /// Gets the material icon kind for this property.
        /// </summary>
        /// <value>
        /// The material icon kind.
        /// </value>
        MaterialIconKind Icon { get; }

        /// <summary>
        /// Gets the title of the property.
        /// </summary>
        /// <returns>A string representing the title.</returns>
        string Title { get; }

        /// <summary>
        /// Gets the observable collection of header menu items along with their corresponding URIs.
        /// </summary>
        /// <value>
        /// The observable collection of header menu items and their URIs.
        /// </value>
        IObservable<IChangeSet<IHeaderMenuItem, Uri>> HeaderItems { get; }

        /// <summary>
        /// Gets the collection of status items.
        /// </summary>
        /// <remarks>
        /// This property returns an <see cref="IObservable{T}"/> where T is an instance of <see cref="IChangeSet{TObject, TIdentifier}"/>.
        /// The TObject represents an instance of <see cref="IShellStatusItem"/> and TIdentifier is <see cref="Uri"/>.
        /// </remarks>
        /// <returns>An IObservable representing the collection of status items.</returns>
        IObservable<IChangeSet<IShellStatusItem, Uri>> StatusItems { get; }

        /// <summary>
        /// Sets additional arguments for the page.
        /// </summary>
        /// <param name="link">The URI link specifying the additional arguments.</param>
        void SetArgs(Uri link);

        /// <summary>
        /// Attempts to close the current instance.
        /// </summary>
        /// <returns>
        /// A <see cref="Task{TResult}"/> representing the asynchronous operation.
        /// The <see cref="Task{TResult}"/> will return <see langword="true"/> if the instance was successfully closed;
        /// otherwise, it will return <see langword="false"/>.
        /// </returns>
        Task<bool> TryClose();
    }
}