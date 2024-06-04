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
using ReactiveUI.Fody.Helpers;

namespace Asv.Drones.Gui;

public class SavingBrowserViewModel : HierarchicalStoreViewModel
{
    private readonly IPlaningMission _svc;

    public SavingBrowserViewModel() : base()
    {
        DesignTime.ThrowIfNotDesignMode();
    }

    [ImportingConstructor]
    public SavingBrowserViewModel(IPlaningMission svc, ILogService log) : base()
    {
        _svc = svc;
        using var a = Refresh.Execute().Subscribe();
    }
    
    [Reactive] public string FileName { get; set; }
    
    protected override void RefreshImpl()
    {
        if (_svc.MissionStore is FileSystemHierarchicalStore<Guid, PlaningMissionFile> fileStore)
        {
            fileStore.UpdateEntries();
        }

        base.RefreshImpl();
    }

    public Guid DialogResult { get; set; }

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