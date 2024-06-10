using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Reactive.Linq;
using Asv.Drones.Gui.Api;
using Asv.Mavlink;
using Asv.Mavlink.V2.Common;
using Avalonia.Controls;
using Avalonia.Media;
using DynamicData.Binding;
using FluentAvalonia.UI.Controls;
using Material.Icons;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Asv.Drones.Gui;

public class PlaningMissionSavingBrowserViewModel : HierarchicalStoreViewModel<Guid, PlaningMissionFile>
{
    private readonly IPlaningMission _svc;

    public PlaningMissionSavingBrowserViewModel() : base()
    {
        DesignTime.ThrowIfNotDesignMode();
    }

    [ImportingConstructor]
    public PlaningMissionSavingBrowserViewModel(IPlaningMission svc, ILogService log) : base(
        WellKnownUri.ShellPageMapPlaningMissionSavingBrowserUri, svc.MissionStore, log)
    {
        _svc = svc;
        using var a = Refresh.Execute().Subscribe();
    }
    
    [Reactive] public string FileName { get; set; } = string.Empty;

    protected override void RefreshImpl()
    {
        if (_svc.MissionStore is FileSystemHierarchicalStore<Guid, PlaningMissionFile> fileStore)
        {
            fileStore.UpdateEntries();
        }

        base.RefreshImpl();
    }

    private IObservable<bool> CanSave => this.WhenPropertyChanged(x => x.FileName)
        .Select(x => x.Value != string.Empty);

    protected override Guid GenerateNewId() => Guid.NewGuid();

    public void ApplyDialog(ContentDialog dialog)
    {
        dialog.PrimaryButtonCommand = ReactiveCommand.Create(() =>
        {
            if (SelectedItem != null)
            {
            }
        }, CanSave.Do(v => dialog.IsPrimaryButtonEnabled = v));
    }

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

    public void SaveAsImpl(out Guid id)
    {
        Guid parentId;
        Guid fileId = default;
        if (SelectedItem != null)
        {
            parentId =
                (Guid)(SelectedItem.Type == FolderStoreEntryType.Folder ? SelectedItem.Id : SelectedItem.ParentId);
        }
        else
        {
            parentId = _svc.MissionStore.RootFolderId;
        }

        try
        {
            using var file = _svc.MissionStore.CreateFile(GenerateNewId(), FileName, parentId);
            fileId = file.Id;
        }
        catch (HierarchicalStoreFolderAlreadyExistException)
        {
            var dialog = new ContentDialog
            {
                Title = RS.PlanningPageViewModel_MissionSavingBrowserDialog_ConfirmationText_Title,
                PrimaryButtonText = RS.PlanningPageViewModel_MissionSavingBrowserDialog_PrimaryButton,
                Content = new TextBlock
                    { Text = RS.PlanningPageViewModel_MissionSavingBrowserDialog_ConfirmationText_ContentText }
            };

            var result = dialog.ShowAsync();
            if (result.Result == ContentDialogResult.Primary)
            {
                id = default;
            }
        }

        id = fileId;
        Refresh.Execute().Subscribe(_ => { });
    }
}