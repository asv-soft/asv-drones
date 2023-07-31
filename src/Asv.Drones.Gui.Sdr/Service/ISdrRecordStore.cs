using System.Collections.Immutable;
using Asv.Common;
using LiteDB;

namespace Asv.Drones.Gui.Sdr;

public interface ISdrRecordStore
{
    IReadOnlyList<RecordMetadata> GetRecords();
}

public class RecordMetadata
{
    public Guid Id { get; init; }
    public string Name { get; init; }
    public Guid ParentId { get; init; }
}

public interface IRecordDataWriter
{
    
}

public class LiteDbSdrRecordStore : DisposableOnceWithCancel, ISdrRecordStore
{
    private const string MetadataParentId = "ParentId";
    private const string MetadataFileName = "RecordFileName";
    private readonly ILiteStorage<Guid> _storage;
    public LiteDbSdrRecordStore(ILiteStorage<Guid> storage)
    {
        _storage = storage;
    }

    public IReadOnlyList<RecordMetadata> GetRecords()
    {
        return _storage.FindAll().Select(_ => new RecordMetadata
        {
            Id = _.Id,
            Name = _.Metadata[MetadataFileName],
            ParentId = _.Metadata[MetadataParentId],
        }).ToImmutableList();
    }

    public bool Delete(Guid id)
    {
        return _storage.Delete(id);
    }

    public bool IsExist(Guid id)
    {
        return _storage.Exists(id);
    }

    public bool Rename(Guid id, string name)
    {
        var item = _storage.FindById(id);
        if (item == null) return false;
        // TODO: add validation
        item.Metadata[MetadataFileName] = name;
        return _storage.SetMetadata(id, item.Metadata);
    }
}