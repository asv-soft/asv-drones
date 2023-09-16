using System.ComponentModel.Composition;
using Asv.Cfg;
using Asv.Common;
using Asv.Drones.Gui.Core;
using Asv.Mavlink;
using LiteDB;

namespace Asv.Drones.Gui.Sdr;



public interface ISdrStoreService
{
    IHierarchicalStore<Guid,IListDataFile<AsvSdrRecordFileMetadata>> Store { get; }
}

public class SdrStoreServiceConfig
{
    public int FileCacheTimeMs { get; set; } = 10_000;
}

[Export(typeof(ISdrStoreService))]
[PartCreationPolicy(CreationPolicy.Shared)]
public class SdrStoreService : ServiceWithConfigBase<SdrStoreServiceConfig>, ISdrStoreService
{
    public const string SdrRecordsDataFolder = "sdr_records";
    
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

    public IHierarchicalStore<Guid,IListDataFile<AsvSdrRecordFileMetadata>> Store { get; }
}



