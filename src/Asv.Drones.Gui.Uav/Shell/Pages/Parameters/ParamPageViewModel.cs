using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Web;
using Asv.Cfg;
using Asv.Common;
using Asv.Drones.Gui.Core;
using Asv.Mavlink;
using DynamicData;
using DynamicData.Binding;
using FluentAvalonia.UI.Controls;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Asv.Drones.Gui.Uav;

public class ParamsConfig
{
    public List<ParamItemViewModelConfig> Params { get; set; } = new();
}

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
    private Subject<bool> _canClearSearchText = new ();
    private ParamsConfig _config;

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

        _config = _cfg.Get<ParamsConfig>();

        FilterPipe.DisposeItWith(Disposable);
        this.WhenAnyValue(_ => _.SearchText, _ => _.ShowStaredOnly)
            .Throttle(TimeSpan.FromMilliseconds(100), RxApp.MainThreadScheduler)
            .Subscribe(_ => FilterPipe.OnNext(item => item.Filter(SearchText, ShowStaredOnly)))
            .DisposeItWith(Disposable);
         _viewedParamsList = new SourceList<ParamItemViewModel>().DisposeItWith(Disposable);
         _viewedParamsList.Connect()
             .Bind(out _viewedParams)
             .Subscribe()
             .DisposeItWith(Disposable);

         this.WhenValueChanged(_ => _.SearchText)
             .Subscribe(_ =>
             {
                 _canClearSearchText.OnNext(!string.IsNullOrWhiteSpace(_));
             })
             .DisposeItWith(Disposable);
         
         Clear = ReactiveCommand.Create(() =>
         {
             SearchText = string.Empty;
         }, _canClearSearchText).DisposeItWith(Disposable);
         
         Disposable.AddAction(() =>
         {
             _config.Params = _config.Params.Where(_ => _.IsStarred).ToList();
             _cfg.Set(_config);
         });
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
                var gbs = _svc.GetGbsByFullId(id);
                if (gbs == null) return;
                DeviceName = gbs.Name.Value;
                Init(gbs.Params);
                break;
            case DeviceClass.SdrPayload:
                var sdr = _svc.GetPayloadsByFullId(id);
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
        @paramsIfc.RemoteCount.Where(_ => _.HasValue).Subscribe(_ => Total = _.Value).DisposeItWith(Disposable);
        
        var inputPipe = @paramsIfc.Items
            .Transform(_ => new ParamItemViewModel(_,_log))
            .DisposeMany()
            .RefCount();
        inputPipe
            .Bind(out var allItems)
            .Subscribe(_ =>
            {
                foreach (var item in _config.Params)
                {
                    var existItem = _.FirstOrDefault(_ => _.Current.Name == item.Name);
                    if (existItem == null) continue;
                    existItem.Current?.SetConfig(item);
                }
            })
            .DisposeItWith(Disposable);
        inputPipe
            .Filter(FilterPipe)
            .SortBy(_ => _.Name)
            .Bind(out var leftItems)
            .Subscribe()
            .DisposeItWith(Disposable);
        
        inputPipe
            .AutoRefresh(_ => _.IsStarred)
            .Filter(_ => _.IsStarred)
            .Subscribe(_ =>
            {
                foreach (var item in _)
                {
                    var existItem = _config.Params.FirstOrDefault(__ => __.Name == item.Current.Name);
                    
                    if(existItem != null)
                        _config.Params.Remove(existItem);
                    
                    _config.Params.Add(new ParamItemViewModelConfig
                    {
                        IsStarred = item.Current.IsStarred,
                        Name = item.Current.Name
                    });
                }
            })
            .DisposeItWith(Disposable);
        
        FilterPipe.OnNext(_=>true);
        Params = leftItems;

        UpdateParams = ReactiveCommand.CreateFromTask(async cancel =>
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
        
        RemoveAllPins = ReactiveCommand.Create(() =>
        {
            _viewedParamsList.Edit(_ =>
            {
                foreach (var item in _)
                {
                    item.IsPinned = false;
                }
            });
        }).DisposeItWith(Disposable);
    }

    public override async Task<bool> TryClose()
    {
        var notSyncedParams = _viewedParamsList.Items.Where(_ => !_.IsSynced).ToArray();
        
        if (notSyncedParams.Any())
        {
            var dialog = new ContentDialog()
            {
                Content = "There are unsaved changes, do you want to save them?",
                IsSecondaryButtonEnabled = true,
                PrimaryButtonText = "Save",
                SecondaryButtonText = "Don't save",
                CloseButtonText = "Close"
            };
            
            var result = await dialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                foreach (var param in notSyncedParams)
                {
                    param.WriteParamData();
                    param.IsSynced = true;
                }

                return true;
            }

            if (result == ContentDialogResult.Secondary) return true;

            if (result == ContentDialogResult.None) return false;
        }
        
        return true;
    }

    private void OnRefreshError(Exception ex)
    {
        _log.Error("Params view","Error to read all params items",ex);
    }
    
    public bool IsRefreshing => _isRefreshing.Value;
    [Reactive]
    public bool ShowStaredOnly { get; set; }
    [Reactive]
    public double Progress { get; set; }
    
    [Reactive]
    public ReactiveCommand<Unit,Unit> Clear { get; set; }
    [Reactive]
    public ReactiveCommand<Unit,Unit> UpdateParams { get; set; }
    [Reactive]
    public ReactiveCommand<Unit,Unit> RemoveAllPins { get; set; }
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
}