using System;
using System.Composition;
using Asv.Common;
using Asv.Drones.Gui.Api;
using Asv.Mavlink;
using ReactiveUI.Fody.Helpers;

namespace Asv.Drones.Gui;

public class SavingBrowserViewModel : HierarchicalStoreViewModel
{
    private readonly IPlaningMission _svc;

    public SavingBrowserViewModel()
    {
        DesignTime.ThrowIfNotDesignMode();
    }

    [ImportingConstructor]
    public SavingBrowserViewModel(IPlaningMission svc)
    {
        _svc = svc;
        using var a = Refresh.Execute().Subscribe().DisposeItWith(Disposable);
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
}