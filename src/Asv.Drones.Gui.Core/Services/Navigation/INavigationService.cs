namespace Asv.Drones.Gui.Core
{
    public class NavigationServiceHelper
    {

    }

    public interface INavigationService
    {
        void Init(IShell shell);
        void GoTo(Uri uri);
    }
}
