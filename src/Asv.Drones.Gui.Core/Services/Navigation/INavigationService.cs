using Avalonia.Platform.Storage;

namespace Asv.Drones.Gui.Core
{
    /// <summary>
    /// This class provides helper methods for navigation in the application.
    /// </summary>
    public class NavigationServiceHelper
    {
    }

    /// <summary>
    /// An interface for navigating between pages in an application.
    /// </summary>
    public interface INavigationService
    {
        /// <summary>
        /// Initializes the storage provider for the given window.
        /// </summary>
        /// <param name="windowStorageProvider">The storage provider to be initialized.</param>
        void InitStorageProvider(IStorageProvider windowStorageProvider);

        /// <summary>
        /// Initializes the application with the specified shell. </summary> <param name="shell">The shell object used for initializing the application.</param> <returns>void</returns>
        /// /
        void Init(IShell shell);

        /// <summary>
        /// Navigates to the specified URI.
        /// </summary>
        /// <param name="uri">The URI to navigate to.</param>
        /// <returns>A task representing the asynchronous operation. The task result represents whether the navigation was successful or not.</returns>
        Task<bool> GoTo(Uri uri);
    }
}