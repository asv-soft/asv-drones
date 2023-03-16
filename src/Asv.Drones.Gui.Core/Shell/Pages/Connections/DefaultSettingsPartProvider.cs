using System.ComponentModel.Composition;
using DynamicData;

namespace Asv.Drones.Gui.Core
{
    [Export(typeof(IViewModelProvider<IConnectionPart>))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class DefaultConnectionSettingsPartProvider : ViewModelProviderBase<IConnectionPart>
    {
        [ImportingConstructor]
        public DefaultConnectionSettingsPartProvider([ImportMany] IEnumerable<IConnectionPart> exportedMenuItems)
        {
            Source.AddOrUpdate(exportedMenuItems);
        }
    }
}