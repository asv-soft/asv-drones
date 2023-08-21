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
    public string SdrRecordFolder { get; set; } = "sdr_data";
    public int FileCacheTimeMs { get; set; } = 10_000;
}

[Export(typeof(ISdrStoreService))]
[PartCreationPolicy(CreationPolicy.Shared)]
public class SdrStoreService : ServiceWithConfigBase<SdrStoreServiceConfig>, ISdrStoreService
{
    [ImportingConstructor]
    public SdrStoreService(IAppService svc, IConfiguration cfg) : base(cfg)
    {
        var dataFolder = InternalGetConfig(x => x.SdrRecordFolder);
        var fileCacheTimeMs = InternalGetConfig(x => x.FileCacheTimeMs);
        var dir = Path.Combine(svc.Paths.DataFolder, dataFolder);
        if (Directory.Exists(dir) == false)
        {
            Directory.CreateDirectory(dir);
        }
        Store = new AsvSdrStore(dir, TimeSpan.FromMilliseconds(fileCacheTimeMs)).DisposeItWith(Disposable);
    }

    public IHierarchicalStore<Guid,IListDataFile<AsvSdrRecordFileMetadata>> Store { get; }
}



