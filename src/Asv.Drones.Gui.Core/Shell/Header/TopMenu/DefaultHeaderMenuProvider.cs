using System.ComponentModel.Composition;
using DynamicData;

namespace Asv.Drones.Gui.Core
{
    [Export(HeaderMenuItem.UriString,typeof(IViewModelProvider<IHeaderMenuItem>))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class DefaultHeaderMenuProvider : ViewModelProviderBase<IHeaderMenuItem>
    {
        [ImportingConstructor]
        public DefaultHeaderMenuProvider([ImportMany(HeaderMenuItem.UriString)]IEnumerable<IHeaderMenuItem> exportedMenuItems)
        {
            Source.AddOrUpdate(exportedMenuItems);
        }
    }
}