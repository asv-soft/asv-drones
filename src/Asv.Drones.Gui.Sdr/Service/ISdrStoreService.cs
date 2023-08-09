using System.ComponentModel.Composition;
using Asv.Common;
using Asv.Drones.Gui.Core;
using Asv.Mavlink;
using LiteDB;

namespace Asv.Drones.Gui.Sdr;



public interface ISdrStoreService
{
    IAsvSdrStore Store { get; }
}



[Export(typeof(ISdrStoreService))]
[PartCreationPolicy(CreationPolicy.Shared)]
public class SdrStoreService : DisposableOnceWithCancel, ISdrStoreService
{
    private const string RecordFolderName = "sdr_data";
   
    [ImportingConstructor]
    public SdrStoreService(IAppService svc)
    {
        var dir = Path.Combine(svc.Paths.DataFolder, RecordFolderName);
        if (Directory.Exists(dir) == false)
        {
            Directory.CreateDirectory(dir);
        }
        Store = new AsvSdrRecordStore(dir).DisposeItWith(Disposable);
    }

    public IAsvSdrStore Store { get; }
}



