using System.ComponentModel.Composition;
using DynamicData;

namespace Asv.Drones.Gui.Core
{
    [Export(typeof(IViewModelProvider<ISettingsPart>))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class DefaultSettingsPartProvider : ViewModelProviderBase<ISettingsPart>
    {
        [ImportingConstructor]
        public DefaultSettingsPartProvider([ImportMany] IEnumerable<ISettingsPart> exportedMenuItems)
        {
            Source.AddOrUpdate(exportedMenuItems);
        }
    }
}