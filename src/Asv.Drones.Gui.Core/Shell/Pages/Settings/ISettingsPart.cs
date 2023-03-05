using DynamicData;
using System.ComponentModel.Composition;

namespace Asv.Drones.Gui.Core
{
    public interface ISettingsPart: IViewModel
    {
        int Order { get; }
        
        bool IsRebootRequired { get; }
    }

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