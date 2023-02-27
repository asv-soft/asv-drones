using System.ComponentModel.Composition;
using DynamicData;

namespace Asv.Drones.Gui.Core
{
    [Export(typeof(IViewModelProvider<IShellStatusItem>))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class DefaultShellStatusProvider : ViewModelProviderBase<IShellStatusItem>
    {
        [ImportingConstructor]
        public DefaultShellStatusProvider([ImportMany] IEnumerable<IShellStatusItem> exportedMenuItems)
        {
            Source.AddOrUpdate(exportedMenuItems);
        }
    }
}