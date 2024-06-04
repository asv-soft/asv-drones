using Asv.Mavlink;

namespace Asv.Drones.Gui.Api;

/// <summary>
/// Represents a service for storing and retrieving data files associated with ASV SDR record file metadata.
/// </summary>
public interface ISdrStoreService
{
    /// <summary>
    /// Represents a hierarchical store that stores a collection of IListDataFile with AsvSdrRecordFileMetadata metadata, using Guid as the key.
    /// </summary>
    /// <value>
    /// The hierarchical store.
    /// </value>
    IHierarchicalStore<Guid, IListDataFile<AsvSdrRecordFileMetadata>> Store { get; }
}