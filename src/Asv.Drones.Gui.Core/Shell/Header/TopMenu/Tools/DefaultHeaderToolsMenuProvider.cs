using System.ComponentModel.Composition;
using DynamicData;

namespace Asv.Drones.Gui.Core;

[Export(HeaderMenuItem.UriString + "/tools",typeof(IViewModelProvider<IHeaderMenuItem>))]
[PartCreationPolicy(CreationPolicy.NonShared)]
public class DefaultHeaderToolsMenuProvider : ViewModelProviderBase<IHeaderMenuItem>
{
    [ImportingConstructor]
    public DefaultHeaderToolsMenuProvider([ImportMany(HeaderMenuItem.UriString + "/tools")]IEnumerable<IHeaderMenuItem> exportedMenuItems)
    {
        Source.AddOrUpdate(exportedMenuItems);
    }
}