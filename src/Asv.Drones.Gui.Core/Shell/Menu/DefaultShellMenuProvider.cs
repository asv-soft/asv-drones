using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Reactive.Linq;
using DynamicData;
using Material.Icons;
using ReactiveUI;

namespace Asv.Drones.Gui.Core
{
    [Export(typeof(IViewModelProvider<IShellMenuItem>))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class DefaultShellMenuProvider : ViewModelProviderBase<IShellMenuItem>
    {
        [ImportingConstructor]
        public DefaultShellMenuProvider([ImportMany]IEnumerable<IShellMenuItem> exportedMenuItems)
        {
            Source.AddOrUpdate(exportedMenuItems);
        }
    }
}