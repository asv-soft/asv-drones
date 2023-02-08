using System.ComponentModel.Composition;

namespace Asv.Drones.Gui.Core
{
    [ExportShellPage(BaseUriString)]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class PlaningViewModel:DisposableViewModelBase,IShellPage
    {
        public const string BaseUriString = "asv:shell.planing";
        public static readonly Uri BaseUri = new(BaseUriString);

        public void SetArgs(Uri link)
        {
            
        }
    }
}