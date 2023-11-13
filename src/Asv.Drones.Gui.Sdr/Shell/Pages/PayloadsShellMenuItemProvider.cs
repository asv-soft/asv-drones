using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using Asv.Common;
using Asv.Drones.Gui.Core;
using Asv.Drones.Gui.Uav;
using Asv.Mavlink;
using DynamicData;
using FluentAvalonia.UI.Controls;
using Material.Icons;

namespace Asv.Drones.Gui.Sdr;


[Export(typeof(IViewModelProvider<IShellMenuItem>))]
[PartCreationPolicy(CreationPolicy.NonShared)]
public class VehiclesShellMenuItemProvider : ViewModelProviderBase<IShellMenuItem>
{
    [ImportingConstructor]
    public VehiclesShellMenuItemProvider(IMavlinkDevicesService svc, CompositionContainer container)
    {
        svc.Payloads.Transform(v=>(IShellMenuItem)new SdrGroupShellMenuItem(v,container))
            .ChangeKey((_, v) => v.Id)
            .DisposeMany()
            .PopulateInto(Source)
            .DisposeItWith(Disposable);
         
    }
}


public class SdrGroupShellMenuItem:ShellMenuItem
{
    public SdrGroupShellMenuItem(ISdrClientDevice sdr, CompositionContainer compositionContainer) : base($"asv:shell.menu.sdr.{sdr.FullId}")
    {
        sdr.Name.Subscribe(x => Name = x).DisposeItWith(Disposable);

        Icon = MaterialIconDataProvider.GetData(SdrIconHelper.DefaultIcon);
        Position = ShellMenuPosition.Top;
        Type = ShellMenuItemType.Group;
        Order = sdr.FullId;

        Items = new ReadOnlyObservableCollection<IShellMenuItem>(new ObservableCollection<IShellMenuItem>(compositionContainer.GetExportedValues<IShellMenuItem<ISdrClientDevice>>()
            .Select(x=>x.Init(sdr))));
        InfoBadge = new InfoBadge
        {
            Value = sdr.Identity.TargetSystemId,
        };
    }
}


[Export(typeof(IShellMenuItem<ISdrClientDevice>))]
[PartCreationPolicy(CreationPolicy.NonShared)]
public class SdrParamsEditorShellMenuItem : ShellMenuItem,IShellMenuItem<ISdrClientDevice>
{
    public SdrParamsEditorShellMenuItem() : base($"asv:shell.menu.sdr.params?{Guid.NewGuid()}")
    {
        Icon = MaterialIconDataProvider.GetData(MaterialIconKind.WrenchCog);
        Position = ShellMenuPosition.Top;
        Type = ShellMenuItemType.PageNavigation;
        Order = 100;
        Name = "Settings";
    }


    public IShellMenuItem Init(ISdrClientDevice target)
    {
        NavigateTo = ParamPageViewModel.GenerateUri(target.FullId, target.Class);
        return this;
    }
}

