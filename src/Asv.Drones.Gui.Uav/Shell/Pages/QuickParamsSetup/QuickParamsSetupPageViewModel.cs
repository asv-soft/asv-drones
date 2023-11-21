using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Web;
using System.Windows.Input;
using Asv.Cfg;
using Asv.Common;
using Asv.Drones.Gui.Core;
using Asv.Mavlink;
using DynamicData;
using DynamicData.Binding;
using JetBrains.Annotations;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Asv.Drones.Gui.Uav;

[ExportShellPage(UriString)]
[PartCreationPolicy(CreationPolicy.NonShared)]
public class QuickParamsSetupPageViewModel : ShellPage
{
    private readonly IMavlinkDevicesService _svc;
    private readonly ILogService _log;
    private readonly IConfiguration _cfg;
    
    private readonly ReadOnlyObservableCollection<IQuickParamsPart> _items;
    private readonly IEnumerable<IQuickParamsPartProvider> _quickParams;
    private readonly SourceList<IQuickParamsPart> _itemsList;
    private readonly ObservableAsPropertyHelper<bool> _isNotAllSynced;
    
    #region Uri
    public const string UriString = "asv:shell.page.quickparamssetup";
    public static Uri GenerateUri(ushort id, DeviceClass @class) => new($"{UriString}?id={id}&class={@class:G}");
    public static bool ParseUri(Uri uri, out ushort id, out DeviceClass deviceClass)
    {
        var query =  HttpUtility.ParseQueryString(uri.Query);
        if (ushort.TryParse(query["id"], out id)) return Enum.TryParse(query["class"], true, out deviceClass);
        deviceClass = DeviceClass.Unknown;
        return false;
    }
    #endregion
    
    public QuickParamsSetupPageViewModel() : base(new Uri("asv:designTime"))
    {
        DeviceName = "Params";
        _items = new ReadOnlyObservableCollection<IQuickParamsPart>(new ObservableCollection<IQuickParamsPart>(
            new IQuickParamsPart[]
            {
                
            }));
    }
    
    [ImportingConstructor]
    public QuickParamsSetupPageViewModel(IMavlinkDevicesService svc, ILogService log, 
        IConfiguration cfg, [ImportMany] IEnumerable<IQuickParamsPartProvider> quickParams) 
        : base(new Uri(UriString))
    {
        _svc = svc ?? throw new ArgumentNullException(nameof(svc));
        _log = log ?? throw new ArgumentNullException(nameof(log));
        _cfg = cfg ?? throw new ArgumentNullException(nameof(cfg));

        _quickParams = quickParams;
        
        _itemsList = new SourceList<IQuickParamsPart>().DisposeItWith(Disposable);
        _itemsList.Connect()
            .DisposeMany()
            .Bind(out _items)
            .Subscribe()
            .DisposeItWith(Disposable);
        
        _itemsList.Connect()
            .DisposeMany()
            .AutoRefresh(_ => _.IsSynced) // update collection when any part require reboot
            .ToCollection()
            .Select(parts => parts.Any(part => !part.IsSynced)) // check if any part require reboot
            .ToProperty(this, _ => _.IsNotAllSynced, out _isNotAllSynced)
            .DisposeItWith(Disposable);

        WriteAll = ReactiveCommand.CreateFromTask(WriteAllImpl, 
            this.WhenValueChanged(_ => _.IsNotAllSynced, false));
        
        ReadAll = ReactiveCommand.CreateFromTask(ReadAllImpl);
    }

    private async Task WriteAllImpl(CancellationToken token)
    {
        foreach (var item in _itemsList.Items)
        {
            if (token.IsCancellationRequested) break;
            await item.Write();
            await item.Read();
            item.RaisePropertyChanged();
        }
    }

    private async Task ReadAllImpl(CancellationToken token)
    {
        foreach (var item in _itemsList.Items)
        {
            if (token.IsCancellationRequested) break;
            await item.Read();
        }
    }    
    
    [Reactive]
    public ICommand WriteAll { get; set; }
    
    [Reactive]
    public ICommand ReadAll { get; set; }

    public bool IsNotAllSynced => _isNotAllSynced.Value;
    
    [Reactive]
    public bool IsAdvancedMode { get; set; }
    
    [Reactive]
    public string DeviceName { get; set; }
    
    public ReadOnlyObservableCollection<IQuickParamsPart> Items => _items;

    public override void SetArgs(Uri link)
    {
        if (!ParseUri(link, out var id, out var deviceClass)) return;
        switch (deviceClass)
        {
            case DeviceClass.Unknown:
                break;
            case DeviceClass.Plane:
                var plane = _svc.GetVehicleByFullId(id);
                if (plane == null) return;
                DeviceName = plane.Name.Value;
                Init(plane);
                break;
            case DeviceClass.Copter:
                var copter = _svc.GetVehicleByFullId(id);
                if (copter == null) return;
                DeviceName = copter.Name.Value;
                Init(copter);
                break;
            case DeviceClass.GbsRtk:
                var gbs = _svc.GetVehicleByFullId(id);
                if (gbs == null) return;
                DeviceName = gbs.Name.Value;
                Init(gbs);
                break;
            case DeviceClass.SdrPayload:
                var sdr = _svc.GetVehicleByFullId(id);
                if (sdr == null) return;
                DeviceName = sdr.Name.Value;
                Init(sdr);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
        Title = $"Quick params {DeviceName}";
    }
    
    private async void Init(IVehicleClient vehicleIfc)
    {
        _itemsList.Clear();
        
        _itemsList.AddRange(_quickParams
            .SelectMany(_ => _.Create(vehicleIfc))
            .OrderBy(_ => _.Order));
        
        foreach (var item in _itemsList.Items)
        {
            await item.Read();
        }
        
        this.WhenValueChanged(_ => _.IsAdvancedMode)
            .Subscribe(_ =>
            {
                _itemsList.Edit(_ =>
                {
                    foreach (var item in _)
                    {
                        if (item.IsVisibleInAdvancedMode)
                        {
                            item.IsVisible = IsAdvancedMode;
                        }
                    }
                });
                
            })
            .DisposeItWith(Disposable);
    }
    
    private void OnRefreshError(Exception ex)
    {
        _log.Error("Quick params setup","Error to setup params items",ex);
    }
}