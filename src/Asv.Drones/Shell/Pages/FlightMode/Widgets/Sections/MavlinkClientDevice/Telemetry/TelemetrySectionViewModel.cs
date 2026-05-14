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

public sealed class TelemetrySectionConfig
{
    public string[]? ItemIds { get; set; }
}

public class TelemetrySectionViewModel : RoutableViewModel, ITelemetrySection
{
    public const string SectionId = "telemetry-widget-section";

    public static readonly string[] DefaultItemIds =
    [
        BatteryTelemetryItemFactory.Id,
        AltitudeTelemetryItemFactory.Id,
        VelocityTelemetryItemFactory.Id,
        AngleTelemetryItemFactory.Id,
    ];

    private readonly ILoggerFactory _loggerFactory;
    private readonly INavigationService _navigationService;
    private readonly IReadOnlyDictionary<string, ITelemetryItemFactory> _factories;
    private readonly ObservableList<ITelemetryDisplayItem> _displayItems;
    private readonly TelemetrySectionDisplayComposer _displayComposer;

    private MavlinkClientDevice? _device;
    private TelemetrySectionConfig? _config;

    public TelemetrySectionViewModel()
        : base(SectionId, DesignTime.LoggerFactory)
    {
        DesignTime.ThrowIfNotDesignMode();

        _loggerFactory = NullLoggerFactory.Instance;
        _navigationService = NullNavigationService.Instance;
        _factories = new ITelemetryItemFactory[]
        {
            new BatteryTelemetryItemFactory(
                DeviceTelemetryDesignPreview.UnitService,
                DesignTime.LoggerFactory
            ),
            new AltitudeTelemetryItemFactory(
                DeviceTelemetryDesignPreview.UnitService,
                DesignTime.LoggerFactory
            ),
            new VelocityTelemetryItemFactory(
                DeviceTelemetryDesignPreview.UnitService,
                DesignTime.LoggerFactory
            ),
            new AngleTelemetryItemFactory(
                DeviceTelemetryDesignPreview.UnitService,
                DesignTime.LoggerFactory
            ),
        }.ToDictionary(f => f.ItemId);
        _displayComposer = new TelemetrySectionDisplayComposer(DesignTime.LoggerFactory);

        Items = [];
        Items.SetRoutableParent(this).DisposeItWith(Disposable);
        Items.DisposeRemovedItems().DisposeItWith(Disposable);
        _displayItems = [];
        _displayItems.SetRoutableParent(this).DisposeItWith(Disposable);
        _displayItems.DisposeRemovedItems().DisposeItWith(Disposable);
        DisplayItemsView = _displayItems.ToNotifyCollectionChangedSlim().DisposeItWith(Disposable);

        IsEditMode = new BindableReactiveProperty<bool>(true).DisposeItWith(Disposable);
        ToggleEdit = new ReactiveCommand(_ => IsEditMode.Value = !IsEditMode.Value).DisposeItWith(
            Disposable
        );
        Reset = new ReactiveCommand(_ =>
        {
            Items.RemoveAll();

            foreach (var itemId in DefaultItemIds)
            {
                var item = _factories[itemId].CreatePreview();
                Items.Add(item);
            }
        }).DisposeItWith(Disposable);

        IsEditMode.Subscribe(_ => RefreshDisplayItems()).DisposeItWith(Disposable);
        Items.ObserveCountChanged().Subscribe(_ => RefreshDisplayItems()).DisposeItWith(Disposable);
        Events.Subscribe(InternalCatchEvent).DisposeItWith(Disposable);

        Reset.Execute(null);

        InitArgs("1");
    }

    public TelemetrySectionViewModel(
        ILoggerFactory loggerFactory,
        INavigationService navigationService,
        IEnumerable<ITelemetryItemFactory> factories
    )
        : base(SectionId, loggerFactory)
    {
        _loggerFactory = loggerFactory;
        _navigationService = navigationService;
        _factories = factories.ToDictionary(f => f.ItemId);
        _displayComposer = new TelemetrySectionDisplayComposer(loggerFactory);

        Items = [];
        Items.SetRoutableParent(this).DisposeItWith(Disposable);
        Items.DisposeRemovedItems().DisposeItWith(Disposable);
        _displayItems = [];
        _displayItems.SetRoutableParent(this).DisposeItWith(Disposable);
        _displayItems.DisposeRemovedItems().DisposeItWith(Disposable);
        DisplayItemsView = _displayItems.ToNotifyCollectionChangedSlim().DisposeItWith(Disposable);

        IsEditMode = new BindableReactiveProperty<bool>(false).DisposeItWith(Disposable);
        ToggleEdit = new ReactiveCommand(_ => IsEditMode.Value = !IsEditMode.Value).DisposeItWith(
            Disposable
        );
        Reset = new ReactiveCommand(
            async (_, ct) =>
                await this.ExecuteCommand(ResetTelemetryItemsCommand.Id, CommandArg.Empty, ct)
        ).DisposeItWith(Disposable);

        IsEditMode.Subscribe(_ => RefreshDisplayItems()).DisposeItWith(Disposable);
        Items.ObserveCountChanged().Subscribe(_ => RefreshDisplayItems()).DisposeItWith(Disposable);
        Events.Subscribe(InternalCatchEvent).DisposeItWith(Disposable);
    }

