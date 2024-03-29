﻿using System.Collections.Immutable;
using System.ComponentModel.Composition;
using System.Reactive.Linq;
using Asv.Drones.Gui.Core;
using Asv.Mavlink;
using Avalonia.Media;
using DynamicData.Binding;
using FluentAvalonia.UI.Controls;
using Material.Icons;
using ReactiveUI;

namespace Asv.Drones.Gui.Core;

public class PlaningMissionBrowserViewModel : HierarchicalStoreViewModel<Guid,PlaningMissionFile>
{
    private readonly IPlaningMission _svc;

    public const string UriString = "asv:shell.page.mission.mission-browser";

    public PlaningMissionBrowserViewModel() : base()
    {
    }
    
    [ImportingConstructor]
    public PlaningMissionBrowserViewModel(IPlaningMission svc, ILogService log) : base(new Uri(UriString), svc.MissionStore, log)
    {
        _svc = svc;
        using var a = Refresh.Execute().Subscribe();
    }

    protected override void RefreshImpl()
    {
        if (_svc.MissionStore is FileSystemHierarchicalStore<Guid, PlaningMissionFile> fileStore)
        {
            fileStore.UpdateEntries();
        }
        base.RefreshImpl();
    }

    public IObservable<bool> CanOpen => this.WhenValueChanged(x => x.SelectedItem)
        .Select(item => item is { Type: FolderStoreEntryType.File });
    protected override Guid GenerateNewId() => Guid.NewGuid();

    public void ApplyDialog(ContentDialog dialog)
    {
        dialog.PrimaryButtonCommand = ReactiveCommand.Create(() =>
        {
            if (SelectedItem != null) DialogResult = (Guid)SelectedItem.Id;
        },CanOpen.Do(_=>dialog.IsPrimaryButtonEnabled = _));
    }

    public Guid DialogResult { get; set; }

    protected override IReadOnlyCollection<HierarchicalStoreEntryTagViewModel> InternalGetEntryTags(IHierarchicalStoreEntry<Guid> itemValue)
    {
        switch (itemValue.Type)
        {
            case FolderStoreEntryType.File:
                using (var file = _svc.MissionStore.OpenFile(itemValue.Id))
                {
                    var item = file.File.Load();
                    if (item == null) return ArraySegment<HierarchicalStoreEntryTagViewModel>.Empty;
                    return item.Points.Select(x=>x.Type).Distinct().Select(x => ConvertToTag(x)).ToImmutableArray();
                }
            case FolderStoreEntryType.Folder:
                return base.InternalGetEntryTags(itemValue);
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
    
    private HierarchicalStoreEntryTagViewModel ConvertToTag(PlaningMissionPointType argType)
    {
        return new HierarchicalStoreEntryTagViewModel
        {
            Name = argType.ToString("G"),
            Color = Brushes.CornflowerBlue,
            Icon = MaterialIconKind.AlphaL,
        };
    }

}