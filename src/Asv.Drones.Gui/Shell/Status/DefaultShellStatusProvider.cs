using System.Collections.Generic;
using System.Composition;
using Asv.Drones.Gui.Api;
using DynamicData;

namespace Asv.Drones.Gui
{
    [Export(typeof(IViewModelProvider<IShellStatusItem>))]
    public class DefaultShellStatusProvider : ViewModelProviderBase<IShellStatusItem>
    {
        [ImportingConstructor]
        public DefaultShellStatusProvider([ImportMany] IEnumerable<IShellStatusItem> exportedMenuItems)
        {
            Source.AddOrUpdate(exportedMenuItems);
        }
    }
}