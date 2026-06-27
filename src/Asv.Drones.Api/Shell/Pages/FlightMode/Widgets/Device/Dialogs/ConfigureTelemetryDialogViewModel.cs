using System.Windows.Input;
using Asv.Avalonia;
using Asv.Common;
using Material.Icons;
using R3;

namespace Asv.Drones.Api;

public sealed class ConfigureTelemetryDialogViewModel : DialogViewModelBase
{
    public const string DialogId = $"{BaseId}.configure-telemetry";

    public ConfigureTelemetryDialogViewModel()
        : this([])
    {
        DesignTime.ThrowIfNotDesignMode();
    }

    public ConfigureTelemetryDialogViewModel(IEnumerable<ITileViewModel> telemetryItems)
        : base(DialogId)
    {
        ArgumentNullException.ThrowIfNull(telemetryItems);
        Editor = new PropertyEditorViewModel($"{DialogId}.items") { ShowHeader = false }
            .SetRoutableParent(this)
            .DisposeItWith(Disposable);
        SelectAllCommand = new ReactiveCommand(_ => SetAllVisible(true)).DisposeItWith(Disposable);
        ClearSelectionCommand = new ReactiveCommand(_ => SetAllVisible(false)).DisposeItWith(
            Disposable
        );

        foreach (var tile in telemetryItems)
        {
            AddTile(tile);
        }
    }

    public PropertyEditorViewModel Editor { get; }

    public string SelectAllHeader => RS.ConfigureTelemetryDialogViewModel_SelectAll_Header;

    public string ClearSelectionHeader =>
        RS.ConfigureTelemetryDialogViewModel_ClearSelection_Header;

    public ICommand SelectAllCommand { get; }

    public ICommand ClearSelectionCommand { get; }

    public override IEnumerable<IViewModel> GetChildren()
    {
        yield return Editor;
    }

    public IReadOnlyDictionary<string, bool> GetVisibility()
    {
        return GetItems().ToDictionary(item => item.TileId, item => item.IsTelemetryVisible);
    }

    private void AddTile(ITileViewModel tile)
    {
        if (GetItems().Any(item => item.TileId == tile.Id.ToString()))
        {
            return;
        }

        var item = new ConfigureTelemetryTileMenuState(
            tile.Id.ToString(),
            string.IsNullOrWhiteSpace(tile.Header) ? tile.Id.ToString() : tile.Header,
            tile.Icon ?? MaterialIconKind.ViewDashboard,
            tile.Order,
            tile.IsVisible
        );

        Editor.ItemsSource.Add(item);
        Editor.ItemsSource.Sort(ConfigureTelemetryTileMenuStateComparer.Instance);
    }

    private void SetAllVisible(bool isVisible)
    {
        foreach (var item in GetItems())
        {
            item.SetTelemetryVisible(isVisible);
        }
    }

    private IEnumerable<ConfigureTelemetryTileMenuState> GetItems()
    {
        return Editor.ItemsSource.OfType<ConfigureTelemetryTileMenuState>();
    }
}

public sealed class ConfigureTelemetryTileMenuState : PropertyToggleSwitchViewModel
{
    private const string TileItemIdPrefix = "configure-telemetry-tile";

    public ConfigureTelemetryTileMenuState(
        string id,
        string header,
        MaterialIconKind icon,
        int order,
        bool isVisible
    )
        : base($"{TileItemIdPrefix}.{id}", false)
    {
        TileId = id;
        Header = header;
        ShortHeader = header;
        Icon = icon;
        Order = order;
        ApplyValueFromModel(isVisible);
    }

    public string TileId { get; }

    public bool IsTelemetryVisible => Value.Value;

    public void SetTelemetryVisible(bool isVisible)
    {
        ApplyValueFromModel(isVisible);
    }

    protected override ValueTask ApplyFromUser(bool value, CancellationToken cancel)
    {
        ApplyValueFromModel(value);
        return ValueTask.CompletedTask;
    }

    public override IEnumerable<IViewModel> GetChildren()
    {
        return [];
    }
}

file sealed class ConfigureTelemetryTileMenuStateComparer : IComparer<IPropertyViewModel>
{
    public static readonly ConfigureTelemetryTileMenuStateComparer Instance = new();

    public int Compare(IPropertyViewModel? x, IPropertyViewModel? y)
    {
        if (ReferenceEquals(x, y))
        {
            return 0;
        }

        if (x is null)
        {
            return -1;
        }

        if (y is null)
        {
            return 1;
        }

        if (x is not ConfigureTelemetryTileMenuState left)
        {
            return -1;
        }

        if (y is not ConfigureTelemetryTileMenuState right)
        {
            return 1;
        }

        var order = left.Order.CompareTo(right.Order);
        return order != 0 ? order : string.CompareOrdinal(left.Header, right.Header);
    }
}
