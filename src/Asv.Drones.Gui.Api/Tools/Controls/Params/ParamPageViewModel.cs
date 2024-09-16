using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Asv.Cfg;
using Asv.Common;
using Asv.Mavlink;
using DynamicData;
using DynamicData.Binding;
using FluentAvalonia.UI.Controls;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Asv.Drones.Gui.Api;

public class ParamsConfig
{
    public List<ParamItemViewModelConfig> Params { get; set; } = new();
}

public class ParamPageViewModel : ShellPage
{
    private readonly IMavlinkDevicesService _svc;
    private readonly ILogService _log;
    private readonly IConfiguration _cfg;
    private CancellationTokenSource _cancellationTokenSource;
    private readonly ReadOnlyObservableCollection<ParamItemViewModel> _viewedParams;
    private ObservableAsPropertyHelper<bool> _isRefreshing;
    private readonly SourceList<ParamItemViewModel> _viewedParamsList;
    private ParamItemViewModel _selectedItem;
    private readonly Subject<bool> _canClearSearchText = new();
    private readonly ParamsConfig _config;
    private IParamsClientEx _paramsIfc;

    #region Uri

    public static Uri GenerateUri(string baseUri, ushort deviceFullId, DeviceClass @class) =>
        new($"{baseUri}?id={deviceFullId}&class={@class:G}");

    #endregion

    public ParamPageViewModel() : base(WellKnownUri.UndefinedUri)
    {
        DesignTime.ThrowIfNotDesignMode();
        DeviceName = "Params";
    }

    protected ParamPageViewModel(string id, IMavlinkDevicesService svc, ILogService log, IConfiguration cfg) : base(id)
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
        _cancellationTokenSource = new CancellationTokenSource().DisposeItWith(Disposable);
        _viewedParamsList.Connect()
            .Bind(out _viewedParams)
            .Subscribe()
            .DisposeItWith(Disposable);
        this.WhenValueChanged(_ => _.SearchText)
            .Subscribe(_ => { _canClearSearchText.OnNext(!string.IsNullOrWhiteSpace(_)); })
            .DisposeItWith(Disposable);

        Clear = ReactiveCommand.Create(() => { SearchText = string.Empty; }, _canClearSearchText)
            .DisposeItWith(Disposable);

