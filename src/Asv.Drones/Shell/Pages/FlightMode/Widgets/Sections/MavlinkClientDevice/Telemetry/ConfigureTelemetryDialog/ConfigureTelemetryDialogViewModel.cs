using System.Collections.Generic;
using System.Windows.Input;
using Asv.Avalonia;
using Asv.Common;
using Asv.Drones.Api;
using Microsoft.Extensions.Logging;
using ObservableCollections;
using R3;

namespace Asv.Drones;

public sealed class ConfigureTelemetryItemEntry
{
    public required string ItemId { get; init; }
    public required string DisplayName { get; init; }
    public required ITelemetryItem Item { get; init; }
    public required ICommand Select { get; init; }
}

public sealed class ConfigureTelemetryDialogViewModel : DialogViewModelBase
{
    public const string DialogId = $"{BaseId}.configureTelemetry";

    private readonly ObservableList<ConfigureTelemetryItemEntry> _entries;
    private ContentDialog? _dialog;

    public ConfigureTelemetryDialogViewModel()
        : this(
            [
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
            ],
            DesignTime.LoggerFactory
        )
    {
        DesignTime.ThrowIfNotDesignMode();
    }

    public ConfigureTelemetryDialogViewModel(
        IEnumerable<ITelemetryItemFactory> factories,
        ILoggerFactory loggerFactory
    )
        : base(DialogId, loggerFactory)
    {
        _entries = [];

        foreach (var factory in factories)
        {
            var item = factory.CreatePreview().SetRoutableParent(this).DisposeItWith(Disposable);

            var selectCommand = new ReactiveCommand(_ =>
            {
                SelectedItemId = factory.ItemId;
                _dialog?.Hide(ContentDialogResult.Primary);
            }).DisposeItWith(Disposable);

            _entries.Add(
                new ConfigureTelemetryItemEntry
                {
                    ItemId = factory.ItemId,
                    DisplayName = factory.DisplayName,
                    Item = item,
                    Select = selectCommand,
                }
            );
        }

        Entries = _entries.ToNotifyCollectionChangedSlim().DisposeItWith(Disposable);
    }

    public INotifyCollectionChangedSynchronizedViewList<ConfigureTelemetryItemEntry> Entries { get; }

    public string? SelectedItemId { get; private set; }

    public override void ApplyDialog(ContentDialog dialog)
    {
        _dialog = dialog;
    }

    public override IEnumerable<IRoutable> GetChildren()
    {
        foreach (var entry in _entries)
        {
            yield return entry.Item;
        }
    }
}
