using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Asv.Avalonia;
using Asv.Common;
using Asv.Drones.Api;
using Asv.Mavlink;
using Asv.Modeling;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using ObservableCollections;
using R3;

namespace Asv.Drones;

public class TelemetrySectionViewModel : ViewModel, ITelemetrySection
{
    public const string SectionId = "telemetry-widget-section";
    private const string SetItemsChangeId = $"{SectionId}.set-items";

    private readonly ILogger<TelemetrySectionViewModel> _logger;
    private readonly IReadOnlyDictionary<string, ITelemetryItemFactory> _factories;
    private readonly ObservableList<IViewModel> _displayItems;
    private readonly Dictionary<string, TelemetryDisplayItemViewModel> _displayItemsById = [];
    private readonly AddTelemetryDisplayItemViewModel _addItem;
    private readonly IUndoChangeSink<ValueUndoChange<string[]>> _setItemsChangeSink;

    private readonly IReadOnlyList<string> _defaultTelemetryItemIds;
    private readonly MavlinkClientDevice? _device;

    public TelemetrySectionViewModel()
        : this(
            new TelemetrySectionArgs(
                null,
                [
                    BatteryTelemetryItemFactory.Id,
                    AltitudeTelemetryItemFactory.Id,
                    VelocityTelemetryItemFactory.Id,
                    AngleTelemetryItemFactory.Id,
                ]
            ),
            NullLoggerFactory.Instance,
            [
                new BatteryTelemetryItemFactory(
                    DeviceTelemetryDesignPreview.UnitService,
                    DesignTime.LoggerFactory
                ),
                new AltitudeTelemetryItemFactory(DeviceTelemetryDesignPreview.UnitService),
                new VelocityTelemetryItemFactory(
                    DeviceTelemetryDesignPreview.UnitService,
                    DesignTime.LoggerFactory
                ),
                new AngleTelemetryItemFactory(DeviceTelemetryDesignPreview.UnitService),
            ]
        )
    {
        DesignTime.ThrowIfNotDesignMode();
        IsEditMode.Value = true;
    }

    public TelemetrySectionViewModel(
        TelemetrySectionArgs args,
        ILoggerFactory loggerFactory,
        IEnumerable<ITelemetryItemFactory> factories
    )
        : base(SectionId)
    {
        ArgumentNullException.ThrowIfNull(args);
        ArgumentNullException.ThrowIfNull(loggerFactory);
        ArgumentNullException.ThrowIfNull(factories);

        _device = args.Device;
        _defaultTelemetryItemIds = args.DefaultItemIds;
        _logger = loggerFactory.CreateLogger<TelemetrySectionViewModel>();
        _factories = factories.ToDictionary(f => f.ItemId);

        Items = [];
        Items.DisposeRemovedItems().DisposeItWith(Disposable);
        Disposable.AddAction(() => Items.ClearWithItemsDispose());
        _displayItems = [];
        _displayItems.SetRoutableParent(this).DisposeItWith(Disposable);
        DisplayItemsView = _displayItems.ToNotifyCollectionChangedSlim().DisposeItWith(Disposable);

        _addItem = new AddTelemetryDisplayItemViewModel()
            .SetRoutableParent(this)
            .DisposeItWith(Disposable);

        IsEditMode = new BindableReactiveProperty<bool>(false).DisposeItWith(Disposable);
        ToggleEdit = new ReactiveCommand(_ => IsEditMode.Value = !IsEditMode.Value).DisposeItWith(
            Disposable
        );
        Reset = new ReactiveCommand(_ => TryResetItems()).DisposeItWith(Disposable);

        WireUpDisplayItemsSync();
        IsEditMode.Subscribe(_ => UpdateAddItemPresence()).DisposeItWith(Disposable);

        Events.Catch(InternalCatchEvent).DisposeItWith(Disposable);
        _setItemsChangeSink = Undo.RegisterValue<string[]>(
                SetItemsChangeId,
                oldValue => TryApplyItems(oldValue),
                newValue => TryApplyItems(newValue)
            )
            .DisposeItWith(Disposable);

        TryApplyItems(_defaultTelemetryItemIds);

        LayoutControllerMixin
            .Register(
                Layout,
                SectionId,
                config =>
                {
                    TryApplyItems(config.ItemIds ?? _defaultTelemetryItemIds);
                },
                () => new TelemetrySectionConfig { ItemIds = ItemIds },
                Items.ObserveChanged().Select(_ => Unit.Default)
            )
            .DisposeItWith(Disposable);
        Layout.LoadWhenRootAttached(RootTracking).AddTo(ref DisposableBag);
    }

