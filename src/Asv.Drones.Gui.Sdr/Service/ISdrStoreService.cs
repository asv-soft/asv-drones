using System.ComponentModel.Composition;
using Asv.Common;
using Asv.Drones.Gui.Core;
using LiteDB;

namespace Asv.Drones.Gui.Sdr;



public interface ISdrStoreService
{
    IEnumerable<ISdrStoreEntry> QueryAll();
    Guid AddNewFolder(string name, Guid parentId);
    void DeleteEntity(Guid id);
    void RenameEntity(Guid id, string name);
    IRecordDataWriter OpenWrite(Guid recId);
}



[Export(typeof(ISdrStoreService))]
[PartCreationPolicy(CreationPolicy.Shared)]
public class SdrStoreService : DisposableOnceWithCancel, ISdrStoreService
{
    private readonly ILiteCollection<SdrFolderModel> _folderCollection;
    private readonly LiteDbSdrRecordStore _recordsStore;

    private const string RecordFilesCollectionName = "sdr_records";
    private const string RecordChunksCollectionName = "sdr_records_chunks";
    private const string RecordFolderCollectionName = "sdr_folders";
    [ImportingConstructor]
    public SdrStoreService(IAppService svc)
    {
        _folderCollection = svc.Store.Db.GetCollection<SdrFolderModel>(RecordFolderCollectionName);
        _recordsStore = new LiteDbSdrRecordStore(svc.Store.Db.GetStorage<Guid>(RecordFilesCollectionName, RecordChunksCollectionName));
    }
    
    public IEnumerable<ISdrStoreEntry> QueryAll()
    {
        var records = _recordsStore.GetRecords().Select(m => new SdrStoreEntry
        {
            Id = m.Id,
            Name = m.Name,
            ParentId = m.ParentId,
            Type = StoreEntryType.Record
        });
        var folders = _folderCollection.Query().Select(m => new SdrStoreEntry
        {
            Id = m.Id,
            Name = m.Name,
            ParentId = m.ParentId,
            Type = StoreEntryType.Folder
        });
        return folders.ToEnumerable().Concat(records);
    }

    public Guid AddNewFolder(string name, Guid parentId)
    {
        // TODO: check if folder with same name already exists and parent exists
        var id = Guid.NewGuid();
        _folderCollection.Insert(new SdrFolderModel()
        {
            Id = id,
            Name = name,
            ParentId = parentId,
        });
        return id;
    }

    public void DeleteEntity(Guid id)
    {
        var folder = _folderCollection.FindOne(x => x.Id == id);
        if (folder != null)
        {
            var subFolders = _folderCollection.Query().Where(_ => _.ParentId == id).ToArray();
            _folderCollection.Delete(id);
            foreach (var subFolder in subFolders)
            {
                DeleteEntity(subFolder.Id);
            }
            var subMissions = _recordsStore.GetRecords().Where(_ => _.ParentId == id).ToArray();
            foreach (var subMission in subMissions)
            {
                _recordsStore.Delete(subMission.Id);
            }
        }
        else
        {
            _recordsStore.Delete(id);
        }
    }

    public void RenameEntity(Guid id, string name)
    {
        var folder = _folderCollection.FindOne(x => x.Id == id);
        if (folder != null)
        {
            folder.Name = name;
            _folderCollection.Update(folder);
        }
        else
        {
            _recordsStore.Rename(id, name);
        }
    }

    public IRecordDataWriter OpenWrite(Guid recId)
    {
        return _recordsStore.OpenWrite();
    }
}

public class SdrFolderModel
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public Guid ParentId { get; set; }
}

public interface ISdrStoreEntry
{
    public Guid Id { get; }
    public string Name { get; }
    public StoreEntryType Type { get; }
    public Guid ParentId { get; }
}

public class SdrStoreEntry : ISdrStoreEntry
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public StoreEntryType Type { get; set; }
    public Guid ParentId { get; set; }
}

public enum StoreEntryType
{
    Record,
    Folder
}



