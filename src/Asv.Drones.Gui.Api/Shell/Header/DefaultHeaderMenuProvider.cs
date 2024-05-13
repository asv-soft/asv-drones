using System.Composition;
using DynamicData;

namespace Asv.Drones.Gui.Api
{
    [Export(WellKnownUri.ShellHeaderMenu, typeof(IViewModelProvider<IMenuItem>))]
    public class DefaultHeaderMenuProvider : ViewModelProviderBase<IMenuItem>
    {
        [ImportingConstructor]
        public DefaultHeaderMenuProvider([ImportMany(WellKnownUri.ShellHeaderMenu)] IEnumerable<IMenuItem> menuItems)
        {
            Source.AddOrUpdate(menuItems);
        }
    }
}