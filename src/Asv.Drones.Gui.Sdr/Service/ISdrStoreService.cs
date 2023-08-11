using System.ComponentModel.Composition;
using Asv.Cfg;
using Asv.Common;
using Asv.Drones.Gui.Core;
using Asv.Mavlink;
using LiteDB;

namespace Asv.Drones.Gui.Sdr;



public interface ISdrStoreService
{
    IListDataStore<AsvSdrRecordFileMetadata, Guid> Store { get; }
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
    private const string RecordFolderName = "sdr_data";
   
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
        Store = new ListDataStore<AsvSdrRecordFileMetadata, Guid>(dir, AsvSdrHelper.StoreFormat, AsvSdrHelper.FileFormat, TimeSpan.FromMilliseconds(fileCacheTimeMs)).DisposeItWith(Disposable);
    }

    public IListDataStore<AsvSdrRecordFileMetadata, Guid> Store { get; }
}



