using System;
using System.Composition;
using System.Reactive.Linq;
using Asv.Common;
using Asv.Drones.Gui.Api;
using Material.Icons;

namespace Asv.Drones.Gui;

[Export(WellKnownUri.ShellPageSettings, typeof(ITreePageMenuItem))]
public class DeviceBrowserTreeMenuItem : TreePageMenuItem
{
    private readonly IMavlinkDevicesService _svc;

    [method: ImportingConstructor]
    public DeviceBrowserTreeMenuItem(IMavlinkDevicesService svc) : base(WellKnownUri
        .ShellPageSettingsConnectionsDevicesUri)
    {
        _svc = svc;
        _svc.Devices.Count().Subscribe(cnt => { Status = cnt <= 0 ? null : cnt.ToString(); })
            .DisposeItWith(Disposable);
    }

    public override Uri ParentId => WellKnownUri.ShellPageSettingsConnectionsUri;
    public override string? Name => RS.DeviceBrowserView_Header;
    public override string? Description => RS.DeviceBrowserTreeMenuItem_Description;
    public override MaterialIconKind Icon => MaterialIconKind.FormatListText;
    public override int Order => -1;

    public override ITreePage? CreatePage(ITreePageContext context)
    {
        return new DeviceBrowserViewModel(_svc);
    }
}