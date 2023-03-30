using System.ComponentModel.Composition;
using Asv.Cfg;
using Asv.Drones.Gui.Core;
using Asv.Drones.Gui.Uav;
using Asv.Mavlink;
using Asv.Mavlink.V2.AsvGbs;
using DynamicData;
using DynamicData.PLinq;
using MavType = Asv.Mavlink.V2.Common.MavType;

namespace Asv.Drones.Gui.Gbs;

public interface IMavlinkGbsService
{
    /// <summary>
    /// List of all founded ground base stations in network
    /// </summary>
    IObservable<IChangeSet<IGbsDevice, ushort>> BaseStations { get; }
}

public class MavlinkGbsServiceConfig
{
    
}

[Export(typeof(IMavlinkGbsService))]
[PartCreationPolicy(CreationPolicy.Shared)]
public class MavlinkGbsService : ServiceWithConfigBase<MavlinkGbsServiceConfig>, IMavlinkGbsService
{
    [ImportingConstructor]
    public MavlinkGbsService(IConfiguration cfg,IMavlinkDevicesService mavlink,IPacketSequenceCalculator seq) : base(cfg)
    {
        BaseStations = mavlink.Devices
            .Filter(_ => _.Type == (MavType)Mavlink.V2.AsvGbs.MavType.MavTypeAsvGbs)
            .Transform(_ => (IGbsDevice)new GbsDevice(mavlink,cfg,seq,_)).RefCount();
    }

    public IObservable<IChangeSet<IGbsDevice, ushort>> BaseStations { get; }
}