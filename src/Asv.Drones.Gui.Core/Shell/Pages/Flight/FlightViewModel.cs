using ReactiveUI;
using System.ComponentModel.Composition;

namespace Asv.Drones.Gui.Core
{
    [ExportShellPage(BaseUriString)]
    [PartCreationPolicy(CreationPolicy.Shared)] //Impotent shared mode
    public class FlightViewModel: DisposableViewModelBase,IShellPage
    {
        public const string BaseUriString = "asv:shell.flight";
        public static readonly Uri BaseUri = new(BaseUriString);

        public void SetArgs(Uri link)
        {
            
        }
    }
}