using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Asv.Common;
using Asv.Drones.Gui.Core;
using Asv.Mavlink;
using Asv.Mavlink.V2.AsvSdr;
using Avalonia.Controls;
using DynamicData;
using DynamicData.Binding;
using FluentAvalonia.UI.Controls;
using Material.Icons;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

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
        using var file = _svc.Store.Open(itemValue.Id);
        return file.File.ReadMetadata().Tags.Select(SdrTagViewModelHelper.ConvertToTag).ToImmutableArray();
    }


    public ILocalizationService Localization => _loc;
}