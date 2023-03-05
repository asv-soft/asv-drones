using System.ComponentModel.Composition;
using DynamicData;

namespace Asv.Drones.Gui.Core
{
    [Export(HeaderFileMenu.UriString,typeof(IViewModelProvider<IHeaderMenuItem>))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class DefaultHeaderFileMenuProvider : ViewModelProviderBase<IHeaderMenuItem>
    {
        [ImportingConstructor]
        public DefaultHeaderFileMenuProvider([ImportMany(HeaderFileMenu.UriString)]IEnumerable<IHeaderMenuItem> exportedMenuItems)
        {
            Source.AddOrUpdate(exportedMenuItems);
        }
    }
}