using System.Collections.Immutable;
using System.ComponentModel.Composition;
using Asv.Drones.Gui.Core;
using Asv.Mavlink;

namespace Asv.Drones.Gui.Sdr;

[Export]
[PartCreationPolicy(CreationPolicy.NonShared)]
public class SdrStoreBrowserViewModel:HierarchicalStoreViewModel<Guid,IListDataFile<AsvSdrRecordFileMetadata>>
{
    public const string UriString = "asv:sdr.store.browser";
    private readonly ISdrStoreService _svc;
    private readonly ILocalizationService _loc;
    public SdrStoreBrowserViewModel():base()
    {
       
    }

    [ImportingConstructor]
    public SdrStoreBrowserViewModel(ISdrStoreService svc, ILocalizationService loc, ILogService log) : base(
        new Uri(UriString), svc.Store, log)
    {
        _svc = svc ?? throw new ArgumentNullException(nameof(svc));
        _loc = loc ?? throw new ArgumentNullException(nameof(loc));
    }


    protected override Guid GenerateNewId()
    {
        return Guid.NewGuid();
    }

    protected override IReadOnlyCollection<HierarchicalStoreEntryTagViewModel> InternalGetEntryTags(IHierarchicalStoreEntry<Guid> itemValue)
    {
        if (itemValue.Type == FolderStoreEntryType.Folder) return ArraySegment<HierarchicalStoreEntryTagViewModel>.Empty;
        using var file = _svc.Store.OpenFile(itemValue.Id);
        var metadata = file.File.ReadMetadata();
        return SdrTagViewModelHelper.ConvertToTag(metadata).ToImmutableArray();
    }
    
    protected override void RefreshImpl()
    {
        if (_svc.Store is FileSystemHierarchicalStore<Guid, IListDataFile<AsvSdrRecordFileMetadata>> fileStore)
        {
            fileStore.UpdateEntries();
        }
        base.RefreshImpl();
    }

    public ILocalizationService Localization => _loc;
}