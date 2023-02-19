using Asv.Drones.Gui.Core;
using Asv.Mavlink;
using ReactiveUI.Fody.Helpers;

namespace Asv.Drones.Gui.Uav
{
    public class DeviceViewModel: ViewModelBase
    {
        [Reactive]
        public string Name { get; set; }

        public DeviceViewModel():base(new Uri(ConnectionsViewModel.BaseUri, $"devices.{Guid.NewGuid()}"))
        {
            
        }

        public DeviceViewModel(IMavlinkDeviceInfo info) : base(new Uri(ConnectionsViewModel.BaseUri,$"devices.{info.FullId}"))
        {

        }
    }
}