using System.ComponentModel.Composition;

namespace Asv.Drones.Gui.Core
{
    [ExportShellPage(WellKnownUri.ShellPagePlaning)]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class PlaningViewModel:ViewModelBase,IShellPage
    {
        public PlaningViewModel():base(new(WellKnownUri.ShellPagePlaning))
        {
            
        }

        public void SetArgs(Uri link)
        {
            
        }

        
    }
}