using Asv.Drones.Gui.Core;
using System.ComponentModel.Composition;
using DynamicData;

namespace Asv.Drones.Gui.Uav
{
    

    public interface IConnectionPart : IViewModel
    {
        int Order { get; }
    }

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