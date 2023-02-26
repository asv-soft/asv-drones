using System.ComponentModel.Composition;

namespace Asv.Drones.Gui.Core
{
    [ExportShellPage(BaseUriString)]
    [PartCreationPolicy(CreationPolicy.Shared)] //Important shared mode
    public class FlightViewModel: ViewModelBase,IShellPage
    {
        public const string BaseUriString = "asv:shell.flight";
        public static readonly Uri BaseUri = new(BaseUriString);

        public FlightViewModel():base(BaseUri)
        {
            
        }

        public void SetArgs(Uri link)
        {
            
        }
    }
}