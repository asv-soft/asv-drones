using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Reactive.Linq;
using Asv.Drones.Gui.Api;
using Asv.Mavlink;
using Asv.Mavlink.V2.Common;
using Avalonia.Media;
using DynamicData.Binding;
using FluentAvalonia.UI.Controls;
using Material.Icons;
using ReactiveUI;

namespace Asv.Drones.Gui;

public class PlaningMissionBrowserViewModel : HierarchicalStoreViewModel<Guid, PlaningMissionFile>
{
    private readonly IPlaningMission _svc;

    public PlaningMissionBrowserViewModel()
    {
        DesignTime.ThrowIfNotDesignMode();
    }

    [ImportingConstructor]
    public PlaningMissionBrowserViewModel(IPlaningMission svc, ILogService log) : base(
        WellKnownUri.ShellPageMapPlaningMissionBrowserUri, svc.MissionStore, log)
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

    private IObservable<bool> CanOpen => this.WhenValueChanged(x => x.SelectedItem)
        .Select(item => item is { Type: FolderStoreEntryType.File });

    protected override Guid GenerateNewId() => Guid.NewGuid();

    public void ApplyDialog(ContentDialog dialog)
    {
        dialog.PrimaryButtonCommand = ReactiveCommand.Create(() =>
        {
            if (SelectedItem != null) DialogResult = (Guid)SelectedItem.Id;
        }, CanOpen.Do(v => dialog.IsPrimaryButtonEnabled = v));
    }

    public Guid DialogResult { get; private set; }

    protected override IReadOnlyCollection<HierarchicalStoreEntryTagViewModel> InternalGetEntryTags(
        IHierarchicalStoreEntry<Guid> itemValue)
    {
        switch (itemValue.Type)
        {
            case FolderStoreEntryType.File:
                using (var file = _svc.MissionStore.OpenFile(itemValue.Id))
                {
                    var item = file.File.Load();
                    return item.Points.Select(x => x.Type).Distinct().Select(ConvertToTag).ToImmutableArray();
                }
            case FolderStoreEntryType.Folder:
                return base.InternalGetEntryTags(itemValue);
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private HierarchicalStoreEntryTagViewModel ConvertToTag(MavCmd argType)
    {
        return new HierarchicalStoreEntryTagViewModel
        {
            Name = argType.ToString("G"),
            Color = Brushes.CornflowerBlue,
            Icon = MaterialIconKind.AlphaL,
        };
    }
}