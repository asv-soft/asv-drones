using Asv.Avalonia;
using Asv.Common;
using Asv.Drones.Api;
using ObservableCollections;
using R3;

namespace Asv.Drones;

public sealed class ConfigureTelemetryDialogViewModel : DialogViewModelBase
{
    public const string DialogId = $"{BaseId}.configure-telemetry";

    private readonly ObservableList<ConfigureTelemetryItemEntry> _entries;
    private readonly ReactiveProperty<int> _selectedCount;
    private readonly SerialDisposable _dialogSub;

    public ConfigureTelemetryDialogViewModel()
        : this([
            new BatteryTelemetryItemFactory(
                DeviceTelemetryDesignPreview.UnitService,
                DesignTime.LoggerFactory
            ),
            new AltitudeTelemetryItemFactory(DeviceTelemetryDesignPreview.UnitService),
            new VelocityTelemetryItemFactory(DeviceTelemetryDesignPreview.UnitService),
            new CurrentFlightModeTelemetryItemFactory(
                DeviceTelemetryDesignPreview.UnitService,
                DesignTime.LoggerFactory
            ),
            new AzimuthTelemetryItemFactory(
                DeviceTelemetryDesignPreview.UnitService,
                DesignTime.LoggerFactory
            ),
            new LinkQualityTelemetryItemFactory(
                DeviceTelemetryDesignPreview.UnitService,
                DesignTime.LoggerFactory
            ),
            new GnssTelemetryItemFactory(DesignTime.LoggerFactory),
            new MissionDistanceTelemetryItemFactory(
                DeviceTelemetryDesignPreview.UnitService,
                DesignTime.LoggerFactory
            ),
            new MissionProgressTelemetryItemFactory(
                DeviceTelemetryDesignPreview.UnitService,
                DesignTime.LoggerFactory
            ),
            new HomeDistanceTelemetryItemFactory(
                DeviceTelemetryDesignPreview.UnitService,
                DesignTime.LoggerFactory
            ),
            new MissionTargetTelemetryItemFactory(
                DeviceTelemetryDesignPreview.UnitService,
                DesignTime.LoggerFactory
            ),
        ])
    {
        DesignTime.ThrowIfNotDesignMode();
    }

    public ConfigureTelemetryDialogViewModel(IEnumerable<ITelemetryItemFactory> factories)
        : base(DialogId)
    {
        _entries = [];
        _selectedCount = new ReactiveProperty<int>(0).DisposeItWith(Disposable);
        _dialogSub = new SerialDisposable().DisposeItWith(Disposable);

        foreach (var factory in factories)
        {
            var item = factory.CreatePreview().SetRoutableParent(this).DisposeItWith(Disposable);

            var isSelected = new BindableReactiveProperty<bool>(false).DisposeItWith(Disposable);
            isSelected
                .Subscribe(_ =>
                {
                    _selectedCount.Value = _entries.Count(e => e.IsSelected.Value);
                })
                .DisposeItWith(Disposable);

            _entries.Add(
                new ConfigureTelemetryItemEntry
                {
                    ItemId = factory.ItemId,
                    Item = item,
                    IsSelected = isSelected,
                }
            );
        }

        Entries = _entries.ToNotifyCollectionChangedSlim().DisposeItWith(Disposable);
    }

    public INotifyCollectionChangedSynchronizedViewList<ConfigureTelemetryItemEntry> Entries { get; }

    public IReadOnlyList<string> SelectedItemIds =>
        _entries.Where(e => e.IsSelected.Value).Select(e => e.ItemId).ToArray();

    public override void ApplyDialog(ContentDialog dialog)
    {
        dialog.DefaultButton = ContentDialogButton.Primary;

        _dialogSub.Disposable = _selectedCount.Subscribe(count =>
        {
            dialog.IsPrimaryButtonEnabled = count > 0;
            dialog.PrimaryButtonText = string.Format(
                RS.TelemetrySectionViewModel_ConfigureTelemetryDialog_PrimaryButtonText,
                count
            );
        });
    }

    public override IEnumerable<IViewModel> GetChildren()
    {
        foreach (var entry in _entries)
        {
            yield return entry.Item;
        }
    }
}

public sealed class ConfigureTelemetryItemEntry
{
    public required string ItemId { get; init; }
    public required IRttBoxViewModel Item { get; init; }
    public required BindableReactiveProperty<bool> IsSelected { get; init; }
}
