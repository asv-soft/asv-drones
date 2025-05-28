using System;
using System.Collections.Generic;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Asv.Avalonia;
using Asv.Avalonia.IO;
using Asv.Cfg;
using Asv.Common;
using Asv.Drones.Api;
using Asv.IO;
using Asv.Mavlink;
using Material.Icons;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using ObservableCollections;
using R3;

namespace Asv.Drones;

public class ParamsConfig
{
    public List<ParamItemViewModelConfig> Params { get; set; } = [];
}

[ExportPage(PageId)]
public class MavParamsPageViewModel
    : DevicePageViewModel<IMavParamsPageViewModel>,
        IMavParamsPageViewModel
{
    public const string PageId = "mav-params";
    public const MaterialIconKind PageIcon = MaterialIconKind.CogTransferOutline;

    private DeviceId _deviceId;
    private IParamsClientEx? _paramsClient;
    private CancellationTokenSource _cancellationTokenSource;
    private ISynchronizedView<KeyValuePair<string, ParamItem>, ParamItemViewModel> _view;
    private readonly ILoggerFactory _loggerFactory;
    private readonly ILogger _log;
    private readonly INavigationService _nav;
    private readonly IConfiguration _cfg;
    private readonly ObservableList<ParamItemViewModel> _viewedParamsList; // TODO: Separate viewModels for this collection and all params
    private readonly Subject<bool> _canClearSearchText = new();
    private readonly ReactiveProperty<bool> _showStarredOnly;
    private readonly ReactiveProperty<string?> _searchText;
    private readonly ParamsConfig _config;

    public MavParamsPageViewModel()
        : this(
            NullDeviceManager.Instance,
            NullCommandService.Instance,
            NullLoggerFactory.Instance,
            new InMemoryConfiguration(),
            NullNavigationService.Instance
        )
    {
        DesignTime.ThrowIfNotDesignMode();

        var list = new ObservableList<ParamItemViewModel>
        {
            new() { DisplayName = "Param 1" },
            new() { DisplayName = "Param 2" },
            new() { DisplayName = "Param 3" },
        };
        var viewedList = new ObservableList<ParamItemViewModel> { list[0] };

        AllParams = list.ToNotifyCollectionChangedSlim();
        ViewedParams = viewedList.ToNotifyCollectionChangedSlim();
    }

    [ImportingConstructor]
    public MavParamsPageViewModel(
        IDeviceManager devices,
        ICommandService cmd,
        ILoggerFactory loggerFactory,
        IConfiguration cfg,
        INavigationService nav
    )
        : base(PageId, devices, cmd)
    {
        ArgumentNullException.ThrowIfNull(devices);
        ArgumentNullException.ThrowIfNull(cmd);
        ArgumentNullException.ThrowIfNull(cfg);
        ArgumentNullException.ThrowIfNull(loggerFactory);
        ArgumentNullException.ThrowIfNull(nav);

        Title = "Params";

        _loggerFactory = loggerFactory;
        _log = loggerFactory.CreateLogger<MavParamsPageViewModel>();
        _cfg = cfg;
        _config = _cfg.Get<ParamsConfig>();
        _nav = nav;
        _searchText = new ReactiveProperty<string?>();
        _showStarredOnly = new ReactiveProperty<bool>();
        _viewedParamsList = [];
        ViewedParams = _viewedParamsList.ToNotifyCollectionChangedSlim();

        _cancellationTokenSource = new CancellationTokenSource();

        SearchText = new HistoricalStringProperty($"{PageId}{nameof(SearchText)}", _searchText)
        {
            Parent = this,
        };
        ShowStaredOnly = new HistoricalBoolProperty($"{PageId}.{ShowStaredOnly}", _showStarredOnly)
        {
            Parent = this,
        };
        IsRefreshing = new BindableReactiveProperty<bool>();
        SelectedItem = new BindableReactiveProperty<ParamItemViewModel?>();
        Progress = new BindableReactiveProperty<double>();

        _sub1 = SelectedItem.Subscribe(value =>
        {
            var itemsToDelete = _viewedParamsList.Where(_ => !_.IsPinned.ViewValue.Value).ToArray();

            foreach (var item in itemsToDelete)
            {
                _viewedParamsList.Remove(item);
            }

            if (value is null)
            {
                return;
            }

            if (!_viewedParamsList.Contains(value))
            {
                _viewedParamsList.Add(value);
            }
        });

        _sub2 = SearchText.ViewValue.Subscribe(txt =>
            _canClearSearchText.OnNext(!string.IsNullOrWhiteSpace(txt))
        );

        Clear = _canClearSearchText.ToReactiveCommand(_ => _searchText.Value = string.Empty);
    }

    private void InternalInit()
    {
        if (_paramsClient is null)
        {
            throw new Exception($"Service of type {nameof(IParamsClientEx)} was not found");
        }

        Total = _paramsClient.RemoteCount.ToReadOnlyBindableReactiveProperty();
        _view = _paramsClient.Items.CreateView(kvp => new ParamItemViewModel(
            kvp.Key,
            kvp.Value,
            _loggerFactory,
            _config
        ));
        _sub7 = _view.DisposeMany();
        _sub8 = _view.SetRoutableParentForView(this);

        _sub9 = SearchText
            .ViewValue.ThrottleLast(TimeSpan.FromMilliseconds(500))
            .Subscribe(x =>
            {
                if (string.IsNullOrWhiteSpace(x) && !ShowStaredOnly.ViewValue.Value)
                {
                    _view.ResetFilter();
                    return;
                }

                _view.AttachFilter(
                    new SynchronizedViewFilter<KeyValuePair<string, ParamItem>, ParamItemViewModel>(
                        (_, model) =>
                            model.Filter(x ?? string.Empty, ShowStaredOnly.ViewValue.Value)
                    )
                );
            });

        _sub10 = ShowStaredOnly
            .ViewValue.ThrottleLast(TimeSpan.FromMilliseconds(500))
            .Subscribe(x =>
            {
                if (!x && string.IsNullOrWhiteSpace(SearchText.ViewValue.Value))
                {
                    _view.ResetFilter();
                    return;
                }

                _view.AttachFilter(
                    new SynchronizedViewFilter<KeyValuePair<string, ParamItem>, ParamItemViewModel>(
                        (_, model) => model.Filter(SearchText.ViewValue.Value ?? string.Empty, x)
                    )
                );
            });

        _sub11 = _view
            .ObserveChanged()
            .Subscribe(e =>
            {
                if (e.NewItem.View is null) // it can be null
                {
                    return;
                }

                foreach (var item in _config.Params)
                {
                    if (e.NewItem.View.Name == item.Name)
                    {
                        e.NewItem.View.SetConfig(item);
                    }
                }
            });

        AllParams = _view.ToNotifyCollectionChanged();

        UpdateParams = new BindableAsyncCommand(UpdateParamsCommand.Id, this);

        StopUpdateParams = new BindableAsyncCommand(StopUpdateParamsCommand.Id, this);

        RemoveAllPins = new ReactiveCommand(_ =>
        {
            if (!AllParams.Any(item => item.IsPinned.ViewValue.Value))
            {
                return;
            }

            this.ExecuteCommand(RemoveAllPinsCommand.Id)
                .SafeFireAndForget(ex => _log.LogError(ex, "Something went wrong with unpin all"));
        });
    }

    internal void StopUpdateParamsImpl()
    {
        if (!IsRefreshing.Value)
        {
            return;
        }

        _cancellationTokenSource.Cancel();
    }

    internal void UpdateParamsImpl()
    {
        if (_paramsClient is null)
        {
            return;
        }

        SelectedItem.Value = null;
        IsRefreshing.Value = true;
        _cancellationTokenSource = new CancellationTokenSource();
        var viewed = _viewedParamsList.Select(item => item.GetConfig()).ToArray();
        _viewedParamsList.Clear();

        _paramsClient
            .ReadAll(
                new Progress<double>(i => Progress.Value = i),
                cancel: _cancellationTokenSource.Token
            )
            .SafeFireAndForget(ex =>
            {
                if (ex is TaskCanceledException)
                {
                    _log.LogInformation("User canceled updating params");
                    return;
                }

                _log.LogError(ex, "Error to read all param items");
            });

        foreach (var item in viewed)
        {
            var existItem = _view.FirstOrDefault(currentItem =>
                currentItem.Name == item.Name && currentItem.IsPinned.ViewValue.Value
            );

            if (existItem is null)
            {
                continue;
            }

            existItem.SetConfig(item);
            _viewedParamsList.Add(existItem);
        }

        IsRefreshing.Value = false;
        _cancellationTokenSource.Dispose();
    }

    protected override void AfterDeviceInitialized(IClientDevice device)
    {
        Title = $"Params[{device.Id}]";
        _paramsClient = device.GetMicroservice<IParamsClientEx>();
        DeviceName = device
            .Name.Select(x => x ?? RS.MavParamsPageViewModel_DeviceName_Unknown)
            .ToReadOnlyBindableReactiveProperty<string>();
        _deviceId = device.Id;
        Id.ChangeArgs(_deviceId.AsString());
        Icon = DeviceIconMixin.GetIcon(_deviceId) ?? PageIcon;
        InternalInit();
    }

    public HistoricalBoolProperty ShowStaredOnly { get; }
    public BindableReactiveProperty<bool> IsRefreshing { get; }
    public BindableReactiveProperty<double> Progress { get; }
    public ReactiveCommand Clear { get; }
    public ICommand UpdateParams { get; private set; }
    public ICommand StopUpdateParams { get; private set; }
    public ReactiveCommand RemoveAllPins { get; private set; }

    public INotifyCollectionChangedSynchronizedViewList<ParamItemViewModel> AllParams
    {
        get;
        private set;
    }

    public INotifyCollectionChangedSynchronizedViewList<ParamItemViewModel> ViewedParams { get; }

    public IReadOnlyBindableReactiveProperty<string> DeviceName { get; private set; }
    public HistoricalStringProperty SearchText { get; }
    public IReadOnlyBindableReactiveProperty<int> Total { get; private set; }
    public BindableReactiveProperty<ParamItemViewModel?> SelectedItem { get; }

    protected override async ValueTask InternalCatchEvent(AsyncRoutedEvent e)
    {
        if (e is PageCloseAttemptEvent)
        {
            var isCloseReady = await TryCloseWithApproval();
            e.IsHandled = !isCloseReady;
        }

        await base.InternalCatchEvent(e);
    }

    private async Task<bool> TryCloseWithApproval(CancellationToken cancel = default)
    {
        var notSyncedParams = _viewedParamsList.Where(param => !param.IsSynced.Value).ToArray();

        if (notSyncedParams.Length == 0)
        {
            return true;
        }

        using var vm = new TryCloseWithApprovalDialogViewModel();
        var dialog = new ContentDialog(vm, _nav)
        {
            Title = RS.ParamPageViewModel_DataLossDialog_Title,
            IsSecondaryButtonEnabled = true,
            PrimaryButtonText = RS.ParamPageViewModel_DataLossDialog_PrimaryButtonText,
            SecondaryButtonText = RS.ParamPageViewModel_DataLossDialog_SecondaryButtonText,
            CloseButtonText = RS.ParamPageViewModel_DataLossDialog_CloseButtonText,
        };

        var result = await dialog.ShowAsync();

        if (result == ContentDialogResult.Primary)
        {
            foreach (var param in notSyncedParams)
            {
                await param.WriteParamData(cancel);
                param.IsSynced.Value = true;
            }

            return true;
        }

        if (result == ContentDialogResult.Secondary)
        {
            return true;
        }

        if (result == ContentDialogResult.None)
        {
            return false;
        }

        return true;
    }

    public override IEnumerable<IRoutable> GetRoutableChildren()
    {
        yield return ShowStaredOnly;
        yield return SearchText;
        foreach (var paramItemViewModel in AllParams)
        {
            yield return paramItemViewModel;
        }
    }

    protected override void AfterLoadExtensions() { }

    public override IExportInfo Source => SystemModule.Instance;

    #region Dispose

    private readonly IDisposable _sub1;
    private readonly IDisposable _sub2;
    private IDisposable _sub7;
    private IDisposable _sub8;
    private IDisposable _sub9;
    private IDisposable _sub10;
    private IDisposable _sub11;

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _config.Params = _config.Params.Where(_ => _.IsStarred).ToList();
            _cfg.Set(_config);

            _sub1.Dispose();
            _sub2.Dispose();
            _sub7.Dispose();
            _sub8.Dispose();
            _sub9.Dispose();
            _sub10.Dispose();
            _sub11.Dispose();
            _searchText.Dispose();
            _showStarredOnly.Dispose();
            _canClearSearchText.Dispose();
            _cancellationTokenSource.Dispose();
            DeviceName.Dispose();
            SearchText.Dispose();
            IsRefreshing.Dispose();
            ShowStaredOnly.Dispose();
            Progress.Dispose();
            Clear.Dispose();
            RemoveAllPins.Dispose();
            AllParams.Dispose();
            ViewedParams.Dispose();
            Total.Dispose();
            SelectedItem.Dispose();
        }

        base.Dispose(disposing);
    }

    #endregion
}

file class ParamsKvpComparer : IComparer<KeyValuePair<string, ParamItem>>
{
    public static ParamsKvpComparer Instance { get; } = new();

    public int Compare(KeyValuePair<string, ParamItem> x, KeyValuePair<string, ParamItem> y)
    {
        return string.CompareOrdinal(x.Value.Name, y.Value.Name);
    }
}
