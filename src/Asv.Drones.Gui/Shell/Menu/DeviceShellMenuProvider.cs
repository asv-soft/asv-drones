using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Composition;
using System.Linq;
using Asv.Common;
using Asv.Drones.Gui.Api;
using Asv.Mavlink;
using DynamicData;
using FluentAvalonia.UI.Controls;
using Material.Icons;
using MavlinkHelper = Asv.Drones.Gui.Api.MavlinkHelper;

namespace Asv.Drones.Gui;

[Export(typeof(IViewModelProvider<IShellMenuItem>))]
public class DeviceShellMenuProvider : ViewModelProviderBase<IShellMenuItem>
{
    private readonly IMavlinkDevicesService _svc;
    private readonly IApplicationHost _host;

    [ImportingConstructor]
    public DeviceShellMenuProvider(IMavlinkDevicesService svc, IApplicationHost host)
    {
        _svc = svc;
        _host = host;
        svc.AllDevices.Transform(CreateGroup)
            .Filter(x => x != null)
            .ChangeKey((_, v) => v.Id)
            .DisposeMany()
            .PopulateInto(Source)
            .DisposeItWith(Disposable);
    }

    private IShellMenuItem? CreateGroup(IClientDevice device)
    {
        var items = _host.Container.GetExports<IShellMenuItem<IClientDevice>>(device.Class.ToString("G")).ToArray();
        // we don't create group if there is no sub menu
        if (items.Length == 0) return null;
        foreach (var item in items)
        {
            item.Init(device);
        }

        return new DeviceBasedGroupMenu(device, items);
    }
}

public class DeviceBasedGroupMenu : ShellMenuItem
{
    public DeviceBasedGroupMenu(IClientDevice device, IEnumerable<IShellMenuItem<IClientDevice>> subItems) : base(
        $"{WellKnownUri.ShellMenu}.device.{device.FullId}")
    {
        device.Name.Subscribe(x => Name = x).DisposeItWith(Disposable);

        Icon = MaterialIconDataProvider.GetData(MavlinkHelper.GetIcon(device.Class));
        Position = ShellMenuPosition.Top;
        Type = ShellMenuItemType.Group;
        Order = ushort.MaxValue + device.FullId;
        InfoBadge = new InfoBadge
        {
            Value = device.Identity.TargetSystemId,
        };
        Items = new ReadOnlyObservableCollection<IShellMenuItem>(
            new ObservableCollection<IShellMenuItem>(subItems.OrderBy(x => x.Order)));
    }
}