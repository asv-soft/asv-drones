using System.ComponentModel.Composition;

namespace Asv.Drones.Gui.Core
{
    [ExportShellPage(WellKnownUri.ShellPagePlaningUriString)]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class PlaningViewModel:ViewModelBase,IShellPage
    {
        public PlaningViewModel():base(WellKnownUri.ShellPagePlaningUri)
        {
            
        }

        public void SetArgs(Uri link)
        {
            
        }

        
    }
}