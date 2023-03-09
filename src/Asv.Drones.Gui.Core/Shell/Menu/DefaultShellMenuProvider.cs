using System.ComponentModel.Composition;
using DynamicData;

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