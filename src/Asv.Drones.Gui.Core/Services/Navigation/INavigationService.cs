using Avalonia.Platform.Storage;

namespace Asv.Drones.Gui.Core
{
    public class NavigationServiceHelper
    {

    }

    public interface INavigationService
    {
        void InitStorageProvider(IStorageProvider windowStorageProvider);
        void Init(IShell shell);
        /// <summary>
        /// Navigate to IShell page by URI
        /// </summary>
        /// <param name="uri"></param>    
        void GoTo(Uri uri);

    }
}