        Disposable.AddAction(() =>
        {
            _config.Params = _config.Params.Where(_ => _.IsStarred).ToList();
            _cfg.Set(_config);
        });
    }

    public override void SetArgs(NameValueCollection args)
    {
        base.SetArgs(args);
        if (ushort.TryParse(args["id"], out var id) == false) return;
        if (Enum.TryParse<DeviceClass>(args["class"], true, out var deviceClass) == false) return;
        var ifc = GetParamsClient(_svc, id, deviceClass);
        if (ifc == null) return;

        Icon = MavlinkHelper.GetIcon(deviceClass);

        switch (deviceClass)
        {
            case DeviceClass.Plane:
                var plane = _svc.GetVehicleByFullId(id);
                if (plane == null) break;
                DeviceName = plane.Name.Value;
                break;
            case DeviceClass.Copter:
                var copter = _svc.GetVehicleByFullId(id);
                if (copter == null) break;
                DeviceName = copter.Name.Value;
                break;
            case DeviceClass.GbsRtk:
                var gbs = _svc.GetGbsByFullId(id);
                if (gbs == null) break;
                DeviceName = gbs.Name.Value;
                break;
            case DeviceClass.SdrPayload:
                var sdr = _svc.GetPayloadsByFullId(id);
                if (sdr == null) break;
                DeviceName = sdr.Name.Value;
                break;
            case DeviceClass.Adsb:
                var adsb = _svc.GetAdsbVehicleByFullId(id);
                if (adsb == null) break;
                DeviceName = adsb.Name.Value;
                break;
        }

        InternalInit(ifc);
    }

    public virtual IParamsClientEx? GetParamsClient(IMavlinkDevicesService svc, ushort fullId, DeviceClass @class)
    {
        return null;
    }

    protected void InternalInit(IParamsClientEx paramsIfc)
    {
        @paramsIfc.RemoteCount.Where(_ => _.HasValue).Subscribe(_ => Total = _.Value).DisposeItWith(Disposable);
        var inputPipe = @paramsIfc.Items
            .Transform(_ => new ParamItemViewModel(Id, _, _log))
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
            .AutoRefresh(v => v.IsSynced)
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

                    if (existItem != null) _config.Params.Remove(existItem);

                    _config.Params.Add(new ParamItemViewModelConfig
                    {
                        IsStarred = item.Current.IsStarred,
                        Name = item.Current.Name
                    });
                }
            })
            .DisposeItWith(Disposable);

        FilterPipe.OnNext(_ => true);
        Params = leftItems;

        UpdateParams = ReactiveCommand.CreateFromTask(async cancel =>
        {
            _cancellationTokenSource = new CancellationTokenSource().DisposeItWith(Disposable);
            var viewed = _viewedParamsList.Items.Select(_ => _.GetConfig()).ToArray();
            _viewedParamsList.Clear();
            try
            {
                await paramsIfc.ReadAll(new Progress<double>(_ => Progress = _), _cancellationTokenSource.Token);
            }
            catch (TaskCanceledException)
            {
                _log.Info("User", "Canceled updating params");
            }
            finally
            {
                foreach (var item in viewed)
                {
                    var existItem = allItems.FirstOrDefault(_ => _.Name == item.Name);
                    if (existItem == null) continue;
                    existItem.SetConfig(item);
                    _viewedParamsList.Add(existItem);
                }
            }
        }).DisposeItWith(Disposable);
        UpdateParams.IsExecuting.ToProperty(this, _ => _.IsRefreshing, out _isRefreshing).DisposeItWith(Disposable);
        UpdateParams.ThrownExceptions.Subscribe(OnRefreshError).DisposeItWith(Disposable);
        StopUpdateParams = ReactiveCommand.Create(() => { _cancellationTokenSource.Cancel(); });
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
                Title = RS.ParamPageViewModel_DataLossDialog_Title,
                Content = RS.ParamPageViewModel_DataLossDialog_Content,
                IsSecondaryButtonEnabled = true,
                PrimaryButtonText = RS.ParamPageViewModel_DataLossDialog_PrimaryButtonText,
                SecondaryButtonText = RS.ParamPageViewModel_DataLossDialog_SecondaryButtonText,
                CloseButtonText = RS.ParamPageViewModel_DataLossDialog_CloseButtonText
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

    private void OnRefreshError(Exception? ex)
    {
        _log.Error("Params view", "Error to read all params items", ex);
    }

    public bool IsRefreshing => _isRefreshing.Value;
    [Reactive] public bool ShowStaredOnly { get; set; }
    [Reactive] public double Progress { get; set; }
    [Reactive] public ReactiveCommand<Unit, Unit> Clear { get; set; }
    [Reactive] public ReactiveCommand<Unit, Unit> UpdateParams { get; set; }
    [Reactive] public ReactiveCommand<Unit, Unit> StopUpdateParams { get; set; }
    [Reactive] public ReactiveCommand<Unit, Unit> RemoveAllPins { get; set; }
    public Subject<Func<ParamItemViewModel, bool>> FilterPipe { get; } = new();
    [Reactive] public ReadOnlyObservableCollection<ParamItemViewModel> Params { get; set; }
    public ReadOnlyObservableCollection<ParamItemViewModel> ViewedParams => _viewedParams;
    [Reactive] public string DeviceName { get; set; }
    [Reactive] public string SearchText { get; set; }
    [Reactive] public int Total { get; set; }
    [Reactive] public int Loaded { get; set; }

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