    public ObservableList<ITelemetryItem> Items { get; }
    public INotifyCollectionChangedSynchronizedViewList<IViewModel> DisplayItemsView { get; }
    public BindableReactiveProperty<bool> IsEditMode { get; }
    public ICommand ToggleEdit { get; }
    public ICommand Reset { get; }

    public int Order => 2;

    public bool TryAddItem(string itemId)
    {
        if (string.IsNullOrWhiteSpace(itemId) || Items.Any(i => i.ItemId == itemId))
        {
            return false;
        }

        return TrySetItems(ItemIds.Append(itemId).ToArray());
    }

    public bool TryRemoveItem(string itemId)
    {
        if (string.IsNullOrWhiteSpace(itemId))
        {
            return false;
        }

        return TrySetItems(ItemIds.Where(id => id != itemId).ToArray());
    }

    public bool TrySetItems(IReadOnlyList<string> itemIds)
    {
        ArgumentNullException.ThrowIfNull(itemIds);

        var oldItemIds = ItemIds;
        if (!TryApplyItems(itemIds))
        {
            return false;
        }

        _setItemsChangeSink.PublishUpdate(oldItemIds, ItemIds);
        return true;
    }

    public bool TryResetItems()
    {
        return TrySetItems(_defaultTelemetryItemIds);
    }

    private ITelemetryItem? TryCreateItem(string itemId)
    {
        if (string.IsNullOrWhiteSpace(itemId))
        {
            return null;
        }

        if (!_factories.TryGetValue(itemId, out var factory))
        {
            _logger.LogWarning("No telemetry item factory registered for id '{ItemId}'", itemId);
            return null;
        }

        try
        {
            if (_device is null)
            {
                return factory.CreatePreview();
            }

            var createdItem = factory.TryCreate(_device);

            if (createdItem is null)
            {
                _logger.LogDebug(
                    "Skipping telemetry item '{ItemId}': not supported by device {DeviceId}",
                    itemId,
                    _device.Id
                );
                return null;
            }

            return createdItem;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create telemetry item '{ItemId}'", itemId);
            return null;
        }
    }

    private bool IsAddItemAtTail =>
        _displayItems.Count > 0 && ReferenceEquals(_displayItems[^1], _addItem);

    private string[] ItemIds => Items.Select(i => i.ItemId).ToArray();

    private bool TryApplyItems(IReadOnlyList<string> itemIds)
    {
        ArgumentNullException.ThrowIfNull(itemIds);

        var seen = new HashSet<string>(itemIds.Count);
        var targetIndex = 0;
        var changed = false;

        foreach (var targetId in itemIds)
        {
            if (string.IsNullOrWhiteSpace(targetId) || !seen.Add(targetId))
            {
                continue;
            }

            var existingIndex = -1;
            for (var j = targetIndex; j < Items.Count; j++)
            {
                if (Items[j].ItemId == targetId)
                {
                    existingIndex = j;
                    break;
                }
            }

            if (existingIndex >= 0)
            {
                if (existingIndex != targetIndex)
                {
                    Items.Move(existingIndex, targetIndex);
                    changed = true;
                }
            }
            else
            {
                var item = TryCreateItem(targetId);
                if (item is null)
                {
                    continue;
                }
                Items.Insert(targetIndex, item);
                changed = true;
            }

            targetIndex++;
        }

        while (Items.Count > targetIndex)
        {
            Items.RemoveAt(Items.Count - 1);
            changed = true;
        }

        return changed;
    }

