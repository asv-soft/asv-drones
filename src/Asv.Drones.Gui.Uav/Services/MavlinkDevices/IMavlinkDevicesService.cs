using Asv.Common;
using Asv.Mavlink;
using DynamicData;

namespace Asv.Drones.Gui.Uav
{
    public interface IMavlinkDevicesService
    {
        IObservable<IChangeSet<IMavlinkDevice, ushort>> Devices { get; }
        IRxEditableValue<TimeSpan> DeviceTimeout { get; }
        IMavlinkRouter Router { get; }

        IRxValue<bool> NeedReloadToApplyConfig { get; }
        IRxEditableValue<byte> ComponentId { get; }
        IRxEditableValue<byte> SystemId { get; }
        IRxEditableValue<TimeSpan> HeartbeatRate { get; }
        
    }

    
}