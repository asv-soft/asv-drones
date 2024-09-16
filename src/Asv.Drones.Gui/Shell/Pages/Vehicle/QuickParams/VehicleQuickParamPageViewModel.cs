using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Composition;
using Asv.Common;
using Asv.Drones.Gui.Api;
using Asv.Mavlink;
using Material.Icons;

namespace Asv.Drones.Gui;

[ExportShellPage(WellKnownUri.ShellPageQuickParams)]
public class VehicleQuickParamPageViewModel : ShellPage, ITreePageContext
{
    private readonly ILogService _log;
    private readonly IMavlinkDevicesService _svc;
    private readonly IEnumerable<IViewModelProvider<ITreePageMenuItem>> _arduCopterItems;
    private readonly IEnumerable<IViewModelProvider<ITreePageMenuItem>> _arduPlaneItems;

    public VehicleQuickParamPageViewModel() : base(WellKnownUri.UndefinedUri)
    {
        DesignTime.ThrowIfNotDesignMode();
        QuickParams = new TreePageExplorerViewModel();
    }
    
    [ImportingConstructor]
    public VehicleQuickParamPageViewModel(
        [ImportMany(WellKnownUri.ShellPageQuickParamsArduCopterVehicle)] IEnumerable<IViewModelProvider<ITreePageMenuItem>> arduCopterItems, 
        [ImportMany(WellKnownUri.ShellPageQuickParamsArduPlaneVehicle)] IEnumerable<IViewModelProvider<ITreePageMenuItem>> arduPlaneItems, 
        IMavlinkDevicesService svc, ILogService log) 
        : base(WellKnownUri.ShellPageQuickParams)
    {
        _svc = svc;
        _log = log;
        _arduCopterItems = arduCopterItems;
        _arduPlaneItems = arduPlaneItems;
    }
    
    public TreePageExplorerViewModel QuickParams { get; private set; }

    public IParamsClientEx? ParamsClientEx { get; set; }
    
    #region Uri

    public static Uri GenerateUri(string baseUri, ushort deviceFullId, DeviceClass @class) =>
        new($"{baseUri}?id={deviceFullId}&class={@class:G}");

    #endregion
    
    public override void SetArgs(NameValueCollection args)
    {
        base.SetArgs(args);
        
        if (ushort.TryParse(args["id"], out var id) == false) return;
        if (Enum.TryParse<DeviceClass>(args["class"], true, out var deviceClass) == false) return;
        
        ParamsClientEx = GetParamsClient(_svc, id, deviceClass);
        if (ParamsClientEx == null) return;
        
        Icon = Api.MavlinkHelper.GetIcon(deviceClass);

        var vehicle = _svc.GetVehicleByFullId(id);
        if (vehicle == null) return;

        switch (vehicle)
        {
            case ArduCopterClient:
                QuickParams = new TreePageExplorerViewModel(_arduCopterItems, this, _log).DisposeItWith(Disposable);
                break;
            case ArduPlaneClient:
                QuickParams = new TreePageExplorerViewModel(_arduPlaneItems, this, _log).DisposeItWith(Disposable);
                break;
        }
        
        Title = $"{vehicle.Class}: {vehicle.Name.Value}";
        
        QuickParams.Title = RS.VehicleQuickParamPageViewModel_Title;
        QuickParams.Icon = MaterialIconKind.WrenchClock;
    }

    private IParamsClientEx? GetParamsClient(IMavlinkDevicesService svc, ushort fullId, DeviceClass @class)
    {
        var dev = svc.GetVehicleByFullId(fullId);
        if (dev == null) return null;
        dev.Name.Subscribe(n => Title = n).DisposeItWith(Disposable);
        return dev.Params;
    }
}