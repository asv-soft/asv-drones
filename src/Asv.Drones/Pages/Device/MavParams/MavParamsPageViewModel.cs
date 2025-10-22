using System;
using System.Collections.Generic;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Asv.Avalonia;
using Asv.Avalonia.IO;
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

public sealed class MavParamsPageViewModelConfig
{
    public IDictionary<string, ParamItemViewModelConfig> Params { get; } =
        new Dictionary<string, ParamItemViewModelConfig>();
    public string SearchText { get; set; } = string.Empty;
    public bool IsStarredOnly { get; set; }
}

[ExportPage(PageId)]
public class MavParamsPageViewModel
    : DevicePageViewModel<IMavParamsPageViewModel>,
        IMavParamsPageViewModel
{
    public const string PageId = "mav-params";
    public const MaterialIconKind PageIcon = MaterialIconKind.CogTransferOutline;

    private readonly ILayoutService _layoutService;
    private readonly ILoggerFactory _loggerFactory;
    private readonly INavigationService _nav;
    private readonly ObservableList<ParamItemViewModel> _viewedParamsList; // TODO: Separate views for this collection and all params
    private readonly ReactiveProperty<bool> _isStarredOnly;
    private readonly SynchronizedViewFilter<
        KeyValuePair<string, ParamItem>,
        ParamItemViewModel
    > _fullFilter;

    private DeviceId _deviceId;
    private IParamsClientEx? _paramsClient;
    private CancellationTokenSource? _cancellationTokenSource;
    private ISynchronizedView<KeyValuePair<string, ParamItem>, ParamItemViewModel> _view;
    private MavParamsPageViewModelConfig? _config;

    public MavParamsPageViewModel()
        : this(
            NullDeviceManager.Instance,
            NullCommandService.Instance,
            NullLoggerFactory.Instance,
            NullLayoutService.Instance,
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
        ILayoutService layoutService,
        INavigationService nav
    )
        : base(PageId, devices, cmd, layoutService, loggerFactory)
    {
        ArgumentNullException.ThrowIfNull(devices);
        ArgumentNullException.ThrowIfNull(cmd);
        ArgumentNullException.ThrowIfNull(layoutService);
        ArgumentNullException.ThrowIfNull(loggerFactory);
        ArgumentNullException.ThrowIfNull(nav);

        Title = RS.MavParamsPageViewModel_Title;

        _layoutService = layoutService;
        _loggerFactory = loggerFactory;
        _nav = nav;
        _isStarredOnly = new ReactiveProperty<bool>().DisposeItWith(Disposable);
        _viewedParamsList = [];
        ViewedParams = _viewedParamsList
            .ToNotifyCollectionChangedSlim(SynchronizationContextCollectionEventDispatcher.Current)
            .DisposeItWith(Disposable);

        Search = new SearchBoxViewModel(
            nameof(Search),
            loggerFactory,
            UpdateImpl,
            TimeSpan.FromMilliseconds(500)
        )
            .SetRoutableParent(this)
            .DisposeItWith(Disposable);

        IsStarredOnly = new HistoricalBoolProperty(
            nameof(IsStarredOnly),
            _isStarredOnly,
            loggerFactory,
            this
        ).DisposeItWith(Disposable);
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

        Disposable.AddAction(StopUpdateParamsImpl);

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

        IsStarredOnly
            .ViewValue.ThrottleLast(TimeSpan.FromMilliseconds(500))
            .Subscribe(x => UpdateFilter())
            .DisposeItWith(Disposable);

        Progress
            .Where(p => p.ApproximatelyEquals(1.0))
            .Subscribe(_ => IsRefreshing.Value = false)
            .DisposeItWith(Disposable);
        IsRefreshing
            .Where(isRefreshing => isRefreshing)
            .Subscribe(_ => Progress.Value = 0)
            .DisposeItWith(Disposable);

        _fullFilter = new SynchronizedViewFilter<
            KeyValuePair<string, ParamItem>,
            ParamItemViewModel
        >(
            (_, model) =>
                model.Filter(
                    Search.Text.ViewValue.Value ?? string.Empty,
                    IsStarredOnly.ViewValue.Value
                )
        );
    }

    private Task UpdateImpl(string? query, IProgress<double> progress, CancellationToken cancel)
    {
        UpdateFilter();
        return Task.CompletedTask;
    }

    private void UpdateFilter()
    {
        if (
            string.IsNullOrWhiteSpace(Search.Text.ViewValue.Value) && !IsStarredOnly.ViewValue.Value
        )
        {
            _view?.ResetFilter();
            return;
        }

        _view?.AttachFilter(_fullFilter);
    }

    private void InternalInit(CancellationToken cancel)
    {
        if (_paramsClient is null)
        {
            throw new Exception($"Service of type {nameof(IParamsClientEx)} was not found");
        }

        Total = _paramsClient.RemoteCount.ToReadOnlyBindableReactiveProperty();
        Total.RegisterTo(cancel);
        _view = _paramsClient.Items.CreateView(kvp => new ParamItemViewModel(
            kvp.Key,
            kvp.Value,
            GetConfigFor,
            SetConfigFor,
            _loggerFactory
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
            .SubscribeAwait(
                async (e, ct) =>
                {
                    await e.Value.View.RequestLoadLayout(_layoutService, ct);

                    if (!e.Value.View.IsPinned.ViewValue.Value)
                    {
                        return;
                    }

                    if (_viewedParamsList.Contains(e.Value.View))
                    {
                        return;
                    }

                    _viewedParamsList.Add(e.Value.View);
                }
            )
            .RegisterTo(cancel);

        AllParams = _view.ToNotifyCollectionChanged(
            SynchronizationContextCollectionEventDispatcher.Current
        );
        AllParams.RegisterTo(cancel);
        Search.Refresh();
    }

    internal void StopUpdateParamsImpl()
    {
        if (_cancellationTokenSource is not null)
        {
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource?.Dispose();
            _cancellationTokenSource = null;
        }

        IsRefreshing.Value = false;
    }

    internal async Task UpdateParamsImpl(CancellationToken cancel = default)
    {
        if (IsRefreshing.Value)
        {
            return;
        }

        if (_paramsClient is null)
        {
            return;
        }

        cancel.ThrowIfCancellationRequested();
        if (_cancellationTokenSource is not null)
        {
            StopUpdateParamsImpl();
        }

        SelectedItem.Value = null;
        IsRefreshing.Value = true;

        _cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancel);
        _cancellationTokenSource.Token.Register(() => IsRefreshing.Value = false);
        try
        {
            await _paramsClient.ReadAll(
                new Progress<double>(i => Progress.Value = i),
                cancel: _cancellationTokenSource.Token
            );
        }
        catch (OperationCanceledException)
        {
            Logger.LogInformation("User canceled updating params");
        }
        catch (Exception e)
        {
            Logger.LogError(e, "Error to read all param items");
        }
    }

    private ParamItemViewModelConfig? GetConfigFor(string name)
    {
        if (_config is null)
        {
            return null;
        }

        _config.Params.TryGetValue(name, out var config);
        return config;
    }

    private void SetConfigFor(string name, ParamItemViewModelConfig config)
    {
        if (_config is null)
        {
            return;
        }

        _config.Params[name] = config;
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

    public HistoricalBoolProperty IsStarredOnly { get; }
    public BindableReactiveProperty<bool> IsRefreshing { get; }
    public BindableReactiveProperty<double> Progress { get; }
    public ICommand UpdateParams { get; }
    public ICommand StopUpdateParams { get; }
    public ReactiveCommand RemoveAllPins { get; }
    public SearchBoxViewModel Search { get; }

    public INotifyCollectionChangedSynchronizedViewList<ParamItemViewModel>? AllParams
    {
        get;
        private set;
    }

    public IReadOnlyBindableReactiveProperty<int> Total { get; private set; }

    public INotifyCollectionChangedSynchronizedViewList<ParamItemViewModel> ViewedParams { get; }

    public IReadOnlyBindableReactiveProperty<string> DeviceName { get; private set; }

    public BindableReactiveProperty<ParamItemViewModel?> SelectedItem { get; }

    public bool IsDeviceInitialized
    {
        get;
        set => SetField(ref field, value);
    }

    public override IEnumerable<IRoutable> GetRoutableChildren()
    {
        yield return Search;
        yield return IsStarredOnly;

        if (AllParams is not null)
        {
            foreach (var paramItemViewModel in AllParams)
            {
                yield return paramItemViewModel;
            }
        }
    }

    protected override void AfterLoadExtensions() { }

    protected override async ValueTask InternalCatchEvent(AsyncRoutedEvent e)
    {
        switch (e)
        {
            case ParamItemChangedEvent { Source: ParamItemViewModel param } paramChanged:
            {
                if (paramChanged.Caller is HistoricalBoolProperty caller)
                {
                    if (caller.Id == param.IsPinned.Id)
                    {
                        UpdateViewedItems(param);
                    }
                }

                e.IsHandled = true;
                break;
            }

            case PageCloseAttemptEvent { Source: MavParamsPageViewModel } closeEvent:
            {
                var isCloseReady = await TryCloseWithApproval();
                if (!isCloseReady)
                {
                    closeEvent.AddRestriction(new Restriction(this));
                }

                break;
            }

            case SaveLayoutEvent saveLayoutEvent:
            {
                if (_config is null)
                {
                    break;
                }

                saveLayoutEvent.HandleSaveLayout(
                    this,
                    _config,
                    cfg =>
                    {
                        cfg.SearchText = Search.Text.ViewValue.Value ?? string.Empty;
                        cfg.IsStarredOnly = IsStarredOnly.ViewValue.Value;
                    }
                );
                break;
            }

            case LoadLayoutEvent loadLayoutEvent:
            {
                _config = loadLayoutEvent.HandleLoadLayout<MavParamsPageViewModelConfig>(
                    this,
                    cfg =>
                    {
                        Search.Text.ModelValue.Value = cfg.SearchText;
                        IsStarredOnly.ModelValue.Value = cfg.IsStarredOnly;
                    }
                );
                break;
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

    private async Task<bool> TryCloseWithApproval(CancellationToken cancel = default)
    {
        cancel.ThrowIfCancellationRequested();
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

        if (result == ContentDialogResult.None)
        {
            return false;
        }

        if (result == ContentDialogResult.Primary)
        {
            foreach (var param in notSyncedParams)
            {
                param.Write.Execute(Unit.Default);
            }
        }

        return true;
    }

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
