using System.ComponentModel.Composition;
using System.Diagnostics;
using Asv.Common;
using Asv.Drones.Gui.Core;
using Asv.Mavlink;
using Asv.Mavlink.V2.AsvSdr;
using LiteDB;

namespace Asv.Drones.Gui.Sdr;



public interface ISdrStoreService
{
    IEnumerable<ISdrStoreEntry> QueryAll();
    Guid AddNewFolder(string name, Guid parentId);
    void DeleteEntity(Guid id);
    void RenameEntity(Guid id, string name);
    IAsvSdrRecordFile Open(Guid recId);
}



[Export(typeof(ISdrStoreService))]
[PartCreationPolicy(CreationPolicy.Shared)]
public class SdrStoreService : DisposableOnceWithCancel, ISdrStoreService
{
    private readonly ILiteCollection<SdrFolderModel> _folderCollection;
    private readonly ILiteStorage<Guid> _recordsStore;
    private readonly string _dataFolder;
    public const string SdrRecordsFolder = "sdr_records";

    private const string RecordFilesCollectionName = "sdr_records";
    private const string RecordChunksCollectionName = "sdr_records_chunks";
    private const string RecordFolderCollectionName = "sdr_folders";
    private const string MetadataParentId = "ParentId";
    private const string MetadataFileName = "RecordFileName";
    [ImportingConstructor]
    public SdrStoreService(IAppService svc)
    {
        _dataFolder = Path.Combine(svc.Paths.DataFolder, SdrRecordsFolder);
        if (Directory.Exists(_dataFolder) == false)
        {
            Directory.CreateDirectory(_dataFolder);
        }
        
        
        _folderCollection = svc.Store.Db.GetCollection<SdrFolderModel>(RecordFolderCollectionName);
        _recordsStore = svc.Store.Db.GetStorage<Guid>(RecordFilesCollectionName, RecordChunksCollectionName);
    }
    
    public IEnumerable<ISdrStoreEntry> QueryAll()
    {
        var records = _recordsStore.FindAll().Select(_ => new SdrStoreEntry
        {
            Id = _.Id,
            Name = _.Metadata[MetadataFileName],
            ParentId = _.Metadata[MetadataParentId],
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
            var subFolders = _folderCollection.Query().Where(x => x.ParentId == id).ToArray();
            _folderCollection.Delete(id);
            foreach (var subFolder in subFolders)
            {
                DeleteEntity(subFolder.Id);
            }
            var subMissions = _recordsStore.Find(x=>x.Metadata[MetadataParentId].Equals(id)).ToArray();
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
        SdrWellKnown.CheckRecordName(name);
        var folder = _folderCollection.FindOne(x => x.Id == id);
        if (folder != null)
        {
            folder.Name = name;
            _folderCollection.Update(folder);
        }
        else
        {
            var item = _recordsStore.FindById(id);
            if (item == null) return;
            item.Metadata[MetadataFileName] = name;
            using var file = Open(id);
            var fileRecId = new Guid(file.ReadMetadata().Info.RecordGuid);
            Debug.Assert(fileRecId == id);
            file.EditMetadata(x=>
            {
                MavlinkTypesHelper.SetString(x.Info.RecordName,name);
            });
            _recordsStore.SetMetadata(id, item.Metadata);
        }
    }

    public IAsvSdrRecordFile Open(Guid recId)
    {
        var filePath = Path.Combine(_dataFolder, recId.ToString("N"));
        var file = File.Open(filePath, FileMode.Create, FileAccess.ReadWrite);
        return new AsvSdrRecordFile( file, true);
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