    private void WireUpDisplayItemsSync()
    {
        Items
            .ObserveAdd()
            .ObserveOnUIThreadDispatcher()
            .Subscribe(e =>
            {
                var wrapper = new TelemetryDisplayItemViewModel(
                    e.Value,
                    IsEditMode
                ).SetRoutableParent(this);

                _displayItemsById[wrapper.ItemId] = wrapper;
                _displayItems.Insert(e.Index, wrapper);

                UpdateAddItemPresence();
            })
            .DisposeItWith(Disposable);

        Items
            .ObserveRemove()
            .ObserveOnUIThreadDispatcher()
            .Subscribe(e =>
            {
                if (_displayItemsById.Remove(e.Value.ItemId, out var wrapper))
                {
                    _displayItems.Remove(wrapper);
                    wrapper.Dispose();
                }
                UpdateAddItemPresence();
            })
            .DisposeItWith(Disposable);

        Items
            .ObserveMove()
            .ObserveOnUIThreadDispatcher()
            .Subscribe(e =>
            {
                if (!_displayItemsById.TryGetValue(e.Value.ItemId, out var wrapper))
                {
                    return;
                }
                var currentIndex = _displayItems.IndexOf(wrapper);
                if (currentIndex >= 0 && currentIndex != e.NewIndex)
                {
                    _displayItems.Move(currentIndex, e.NewIndex);
                }
            })
            .DisposeItWith(Disposable);

        Items
            .ObserveReset()
            .ObserveOnUIThreadDispatcher()
            .Subscribe(_ =>
            {
                DisposeAllWrappers();
                UpdateAddItemPresence();
            })
            .DisposeItWith(Disposable);

        Disposable.AddAction(DisposeAllWrappers);
    }

    private void UpdateAddItemPresence()
    {
        var canAddMore = _factories.Values.Any(f =>
            (_device is null || f.CanCreate(_device)) && Items.All(i => i.ItemId != f.ItemId)
        );
        var shouldBePresent = canAddMore && (IsEditMode.Value || Items.Count == 0);

        if (shouldBePresent && !IsAddItemAtTail)
        {
            _displayItems.Add(_addItem);
        }
        else if (!shouldBePresent && IsAddItemAtTail)
        {
            _displayItems.Remove(_addItem);
        }
    }

    private async ValueTask ShowConfigureDialogAsync(CancellationToken ct)
    {
        var alreadyAdded = Items.Select(i => i.ItemId).ToHashSet();
        var availableFactories = _factories
            .Values.Where(f =>
                (_device is null || f.CanCreate(_device)) && !alreadyAdded.Contains(f.ItemId)
            )
            .ToArray();
        if (availableFactories.Length == 0)
        {
            return;
        }

        using var vm = new ConfigureTelemetryDialogViewModel(availableFactories);

        var dialog = new ContentDialog(vm)
        {
            Title = RS.TelemetrySectionViewModel_ConfigureTelemetryDialog_Title,
            CloseButtonText = RS.TelemetrySectionViewModel_ConfigureTelemetryDialog_CloseButtonText,
        };
        vm.ApplyDialog(dialog);

        await dialog.ShowAsync();

        if (!string.IsNullOrWhiteSpace(vm.SelectedItemId))
        {
            ct.ThrowIfCancellationRequested();
            TryAddItem(vm.SelectedItemId);
        }
    }

    private void DisposeAllWrappers()
    {
        for (var i = _displayItems.Count - 1; i >= 0; i--)
        {
            if (_displayItems[i] is TelemetryDisplayItemViewModel wrapper)
            {
                _displayItems.RemoveAt(i);
                wrapper.Dispose();
            }
        }
        _displayItemsById.Clear();
    }

    private async ValueTask InternalCatchEvent(
        IViewModel src,
        AsyncRoutedEvent<IViewModel> e,
        CancellationToken cancel
    )
    {
        switch (e)
        {
            case TelemetryDisplayItemAddRequestedEvent addRequested:
                await ShowConfigureDialogAsync(addRequested.Cancel);
                e.IsHandled = true;
                break;
            case TelemetryDisplayItemRemoveRequestedEvent removeRequested:
                TryRemoveItem(removeRequested.ItemId);
                e.IsHandled = true;
                break;
        }
    }

    public override IEnumerable<IViewModel> GetChildren() => _displayItems;
}
