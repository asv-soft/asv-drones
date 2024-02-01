using System.ComponentModel.Composition;
using Asv.Cfg;
using Asv.Common;
using Asv.Drones.Gui.Core;
using Asv.Mavlink;
using LiteDB;

namespace Asv.Drones.Gui.Sdr;

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
    IHierarchicalStore<Guid,IListDataFile<AsvSdrRecordFileMetadata>> Store { get; }
}

/// <summary>
/// Configuration class for SdrStoreService.
/// </summary>
public class SdrStoreServiceConfig
{
    /// <summary>
    /// Gets or sets the time, in milliseconds, to cache the files.
    /// </summary>
    /// <value>
    /// The time, in milliseconds, to cache the files. The default value is 10,000 milliseconds (10 seconds).
    /// </value>
    public int FileCacheTimeMs { get; set; } = 10_000;
}

/// <summary>
/// Represents a service for storing and managing SDR (Software Defined Radio) records.
/// </summary>
[Export(typeof(ISdrStoreService))]
[PartCreationPolicy(CreationPolicy.Shared)]
public class SdrStoreService : ServiceWithConfigBase<SdrStoreServiceConfig>, ISdrStoreService
{
    /// <summary>
    /// This constant represents the data folder for SDR records.
    /// </summary>
    public const string SdrRecordsDataFolder = "sdr_records";

    /// SdrStoreService constructor.
    /// @param svc The application service object.
    /// @param cfg The configuration object.
    /// /
    [ImportingConstructor]
    public SdrStoreService(IAppService svc, IConfiguration cfg) : base(cfg)
    {
        var dataFolder = Path.Combine(svc.Paths.DataFolder, SdrRecordsDataFolder);
        if (Directory.Exists(dataFolder) == false)
        {
            Directory.CreateDirectory(dataFolder);
        }
        var fileCacheTimeMs = InternalGetConfig(x => x.FileCacheTimeMs);
        Store = new AsvSdrStore(dataFolder, TimeSpan.FromMilliseconds(fileCacheTimeMs)).DisposeItWith(Disposable);
    }

    /// <summary>
    /// Gets the hierarchical store which stores a collection of data files with associated metadata.
    /// </summary>
    /// <value>
    /// The hierarchical store of data files.
    /// </value>
    public IHierarchicalStore<Guid,IListDataFile<AsvSdrRecordFileMetadata>> Store { get; }
}



