using Asv.Common;
using Asv.Mavlink;
using DynamicData;

namespace Asv.Drones.Gui.Uav
{
    /// <summary>
    /// Represents a service for managing MAVlink devices.
    /// </summary>
    public interface IMavlinkDevicesService
    {
        /// <summary>
        /// Collection with all devices in the network.
        /// </summary>
        /// <remarks>
        /// The Devices property is an IObservable collection that contains all the devices in the network.
        /// </remarks>
        /// <value>
        /// An IObservable collection of IChangeSet of IMavlinkDevice objects that represents all the devices in the network.
        /// </value>
        IObservable<IChangeSet<IMavlinkDevice, ushort>> Devices { get; }

        /// <summary>
        /// Gets or sets the timeout for device connection.
        /// </summary>
        /// <remarks>
        /// If the device does not respond within this time, it will be removed from the collection.
        /// </remarks>
        /// <value>
        /// The timeout value, in TimeSpan format.
        /// </value>
        IRxEditableValue<TimeSpan> DeviceTimeout { get; }

        /// <summary>
        /// Gets or sets the Mavlink router.
        /// </summary>
        IMavlinkRouter Router { get; }

        /// <summary>
        /// Gets a value indicating whether the application needs to be reloaded in order to apply the new config.
        /// </summary>
        /// <value>
        /// <c>true</c> if the application needs to be reloaded; otherwise, <c>false</c>.
        /// </value>
        IRxValue<bool> NeedReloadToApplyConfig { get; }

        /// <summary>
        /// ComponentId identifier of this app in mavlink network
        /// </summary>
        /// <returns>The ComponentId</returns>
        IRxEditableValue<byte> ComponentId { get; }

        /// <summary>
        /// SystemId identifier of this app in mavlink network
        /// </summary>
        IRxEditableValue<byte> SystemId { get; }

        /// <summary>
        /// Rate of heartbeat packets for sending to network.
        /// </summary>
        /// <remarks>
        /// This property represents the rate at which heartbeat packets are sent to the network.
        /// Heartbeat packets are used to maintain a connection between devices and ensure that they are still active.
        /// The value of this property is an <see cref="IRxEditableValue{T}"/> object that allows the rate to be modified.
        /// </remarks>
        IRxEditableValue<TimeSpan> HeartbeatRate { get; }

        /// <summary>
        /// Gets the list of all founded vehicles in the network.
        /// </summary>
        /// <remarks>
        /// The vehicles are observed using the <see cref="IObservable{T}"/> interface.
        /// The sequence contains changes to the vehicles along with the reason for the change represented by the <see cref="IChangeSet{TObject,TKey}"/> interface.
        /// Each vehicle is represented by an instance of <see cref="IVehicleClient"/> interface.
        /// </remarks>
        IObservable<IChangeSet<IVehicleClient, ushort>> Vehicles { get; }

        /// <summary>
        /// Gets a vehicle by its id.
        /// </summary>
        /// <param name="id">The id of the vehicle to be searched.</param>
        /// <returns>Returns the vehicle object representing the found vehicle. Returns null if no vehicle with the specified id is found.</returns>
        public IVehicleClient? GetVehicleByFullId(ushort id);

        /// <summary>
        /// Gets the base stations.
        /// </summary>
        /// <value>
        /// The base stations.
        /// </value>
        IObservable<IChangeSet<IGbsClientDevice, ushort>> BaseStations { get; }

        /// <summary>
        /// Retrieves the GbsClientDevice object associated with the specified full ID.
        /// </summary>
        /// <param name="id">The full ID of the GbsClientDevice to retrieve.</param>
        /// <returns>
        /// The GbsClientDevice object associated with the specified full ID, or null if no device was found.
        /// </returns>
        public IGbsClientDevice? GetGbsByFullId(ushort id);

        /// <summary>
        /// Gets the observable sequence of payloads.
        /// </summary>
        /// <remarks>
        /// The payloads are represented by <see cref="IChangeSet{TObject,TKey}"/> which contains a collection of change sets.
        /// Each change set represents the changes made to the collection of <see cref="ISdrClientDevice"/> objects.
        /// </remarks>
        /// <value>
        /// The observable sequence of payloads.
        /// </value>
        IObservable<IChangeSet<ISdrClientDevice, ushort>> Payloads { get; }

        /// <summary>
        /// Retrieves the payloads associated with the specified full ID.
        /// </summary>
        /// <param name="id">The full ID of the payloads to retrieve.</param>
        /// <returns>An instance of ISdrClientDevice representing the payloads associated with the specified full ID, or null if no payloads found.</returns>
        public ISdrClientDevice? GetPayloadsByFullId(ushort id);

        /// <summary>
        /// Gets an observable sequence of changes to the collection of AdsbClientDevice objects.
        /// </summary>
        /// <value>
        /// An observable sequence of changes that occurred on the collection of AdsbClientDevice objects.
        /// </value>
        public IObservable<IChangeSet<IAdsbClientDevice, ushort>> AdsbDevices { get; }

        /// <summary>
        /// Retrieves an AdsbClientDevice object based on the provided full ID.
        /// </summary>
        /// <param name="id">The full ID of the AdsbClientDevice.</param>
        /// <returns>An instance of the AdsbClientDevice class representing the requested vehicle, or null if not found.</returns>
        public IAdsbClientDevice? GetAdsbVehicleByFullId(ushort id);
    }

    
}