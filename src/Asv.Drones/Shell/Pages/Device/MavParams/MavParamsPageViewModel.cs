using System;
using System.Collections.Generic;
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
using Asv.Modeling;
using Material.Icons;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using ObservableCollections;
using R3;

namespace Asv.Drones;

public sealed class MavParamsPageViewModelConfig
{
    public IDictionary<string, ParamItemViewModelConfig> Params { get; set; } =
        new Dictionary<string, ParamItemViewModelConfig>();
    public string SearchText { get; set; } = string.Empty;
    public bool IsStarredOnly { get; set; }
}

public class MavParamsPageViewModel
    : DevicePageViewModel<IMavParamsPageViewModel>,
        IMavParamsPageViewModel
{
    public const string PageId = "mavParams";
    public const MaterialIconKind PageIcon = MaterialIconKind.CogTransferOutline;

    private readonly ILoggerFactory _loggerFactory;
    private readonly ObservableList<ParamItemViewModel> _viewedParamsList; // TODO: Separate views for this collection and all params
    private readonly ReactiveProperty<bool> _isStarredOnly;
    private readonly SynchronizedViewFilter<
        KeyValuePair<string, ParamItem>,
        ParamItemViewModel
    > _fullFilter;
    private readonly Lock _cancelLock = new();
    private readonly IUndoChangeSink<ValueUndoChange<IReadOnlyList<string>>> _removeAllPinsUndo;
    private readonly Subject<Unit> _layoutChanged = new();
    private readonly ILogger _logger;

    private IParamsClientEx? _paramsClient;
    private CancellationTokenSource? _cancellationTokenSource;
    private ISynchronizedView<KeyValuePair<string, ParamItem>, ParamItemViewModel>? _view;
    private MavParamsPageViewModelConfig? _config;

    public MavParamsPageViewModel()
        : this(
            new PageContext(
                new NavArgs((DevicePageViewModelMixin.ArgsDeviceIdKey, "design")),
                NullUndoHistoryStore.Instance,
                NullLayoutStore.Instance
            ),
            NullDeviceManager.Instance,
            NullLoggerFactory.Instance,
            DesignTime.DialogService,
            DesignTime.ExtensionService
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
    }

    public MavParamsPageViewModel(
        IPageContext context,
        IDeviceManager devices,
        ILoggerFactory loggerFactory,
        IDialogService dialogService,
        IExtensionService ext
    )
        : base(PageId, context, devices, loggerFactory, dialogService, ext)
    {
        Header = RS.MavParamsPageViewModel_Title;

        _loggerFactory = loggerFactory;
        _logger = _loggerFactory.CreateLogger(GetType());

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

        IsStarredOnly = new HistoricalBoolProperty(nameof(IsStarredOnly), _isStarredOnly)
            .SetRoutableParent(this)
            .DisposeItWith(Disposable);
        IsRefreshing = new BindableReactiveProperty<bool>().DisposeItWith(Disposable);
        SelectedItem = new BindableReactiveProperty<ParamItemViewModel?>().DisposeItWith(
            Disposable
        );
        Progress = new BindableReactiveProperty<double>().DisposeItWith(Disposable);

        SelectedItem
            .Subscribe(value =>
            {
                var itemsToDelete = _viewedParamsList
                    .Where(p => !p.IsPinned.ViewValue.Value)
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

        UpdateParams = new ReactiveCommand((_, ct) => UpdateParamsImpl(ct)).DisposeItWith(
            Disposable
        );

        StopUpdateParams = new ReactiveCommand(_ => StopUpdateParamsImpl()).DisposeItWith(
            Disposable
        );

        _removeAllPinsUndo = Undo.RegisterValue<IReadOnlyList<string>>(
                "removeAllPins",
                ApplyPinnedSet,
                ApplyPinnedSet
            )
            .DisposeItWith(Disposable);

        RemoveAllPins = new ReactiveCommand(_ =>
        {
            if (AllParams is null)
            {
                return;
            }

            var pinnedNames = AllParams
                .Where(item => item.IsPinned.ViewValue.Value)
                .Select(item => item.Name)
                .ToArray();
            if (pinnedNames.Length == 0)
            {
                return;
            }

            _removeAllPinsUndo.PublishUpdate(pinnedNames, []);
            ApplyPinnedSet([]);
        }).DisposeItWith(Disposable);

        IsStarredOnly
            .ViewValue.ThrottleLast(TimeSpan.FromMilliseconds(500))
            .Subscribe(_ => UpdateFilter())
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

        _layoutChanged.DisposeItWith(Disposable);
        Search
            .Text.ViewValue.Skip(1)
            .Subscribe(_ => _layoutChanged.OnNext(Unit.Default))
            .DisposeItWith(Disposable);
        IsStarredOnly
            .ViewValue.Skip(1)
            .Subscribe(_ => _layoutChanged.OnNext(Unit.Default))
            .DisposeItWith(Disposable);

        Events.Catch(InternalCatchEvent).DisposeItWith(Disposable);

        Target
            .Where(w => w.HasValue)
            .Select(w => w!.Value)
            .ObserveOnUIThreadDispatcher()
            .Subscribe(w => OnDeviceConnected(w.Device, w.WhenDisconnectedToken))
            .DisposeItWith(Disposable);
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
        private set => SetField(ref field, value);
    }

    public IReadOnlyBindableReactiveProperty<int>? Total
    {
        get;
        private set => SetField(ref field, value);
    }

    public INotifyCollectionChangedSynchronizedViewList<ParamItemViewModel> ViewedParams { get; }

    public BindableReactiveProperty<ParamItemViewModel?> SelectedItem { get; }

    private void OnDeviceConnected(IClientDevice device, CancellationToken cancel)
    {
        Header = $"{RS.MavParamsPageViewModel_Title}[{device.Id}]";
        _paramsClient = device.GetMicroservice<IParamsClientEx>();
        Icon = DeviceIconMixin.GetIcon(device.Id) ?? PageIcon;
        InternalInit(cancel);
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

        SelectedItem.Value = null;
        _viewedParamsList.Clear();

        Total = _paramsClient
            .RemoteCount.ObserveOnUIThreadDispatcher()
            .ToReadOnlyBindableReactiveProperty();
        Total.RegisterTo(cancel);
        _view = _paramsClient.Items.CreateView(kvp =>
        {
            ParamItemViewModelConfig? config = null;
            _config?.Params.TryGetValue(kvp.Key, out config);

            return new ParamItemViewModel(
                new NavArgs(("id", kvp.Key)),
                kvp.Value,
                _loggerFactory,
                config
            );
        });
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
                if (!e.Value.View.IsPinned.ViewValue.Value)
                {
                    return;
                }

                if (_viewedParamsList.Contains(e.Value.View))
                {
                    return;
                }

                _viewedParamsList.Add(e.Value.View);
            })
            .RegisterTo(cancel);

        AllParams = _view.ToNotifyCollectionChanged(
            SynchronizationContextCollectionEventDispatcher.Current
        );
        AllParams.RegisterTo(cancel);
        Search.Refresh(cancel);
    }

    private void StopUpdateParamsImpl()
    {
        using (_cancelLock.EnterScope())
        {
            if (_cancellationTokenSource is not null)
            {
                _cancellationTokenSource?.Cancel();
                _cancellationTokenSource?.Dispose();
                _cancellationTokenSource = null;
            }
        }

        if (!IsRefreshing.IsDisposed)
        {
            IsRefreshing.Value = false;
        }
    }

    private async ValueTask UpdateParamsImpl(CancellationToken cancel = default)
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

        using (_cancelLock.EnterScope())
        {
            _cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancel);
        }

        try
        {
            await _paramsClient.ReadAll(
                new Progress<double>(i => Progress.Value = i),
                cancel: _cancellationTokenSource.Token
            );
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("User canceled updating params");
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error to read all param items");
        }
        finally
        {
            if (!IsRefreshing.IsDisposed)
            {
                IsRefreshing.Value = false;
            }
        }
    }

    private void ApplyPinnedSet(IReadOnlyList<string> pinnedNames)
    {
        if (AllParams is null)
        {
            return;
        }

        var target = pinnedNames.ToHashSet();

        foreach (var item in AllParams)
        {
            var shouldPin = target.Contains(item.Name);
            if (item.IsPinned.ViewValue.Value != shouldPin)
            {
                item.IsPinned.ModelValue.Value = shouldPin;
            }
        }

        var keep = AllParams.Where(item => target.Contains(item.Name)).ToHashSet();
        if (SelectedItem.Value is not null)
        {
            keep.Add(SelectedItem.Value);
        }

        foreach (var item in _viewedParamsList.Where(item => !keep.Contains(item)).ToArray())
        {
            _viewedParamsList.Remove(item);
        }

        foreach (var item in keep)
        {
            if (!_viewedParamsList.Contains(item))
            {
                _viewedParamsList.Add(item);
            }
        }
    }

    public override IEnumerable<IViewModel> GetChildren()
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

    protected override void AfterLoadExtensions()
    {
        ViewLayoutMixin
            .Register(
                Layout,
                nameof(MavParamsPageViewModel),
                LoadLayout,
                SaveLayout,
                _layoutChanged
                    .ThrottleLast(TimeSpan.FromMilliseconds(200))
                    .ObserveOnUIThreadDispatcher()
            )
            .DisposeItWith(Disposable);
        Layout.LoadWhenRootAttached(RootTracking).DisposeItWith(Disposable);
    }

    private MavParamsPageViewModelConfig SaveLayout()
    {
        var paramsConfig =
            AllParams
                ?.Where(item => item.IsPinned.ViewValue.Value || item.IsStarred.ViewValue.Value)
                .ToDictionary(
                    item => item.Name,
                    item => new ParamItemViewModelConfig
                    {
                        Name = item.Name,
                        IsStarred = item.IsStarred.ViewValue.Value,
                        IsPinned = item.IsPinned.ViewValue.Value,
                    }
                ) ?? (_config?.Params ?? new Dictionary<string, ParamItemViewModelConfig>());

        return new MavParamsPageViewModelConfig
        {
            Params = paramsConfig,
            SearchText = Search.Text.ViewValue.Value ?? string.Empty,
            IsStarredOnly = IsStarredOnly.ViewValue.Value,
        };
    }

    private void LoadLayout(MavParamsPageViewModelConfig config)
    {
        _config = config;

        Search.Text.ModelValue.Value = config.SearchText;
        IsStarredOnly.ModelValue.Value = config.IsStarredOnly;

        if (AllParams is not null)
        {
            foreach (var item in AllParams)
            {
                if (!_config.Params.TryGetValue(item.Name, out var cfg))
                {
                    continue;
                }

                item.IsStarred.ModelValue.Value = cfg.IsStarred;
                item.IsPinned.ModelValue.Value = cfg.IsPinned;
            }
        }
    }

    private async ValueTask InternalCatchEvent(
        IViewModel src,
        AsyncRoutedEvent<IViewModel> e,
        CancellationToken cancel
    )
    {
        switch (e)
        {
            case ParamItemChangedEvent { Sender: ParamItemViewModel param } paramChanged:
            {
                if (paramChanged.TrackedObject is HistoricalBoolProperty caller)
                {
                    if (caller.Id == param.IsPinned.Id)
                    {
                        UpdateViewedItems(param);
                    }
                }

                _layoutChanged.OnNext(Unit.Default);
                e.IsHandled = true;
                break;
            }

            case PageCloseAttemptEvent { Sender: MavParamsPageViewModel } closeEvent:
            {
                var isCloseReady = await TryCloseWithApproval(cancel);
                if (!isCloseReady)
                {
                    closeEvent.AddRestriction(new Restriction(this));
                }

                break;
            }
        }
    }

    private void UpdateViewedItems(ParamItemViewModel param)
    {
        var itemsToDelete = _viewedParamsList
            .Where(p => !p.IsPinned.ViewValue.Value && p.Id != SelectedItem.Value?.Id)
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
        var dialog = new ContentDialog(vm)
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
}
