using System.ComponentModel.Composition;
using DynamicData;

namespace Asv.Drones.Gui.Core
{
    [Export(HeaderMenuItem.UriString + "/file",typeof(IViewModelProvider<IHeaderMenuItem>))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class DefaultHeaderFileMenuProvider : ViewModelProviderBase<IHeaderMenuItem>
    {
        [ImportingConstructor]
        public DefaultHeaderFileMenuProvider([ImportMany(HeaderMenuItem.UriString + "/file")]IEnumerable<IHeaderMenuItem> exportedMenuItems)
        {
            Source.AddOrUpdate(exportedMenuItems);
        }
    }
}