using System.Collections.Generic;
using System.Composition;
using Asv.Drones.Gui.Api;
using DynamicData;

namespace Asv.Drones.Gui
{
    [Export(typeof(IViewModelProvider<IShellMenuItem>))]
    [Shared]
    public class DefaultShellMenuProvider : ViewModelProviderBase<IShellMenuItem>
    {
        [ImportingConstructor]
        public DefaultShellMenuProvider([ImportMany] IEnumerable<IShellMenuItem> exportedMenuItems)
        {
            Source.AddOrUpdate(exportedMenuItems);
        }
    }
}