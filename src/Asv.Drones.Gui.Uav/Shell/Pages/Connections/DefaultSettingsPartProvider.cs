using System.ComponentModel.Composition;
using Asv.Drones.Gui.Core;
using DynamicData;

namespace Asv.Drones.Gui.Uav
{
    [Export(typeof(IViewModelProvider<IConnectionPart>))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class DefaultSettingsPartProvider : ViewModelProviderBase<IConnectionPart>
    {
        [ImportingConstructor]
        public DefaultSettingsPartProvider([ImportMany] IEnumerable<IConnectionPart> exportedMenuItems)
        {
            Source.AddOrUpdate(exportedMenuItems);
        }
    }
}