    public ObservableList<ITelemetryItem> Items { get; }
    public INotifyCollectionChangedSynchronizedViewList<ITelemetryDisplayItem> DisplayItemsView { get; }
    public BindableReactiveProperty<bool> IsEditMode { get; }
    public ICommand ToggleEdit { get; }
    public ICommand Reset { get; }

    public int Order => 2;

    public void InitWith(MavlinkClientDevice device)
    {
        ArgumentNullException.ThrowIfNull(device);

        _device = device;
        InitArgs(device.Id.AsString());

        if (_config is not null)
        {
            ApplyConfig(_config);
        }
    }

    public bool TryAddItem(string itemId)
    {
        if (
            _device is null
            || string.IsNullOrWhiteSpace(itemId)
            || Items.Any(i => i.ItemId == itemId)
        )
        {
            return false;
        }

        if (!_factories.TryGetValue(itemId, out var factory))
        {
            Logger.LogWarning("No telemetry item factory registered for id '{ItemId}'", itemId);
            return false;
        }

        try
        {
            var createdItem = factory.TryCreate(_device);

            if (createdItem is null)
            {
                Logger.LogDebug(
                    "Skipping telemetry item '{ItemId}': not supported by device {DeviceId}",
                    itemId,
                    _device.Id
                );
                return false;
            }

            Items.Add(createdItem);
            return true;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to create telemetry item '{ItemId}'", itemId);
            return false;
        }
    }

    public bool TryRemoveItem(string itemId)
    {
        var existing = Items.FirstOrDefault(i => i.ItemId == itemId);
        if (existing is not null)
        {
            Items.Remove(existing);
            return true;
        }

        return false;
    }

    public bool TrySetItems(IReadOnlyList<string> itemIds)
    {
        if (_device is null)
        {
            return false;
        }

        var oldItemIds = Items.Select(i => i.ItemId).ToArray();
        if (oldItemIds.SequenceEqual(itemIds))
        {
            return false;
        }

        Items.RemoveAll();

        foreach (var itemId in itemIds)
        {
            TryAddItem(itemId);
        }

        return !oldItemIds.SequenceEqual(Items.Select(i => i.ItemId));
    }

    public bool TryResetItems()
    {
        return TrySetItems(DefaultItemIds);
    }

    private void ApplyConfig(TelemetrySectionConfig config)
    {
        if (_device is null)
        {
            return;
        }

        TrySetItems(config.ItemIds ?? DefaultItemIds);
    }

    private void RefreshDisplayItems()
    {
        var allAdded =
            _device is not null
            && Items.Count >= _factories.Values.Count(f => f.CanCreate(_device));

        _displayItems.RemoveAll();
        _displayItems.AddRange(_displayComposer.Compose(Items, IsEditMode, allAdded));
    }

    private async ValueTask ShowConfigureDialogAsync(CancellationToken ct)
    {
        if (_device is null)
        {
            return;
        }

        var alreadyAdded = Items.Select(i => i.ItemId).ToHashSet();
        var availableFactories = _factories
            .Values.Where(f => f.CanCreate(_device) && !alreadyAdded.Contains(f.ItemId))
            .ToArray();

        using var vm = new ConfigureTelemetryDialogViewModel(availableFactories, _loggerFactory);

        var dialog = new ContentDialog(vm, _navigationService)
        {
            Title = RS.TelemetrySectionViewModel_ConfigureTelemetryDialog_Title,
            CloseButtonText = RS.TelemetrySectionViewModel_ConfigureTelemetryDialog_CloseButtonText,
        };
        vm.ApplyDialog(dialog);

        await dialog.ShowAsync();

        if (!string.IsNullOrWhiteSpace(vm.SelectedItemId))
        {
            await this.ExecuteCommand(
                AddTelemetryItemCommand.Id,
                CommandArg.CreateString(vm.SelectedItemId),
                ct
            );
        }
    }

    private async ValueTask RemoveTelemetryItemAsync(string itemId, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(itemId))
        {
            return;
        }

        if (_device is null)
        {
            TryRemoveItem(itemId);
            return;
        }

        await this.ExecuteCommand(
            RemoveTelemetryItemCommand.Id,
            CommandArg.CreateString(itemId),
            ct
        );
    }

    private async ValueTask InternalCatchEvent(IRoutable src, AsyncRoutedEvent<IRoutable> e)
    {
        switch (e)
        {
            case TelemetryDisplayItemAddRequestedEvent addRequested:
                await ShowConfigureDialogAsync(addRequested.Cancel);
                e.IsHandled = true;
                break;
            case TelemetryDisplayItemRemoveRequestedEvent removeRequested:
                await RemoveTelemetryItemAsync(removeRequested.ItemId, removeRequested.Cancel);
                e.IsHandled = true;
                break;
            case SaveLayoutEvent saveLayoutEvent:
                _config ??= new TelemetrySectionConfig();
                this.HandleSaveLayout(
                    saveLayoutEvent,
                    _config,
                    cfg =>
                    {
                        cfg.ItemIds = Items.Select(i => i.ItemId).ToArray();
                    },
                    FlushingStrategy.FlushBothViewModelAndView
                );
                break;
            case LoadLayoutEvent loadLayoutEvent:
                _config = this.HandleLoadLayout<TelemetrySectionConfig>(
                    loadLayoutEvent,
                    ApplyConfig
                );
                break;
        }
    }

    public override IEnumerable<IRoutable> GetChildren()
    {
        foreach (var item in Items)
        {
            yield return item;
        }

        foreach (var item in _displayItems)
        {
            yield return item;
        }
    }
}
