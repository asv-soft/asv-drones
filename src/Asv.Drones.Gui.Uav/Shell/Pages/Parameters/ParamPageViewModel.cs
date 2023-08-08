using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Web;
using System.Windows.Input;
using Asv.Cfg;
using Asv.Common;
using Asv.Drones.Gui.Core;
using Asv.Mavlink;
using DynamicData;
using JetBrains.Annotations;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Asv.Drones.Gui.Uav;

[ExportShellPage(UriString)]
[PartCreationPolicy(CreationPolicy.NonShared)]
public class ParamPageViewModel: ShellPage
{
    private readonly IMavlinkDevicesService _svc;
    private readonly ILogService _log;
    
    private readonly IConfiguration _cfg;
    private ReadOnlyObservableCollection<ParamItemViewModel> _viewedParams;
    private ObservableAsPropertyHelper<bool> _isRefreshing;
    private readonly SourceList<ParamItemViewModel> _viewedParamsList;
    private ParamItemViewModel _selectedItem;

    #region Uri

    public const string UriString = "asv:shell.page.params";
    public static Uri GenerateUri(ushort id, DeviceClass @class) => new($"{UriString}?id={id}&class={@class:G}");

    #endregion

    public ParamPageViewModel():base(UriString)
    {
        DeviceName = "Params";
    }
    
    [ImportingConstructor]
    public ParamPageViewModel(IMavlinkDevicesService svc, ILogService log, IConfiguration cfg) : base(UriString)
    {
        _svc = svc ?? throw new ArgumentNullException(nameof(svc));
        _log = log ?? throw new ArgumentNullException(nameof(log));
        _cfg = cfg ?? throw new ArgumentNullException(nameof(cfg));
        FilterPipe.DisposeItWith(Disposable);
        this.WhenAnyValue(_=>_.SearchText)
            .Throttle(TimeSpan.FromMilliseconds(100), RxApp.MainThreadScheduler)
            .Subscribe(_ =>FilterPipe.OnNext(item=>item.Filter(SearchText,ShowStaredOnly)))
            .DisposeItWith(Disposable);
         _viewedParamsList = new SourceList<ParamItemViewModel>().DisposeItWith(Disposable);
         _viewedParamsList.Connect()
             .Bind(out _viewedParams)
             .Subscribe()
             .DisposeItWith(Disposable);
    }

    public override void SetArgs(Uri link)
    {
        
        var query =  HttpUtility.ParseQueryString(link.Query);
        if (ushort.TryParse(query["id"], out var id) == false) return;
        if (Enum.TryParse<DeviceClass>(query["class"], true, out var deviceClass) == false) return;
        
        switch (deviceClass)
        {
            case DeviceClass.Unknown:
                break;
            case DeviceClass.Plane:
                var plane = _svc.GetVehicleByFullId(id);
                if (plane == null) return;
                DeviceName = plane.Name.Value;
                Init(plane.Params);
                break;
            case DeviceClass.Copter:
                var copter = _svc.GetVehicleByFullId(id);
                if (copter == null) return;
                DeviceName = copter.Name.Value;
                Init(copter.Params);
                break;
            case DeviceClass.GbsRtk:
                var gbs = _svc.GetVehicleByFullId(id);
                if (gbs == null) return;
                DeviceName = gbs.Name.Value;
                Init(gbs.Params);
                break;
            case DeviceClass.SdrPayload:
                var sdr = _svc.GetVehicleByFullId(id);
                if (sdr == null) return;
                DeviceName = sdr.Name.Value;
                Init(sdr.Params);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
        Title = $"Params: {DeviceName}";
    }

    private void Init(IParamsClientEx paramsIfc)
    {
        @paramsIfc.RemoteCount.Where(_=>_.HasValue).Subscribe(_=>Total = _.Value).DisposeItWith(Disposable);
        
        var inputPipe = @paramsIfc.Items
            .Transform(_ => new ParamItemViewModel(_,_log))
            .DisposeMany()
            .RefCount();
        inputPipe
            .Bind(out var allItems)
            .Subscribe()
            .DisposeItWith(Disposable);
        inputPipe
            .Filter(FilterPipe)
            .SortBy(_ => _.Name)
            .Bind(out var leftItems)
            .Subscribe()
            .DisposeItWith(Disposable);
        FilterPipe.OnNext(_=>true);
        Params = leftItems;

        UpdateParams = ReactiveCommand.CreateFromTask(async (cancel) =>
        {
            var viewed = _viewedParamsList.Items.Select(_ => _.GetConfig()).ToArray();
            _viewedParamsList.Clear();
            await paramsIfc.ReadAll(new Progress<double>(_ => Progress = _), cancel);
            foreach (var item in viewed)
            {
                var existItem = allItems.FirstOrDefault(_ => _.Name == item.Name);
                if (existItem == null) continue;
                existItem.SetConfig(item);
                _viewedParamsList.Add(existItem);
            }
        }).DisposeItWith(Disposable);
        UpdateParams.IsExecuting.ToProperty(this, _ => _.IsRefreshing, out _isRefreshing).DisposeItWith(Disposable);
        UpdateParams.ThrownExceptions.Subscribe(OnRefreshError).DisposeItWith(Disposable);
    }

    private void OnRefreshError(Exception ex)
    {
        _log.Error("Params view","Error to read all params items",ex);
    }
    
    public bool IsRefreshing => _isRefreshing.Value;
    [Reactive]
    public double Progress { get; set; }
    
    [Reactive]
    public ICommand Clear { get; set; }
    [Reactive]
    public ReactiveCommand<Unit,Unit> UpdateParams { get; set; }
    [Reactive]
    public ICommand RemoveAllPins { get; set; }
    public Subject<Func<ParamItemViewModel, bool>> FilterPipe { get; } = new();

    [Reactive]
    public ReadOnlyObservableCollection<ParamItemViewModel> Params { get; set; }

    public ReadOnlyObservableCollection<ParamItemViewModel> ViewedParams => _viewedParams;
    [Reactive]
    public string DeviceName { get; set; }
    [Reactive]
    public string SearchText { get; set; }
    [Reactive]
    public int Total { get; set; }

    public ParamItemViewModel SelectedItem
    {
        get => _selectedItem;
        set
        {
            var itemsToDelete = _viewedParamsList.Items.Where(_ => _.IsPinned == false).ToArray();
            _viewedParamsList.RemoveMany(itemsToDelete);
            this.RaiseAndSetIfChanged(ref _selectedItem, value);
            if (value != null)
            {
                if (_viewedParamsList.Items.Contains(value) == false)
                {
                    _viewedParamsList.Add(value);    
                }
                    
            }
            
        }
    }

    
    [Reactive]
    public bool ShowStaredOnly { get; set; }
    
}