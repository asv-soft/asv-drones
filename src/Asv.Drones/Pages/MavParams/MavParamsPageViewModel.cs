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
    private CancellationTokenSource? _cancellationTokenSource;
    private ISynchronizedView<KeyValuePair<string, ParamItem>, ParamItemViewModel> _view;

    private readonly ILoggerFactory _loggerFactory;
    private readonly INavigationService _nav;
    private readonly IConfiguration _cfg;
    private readonly ObservableList<ParamItemViewModel> _viewedParamsList; // TODO: Separate views for this collection and all params
    private readonly ReactiveProperty<bool> _showStarredOnly;
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

        AllParams = list.ToNotifyCollectionChangedSlim().DisposeItWith(Disposable);
        ViewedParams = viewedList.ToNotifyCollectionChangedSlim().DisposeItWith(Disposable);

        IsDeviceInitialized = false;
        TimeProvider
            .System.CreateTimer(
                _ => IsDeviceInitialized = true,
                null,
                TimeSpan.FromSeconds(5),
                Timeout.InfiniteTimeSpan
            )
            .DisposeItWith(Disposable);
    }

    [ImportingConstructor]
    public MavParamsPageViewModel(
        IDeviceManager devices,
        ICommandService cmd,
        ILoggerFactory loggerFactory,
        IConfiguration cfg,
        INavigationService nav
    )
        : base(PageId, devices, cmd, loggerFactory)
    {
        ArgumentNullException.ThrowIfNull(devices);
        ArgumentNullException.ThrowIfNull(cmd);
        ArgumentNullException.ThrowIfNull(cfg);
        ArgumentNullException.ThrowIfNull(loggerFactory);
        ArgumentNullException.ThrowIfNull(nav);

        Title = RS.MavParamsPageViewModel_Title;

        _loggerFactory = loggerFactory;
        _cfg = cfg;
        _config = _cfg.Get<ParamsConfig>();
        _nav = nav;
        _showStarredOnly = new ReactiveProperty<bool>().DisposeItWith(Disposable);
        _viewedParamsList = [];
        ViewedParams = _viewedParamsList.ToNotifyCollectionChangedSlim().DisposeItWith(Disposable);

        Search = new SearchBoxViewModel(
            nameof(Search),
            loggerFactory,
            UpdateImpl,
            TimeSpan.FromMilliseconds(500)
        )
            .SetRoutableParent(this)
            .DisposeItWith(Disposable);

        ShowStaredOnly = new HistoricalBoolProperty(
            nameof(ShowStaredOnly),
            _showStarredOnly,
            loggerFactory
        )
        {
            Parent = this,
        }.DisposeItWith(Disposable);
        IsRefreshing = new BindableReactiveProperty<bool>().DisposeItWith(Disposable);
        SelectedItem = new BindableReactiveProperty<ParamItemViewModel?>().DisposeItWith(
            Disposable
        );
        Progress = new BindableReactiveProperty<double>().DisposeItWith(Disposable);

        SelectedItem
            .Subscribe(value =>
            {
                var itemsToDelete = _viewedParamsList
                    .Where(_ => !_.IsPinned.ViewValue.Value)
                    .ToArray();

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
            })
            .DisposeItWith(Disposable);

        Disposable.AddAction(() =>
        {
            _config.Params = _config.Params.Where(_ => _.IsStarred || _.IsPinned).ToList();
            _cfg.Set(_config);

            if (_cancellationTokenSource is not null)
            {
                if (_cancellationTokenSource.Token.CanBeCanceled)
                {
                    _cancellationTokenSource?.Cancel(false);
                }
                _cancellationTokenSource?.Dispose();
                _cancellationTokenSource = null;
            }
        });

        UpdateParams = new BindableAsyncCommand(UpdateParamsCommand.Id, this);

        StopUpdateParams = new BindableAsyncCommand(StopUpdateParamsCommand.Id, this);

        RemoveAllPins = new ReactiveCommand(
            async (_, ct) =>
            {
                if (!AllParams?.Any(item => item.IsPinned.ViewValue.Value) ?? false)
                {
                    return;
                }

                await this.ExecuteCommand(
                    RemoveAllPinsCommand.Id,
                    CommandArg.CreateDictionary(),
                    ct
                );
            }
        ).DisposeItWith(Disposable);

        ShowStaredOnly
            .ViewValue.ThrottleLast(TimeSpan.FromMilliseconds(500))
            .Subscribe(x =>
            {
                if (!x && string.IsNullOrWhiteSpace(Search.Text.Value))
                {
                    _view?.ResetFilter();
                    return;
                }

                _view?.AttachFilter(
                    new SynchronizedViewFilter<KeyValuePair<string, ParamItem>, ParamItemViewModel>(
                        (_, model) => model.Filter(Search.Text.Value, x)
                    )
                );
            })
            .DisposeItWith(Disposable);
    }

    private Task UpdateImpl(string? query, IProgress<double> progress, CancellationToken cancel)
    {
        if (string.IsNullOrWhiteSpace(query) && !ShowStaredOnly.ViewValue.Value)
        {
            _view.ResetFilter();
            return Task.CompletedTask;
        }

        _view.AttachFilter(
            new SynchronizedViewFilter<KeyValuePair<string, ParamItem>, ParamItemViewModel>(
                (_, model) => model.Filter(query ?? string.Empty, ShowStaredOnly.ViewValue.Value)
            )
        );

        return Task.CompletedTask;
    }

    private void InternalInit(CancellationToken cancel)
    {
        if (_paramsClient is null)
        {
            throw new Exception($"Service of type {nameof(IParamsClientEx)} was not found");
        }

        Total = _paramsClient.RemoteCount.ToReadOnlyBindableReactiveProperty();
        _total.RegisterTo(cancel);
        Total.RegisterTo(cancel);
        _view = _paramsClient.Items.CreateView(kvp => new ParamItemViewModel(
            kvp.Key,
            kvp.Value,
            _loggerFactory,
            _config.Params.FirstOrDefault(_ => _.Name == kvp.Key)
        ));
        _view.RegisterTo(cancel);
        _view.DisposeMany().RegisterTo(cancel);
        _view.SetRoutableParent(this).RegisterTo(cancel);

        foreach (var item in _view.Where(item => item.IsPinned.ViewValue.Value))
        {
            if (_viewedParamsList.Contains(item))
            {
                continue;
            }

            _viewedParamsList.Add(item);
        }

        _view
            .ObserveAdd(cancellationToken: cancel)
            .Subscribe(e =>
            {
                foreach (var item in _config.Params)
                {
                    if (e.Value.View.Name == item.Name)
                    {
                        e.Value.View.SetConfig(item);
                    }

                    if (!e.Value.View.IsPinned.ViewValue.Value)
                    {
                        continue;
                    }

                    if (_viewedParamsList.Contains(e.Value.View))
                    {
                        continue;
                    }

                    _viewedParamsList.Add(e.Value.View);
                }
            })
            .RegisterTo(cancel);

        AllParams = _view.ToNotifyCollectionChanged();
        AllParams.RegisterTo(cancel);
        _allParams?.RegisterTo(cancel);
        Search.Refresh();
    }

    internal void StopUpdateParamsImpl()
    {
        if (!IsRefreshing.Value)
        {
            return;
        }

        if (_cancellationTokenSource != null)
        {
            if (_cancellationTokenSource.Token.CanBeCanceled)
            {
                _cancellationTokenSource?.Cancel(false);
            }
            _cancellationTokenSource?.Dispose();
            _cancellationTokenSource = null;
        }
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
                    Logger.LogInformation("User canceled updating params");
                    return;
                }

                Logger.LogError(ex, "Error to read all param items");
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
        _cancellationTokenSource = null;
    }

    protected override void AfterDeviceInitialized(IClientDevice device, CancellationToken cancel)
    {
        IsDeviceInitialized = true;
        Title = $"{RS.MavParamsPageViewModel_Title}[{device.Id}]";
        _paramsClient = device.GetMicroservice<IParamsClientEx>();
        DeviceName = device
            .Name.Select(x => x ?? RS.MavParamsPageViewModel_DeviceName_Unknown)
            .ToReadOnlyBindableReactiveProperty<string>();
        DeviceName.RegisterTo(cancel);
        _deviceId = device.Id;
        Icon = DeviceIconMixin.GetIcon(_deviceId) ?? PageIcon;
        cancel.Register(() => IsDeviceInitialized = false);
        InternalInit(cancel);
    }

    public HistoricalBoolProperty ShowStaredOnly { get; }
    public BindableReactiveProperty<bool> IsRefreshing { get; }
    public BindableReactiveProperty<double> Progress { get; }
    public ICommand UpdateParams { get; }
    public ICommand StopUpdateParams { get; }
    public ReactiveCommand RemoveAllPins { get; }
    public SearchBoxViewModel Search { get; }

    private INotifyCollectionChangedSynchronizedViewList<ParamItemViewModel>? _allParams;
    public INotifyCollectionChangedSynchronizedViewList<ParamItemViewModel>? AllParams
    {
        get => _allParams;
        private set => SetField(ref _allParams, value);
    }

    private IReadOnlyBindableReactiveProperty<int> _total;
    public IReadOnlyBindableReactiveProperty<int> Total
    {
        get => _total;
        private set => SetField(ref _total, value);
    }

    public INotifyCollectionChangedSynchronizedViewList<ParamItemViewModel> ViewedParams { get; }

    public IReadOnlyBindableReactiveProperty<string> DeviceName { get; private set; }

    public BindableReactiveProperty<ParamItemViewModel?> SelectedItem { get; }

    private bool _isDeviceInitialized;
    public bool IsDeviceInitialized
    {
        get => _isDeviceInitialized;
        set => SetField(ref _isDeviceInitialized, value);
    }

    protected override async ValueTask InternalCatchEvent(AsyncRoutedEvent e)
    {
        if (e is ParamItemChangedEvent { Source: ParamItemViewModel param } paramChanged)
        {
            UpdateConfig(param);

            if (paramChanged.Caller is HistoricalBoolProperty caller)
            {
                if (caller.Id == param.IsPinned.Id)
                {
                    UpdateViewedItems(param);
                }
            }

            e.IsHandled = true;
        }

        if (e is PageCloseAttemptEvent { Source: MavParamsPageViewModel } closeEvent)
        {
            var isCloseReady = await TryCloseWithApproval();
            if (!isCloseReady)
            {
                closeEvent.AddRestriction(new Restriction(this));
            }
        }

        await base.InternalCatchEvent(e);
    }

    private void UpdateViewedItems(ParamItemViewModel param)
    {
        var itemsToDelete = _viewedParamsList
            .Where(_ => !_.IsPinned.ViewValue.Value && _.Id != SelectedItem.Value?.Id)
            .ToArray();

        foreach (var item in itemsToDelete)
        {
            _viewedParamsList.Remove(item);
            return;
        }

        if (!_viewedParamsList.Contains(param))
        {
            _viewedParamsList.Add(param);
        }
    }

    private void UpdateConfig(ParamItemViewModel param)
    {
        var existItem = _config.Params.FirstOrDefault(_ => _.Name == param.Name);

        if (existItem is not null)
        {
            _config.Params.Remove(existItem);
        }

        _config.Params.Add(param.GetConfig());
    }

    private async Task<bool> TryCloseWithApproval()
    {
        var notSyncedParams = _viewedParamsList.Where(param => !param.IsSynced.Value).ToArray();

        if (notSyncedParams.Length == 0)
        {
            return true;
        }

        using var vm = new TryCloseWithApprovalDialogViewModel(_loggerFactory);
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
                param.Write.Execute(Unit.Default);
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
        yield return Search;
        yield return ShowStaredOnly;

        if (AllParams is not null)
        {
            foreach (var paramItemViewModel in AllParams)
            {
                yield return paramItemViewModel;
            }
        }
    }

    protected override void AfterLoadExtensions() { }

    public override IExportInfo Source => SystemModule.Instance;
}

file class ParamsKvpComparer : IComparer<KeyValuePair<string, ParamItem>>
{
    public static ParamsKvpComparer Instance { get; } = new();

    public int Compare(KeyValuePair<string, ParamItem> x, KeyValuePair<string, ParamItem> y)
    {
        return string.CompareOrdinal(x.Value.Name, y.Value.Name);
    }
}
