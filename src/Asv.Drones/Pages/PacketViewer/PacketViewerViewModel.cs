using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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

[ExportPage(PageId)]
public class PacketViewerViewModel : PageViewModel<PacketViewerViewModel>
{
    public const string PageId = "packet-viewer";
    public const MaterialIconKind PageIcon = MaterialIconKind.Package;
    private const int MaxPacketsAmount = 1000;
    private const int PacketsReceiveDelayInSeconds = 1;

    private readonly IAppPath _app;
    private readonly ILoggerFactory _loggerFactory;
    private readonly IUnitService _unit;
    private readonly IDeviceManager _deviceManager;
    private readonly INavigationService _navigationService;
    private readonly IEnumerable<IPacketConverter> _converters;
    private readonly ObservableFixedSizeRingBuffer<PacketMessageViewModel> _packetsBuffer;
    private readonly ObservableHashSet<SourcePacketFilterViewModel> _filtersBySourceSet;
    private readonly ObservableHashSet<TypePacketFilterViewModel> _filtersByTypeSet;
    private readonly ReactiveProperty<bool> _filterChangeTrigger;

    public ReactiveCommand ClearAll { get; }
    public ReactiveCommand ExportToCsv { get; }
    public BindableReactiveProperty<bool> IsPaused { get; }
    public BindableReactiveProperty<string> SearchText { get; }
    public BindableReactiveProperty<bool> IsCheckedAllSources { get; }
    public BindableReactiveProperty<bool> IsCheckedAllTypes { get; }
    public BindableReactiveProperty<PacketMessageViewModel?> SelectedPacket { get; }
    public INotifyCollectionChangedSynchronizedViewList<PacketMessageViewModel> Packets { get; }
    public INotifyCollectionChangedSynchronizedViewList<SourcePacketFilterViewModel> FiltersBySource { get; }
    public INotifyCollectionChangedSynchronizedViewList<TypePacketFilterViewModel> FiltersByType { get; }

    public PacketViewerViewModel()
        : this(
            DesignTime.CommandService,
            NullAppPath.Instance,
            NullLoggerFactory.Instance,
            NullUnitService.Instance,
            [],
            NullDeviceManager.Instance,
            DesignTime.Navigation
        )
    {
        DesignTime.ThrowIfNotDesignMode();
        _packetsBuffer.AddLastRange(
            new[]
            {
                new PacketMessageViewModel(),
                new PacketMessageViewModel(),
                new PacketMessageViewModel(),
            }
        );
    }

    [ImportingConstructor]
    public PacketViewerViewModel(
        ICommandService cmd,
        IAppPath app,
        ILoggerFactory loggerFactory,
        IUnitService unit,
        [ImportMany] IEnumerable<IPacketConverter> converters,
        IDeviceManager deviceManager,
        INavigationService navigationService
    )
        : base(PageId, cmd, loggerFactory)
    {
        Title = "Packet Viewer";
        _app = app;
        _loggerFactory = loggerFactory;
        _unit = unit;
        _converters = converters;
        _deviceManager = deviceManager;
        _navigationService = navigationService;
        _disposables = new CompositeDisposable();
        _filterChangeTrigger = new ReactiveProperty<bool>(false).DisposeItWith(Disposable);

        _packetsBuffer = new ObservableFixedSizeRingBuffer<PacketMessageViewModel>(
            MaxPacketsAmount
        );
        _filtersBySourceSet = new ObservableHashSet<SourcePacketFilterViewModel>(
            SourcePacketFilterComparer.Instance
        );
        _filtersByTypeSet = new ObservableHashSet<TypePacketFilterViewModel>(
            TypePacketFilterComparer.Instance
        );
        _packetsBuffer.SetRoutableParent(this).DisposeItWith(Disposable);
        _filtersBySourceSet.SetRoutableParent(this).DisposeItWith(Disposable);
        _filtersByTypeSet.SetRoutableParent(this).DisposeItWith(Disposable);
        _packetsBuffer.DisposeRemovedItems().DisposeItWith(Disposable);
        _filtersBySourceSet.DisposeRemovedItems().DisposeItWith(Disposable);
        _filtersByTypeSet.DisposeRemovedItems().DisposeItWith(Disposable);

        var packetsView = _packetsBuffer.CreateView(x => x).DisposeItWith(Disposable);
        Packets = packetsView.ToNotifyCollectionChanged().DisposeItWith(Disposable);
        FiltersBySource = _filtersBySourceSet.ToNotifyCollectionChanged().DisposeItWith(Disposable);
        FiltersByType = _filtersByTypeSet.ToNotifyCollectionChanged().DisposeItWith(Disposable);

        IsPaused = new BindableReactiveProperty<bool>().DisposeItWith(Disposable);
        SearchText = new BindableReactiveProperty<string>(string.Empty).DisposeItWith(Disposable);
        IsCheckedAllSources = new BindableReactiveProperty<bool>(true).DisposeItWith(Disposable);
        IsCheckedAllTypes = new BindableReactiveProperty<bool>(true).DisposeItWith(Disposable);
        SelectedPacket = new BindableReactiveProperty<PacketMessageViewModel?>(null).DisposeItWith(
            Disposable
        );

        ExportToCsv = new ReactiveCommand(ExportToCsvImpl).DisposeItWith(Disposable);
        ClearAll = new ReactiveCommand(ClearAllImpl).DisposeItWith(Disposable);

        IsPaused.Subscribe(_ => SelectedPacket.Value = null);
        IsCheckedAllSources
            .Subscribe(isChecked =>
            {
                foreach (var packet in _filtersBySourceSet)
                {
                    packet.IsChecked.Value = isChecked;
                }
            })
            .DisposeItWith(Disposable);
        IsCheckedAllTypes
            .Subscribe(isChecked =>
            {
                foreach (var packet in _filtersByTypeSet)
                {
                    packet.IsChecked.Value = isChecked;
                }
            })
            .DisposeItWith(Disposable);

        _packetsBuffer
            .ObserveAdd()
            .Subscribe(item => UpdateFilters(item.Value))
            .DisposeItWith(Disposable);
        _deviceManager
            .Router.OnRxMessage.Where(_ => !IsPaused.Value)
            .FilterByType<MavlinkMessage>()
            .Chunk(TimeSpan.FromSeconds(PacketsReceiveDelayInSeconds))
            .Select(ConvertToPacketMessage)
            .Subscribe(_packetsBuffer.AddLastRange)
            .DisposeItWith(Disposable);
        _deviceManager
            .Router.OnTxMessage.Where(_ => !IsPaused.Value)
            .FilterByType<MavlinkMessage>()
            .Chunk(TimeSpan.FromSeconds(PacketsReceiveDelayInSeconds))
            .Select(ConvertToPacketMessage)
            .Subscribe(_packetsBuffer.AddLastRange)
            .DisposeItWith(Disposable);

        SelectedPacket
            .WhereNotNull()
            .Subscribe(selectedPacket =>
            {
                foreach (var item in _packetsBuffer)
                {
                    item.Highlight = item.Type == selectedPacket.Type;
                }
            })
            .DisposeItWith(Disposable);

        var viewFilter = CreateSynchronizedViewFilter();

        _filtersBySourceSet
            .ObserveAdd()
            .Subscribe(filter =>
            {
                var sub = filter.Value.IsChecked.Subscribe(_ =>
                    _filterChangeTrigger.Value = !_filterChangeTrigger.Value
                );

                _disposables.Add(sub);
            })
            .DisposeItWith(Disposable);
        _filtersByTypeSet
            .ObserveAdd()
            .Subscribe(filter =>
            {
                var sub = filter.Value.IsChecked.Subscribe(_ =>
                    _filterChangeTrigger.Value = !_filterChangeTrigger.Value
                );

                _disposables.Add(sub);
            })
            .DisposeItWith(Disposable);

        Observable
            .Merge(
                _filtersBySourceSet.ObserveChanged().Select(_ => Unit.Default),
                _filtersByTypeSet.ObserveChanged().Select(_ => Unit.Default),
                SearchText.Select(_ => Unit.Default),
                _filterChangeTrigger.Select(_ => Unit.Default)
            )
            .ThrottleLast(TimeSpan.FromMilliseconds(500))
            .Subscribe(_ =>
            {
                var allSourcesSelected = _filtersBySourceSet.All(x => x.IsChecked.Value);
                var allTypesSelected = _filtersByTypeSet.All(x => x.IsChecked.Value);
                var hasNoSearchString = string.IsNullOrEmpty(SearchText.Value);

                if (hasNoSearchString && allSourcesSelected && allTypesSelected)
                {
                    packetsView.ResetFilter();
                    return;
                }

                packetsView.AttachFilter(viewFilter);
            })
            .DisposeItWith(Disposable);
    }

    private ISynchronizedViewFilter<
        PacketMessageViewModel,
        PacketMessageViewModel
    > CreateSynchronizedViewFilter()
    {
        return new SynchronizedViewFilter<PacketMessageViewModel, PacketMessageViewModel>(
            (_, packet) =>
            {
                var hasRequiredType = _filtersByTypeSet.Any(f =>
                    f.IsChecked.Value && f.FilterValue.Value == packet.Type
                );

                var hasRequiredSource = _filtersBySourceSet.Any(f =>
                    f.IsChecked.Value && f.FilterValue.Value == packet.Source
                );

                if (!hasRequiredSource)
                {
                    return false;
                }

                if (!hasRequiredType)
                {
                    return false;
                }

                if (_filtersBySourceSet.All(x => !x.IsChecked.Value))
                {
                    return false;
                }

                if (_filtersByTypeSet.All(x => !x.IsChecked.Value))
                {
                    return false;
                }

                if (string.IsNullOrWhiteSpace(SearchText.Value))
                {
                    return true;
                }

                return packet.Message.Contains(
                    SearchText.Value,
                    StringComparison.OrdinalIgnoreCase
                );
            }
        );
    }

    private async ValueTask ExportToCsvImpl(Unit unit, CancellationToken cancellationToken)
    {
        using var vm = new SavePacketMessagesDialogViewModel(_loggerFactory);
        var dialog = new ContentDialog(vm, _navigationService)
        {
            Title = $"{RS.PacketViewerViewModel_SavePacketMessagesDialog_Title}",
            CloseButtonText =
                $"{RS.PacketViewerViewModel_SavePacketMessagesDialog_CloseButtonText}",
            PrimaryButtonText =
                $"{RS.PacketViewerViewModel_SavePacketMessagesDialog_PrimaryButtonText}",
            DefaultButton = ContentDialogButton.Primary,
        };

        var res = await dialog.ShowAsync();

        if (res != ContentDialogResult.Primary)
        {
            return;
        }

        var filePath = Path.Join(
            _app.UserDataFolder,
            $"packets{DateTime.Now:yyyy-M-d h-mm-ss}.csv"
        );

        try
        {
            CsvHelper.SaveToCsv(
                _packetsBuffer.ToImmutableList(),
                filePath,
                vm.Separator,
                vm.ShieldSymbol,
                new CsvColumn<PacketMessageViewModel>(
                    $"{RS.PacketMessageViewModel_CsvColumn_Date}",
                    x => x.DateTime.ToString("G")
                ),
                new CsvColumn<PacketMessageViewModel>(
                    $"{RS.PacketMessageViewModel_CsvColumn_Type}",
                    x => x.Type
                ),
                new CsvColumn<PacketMessageViewModel>(
                    $"{RS.PacketMessageViewModel_CsvColumn_Source}",
                    x => x.Source
                ),
                new CsvColumn<PacketMessageViewModel>(
                    $"{RS.PacketMessageViewModel_CsvColumn_Message}",
                    x => x.Message
                )
            );

            Logger.LogInformation("Export file saved to: {filePath}", filePath);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Аn error occurred while saving the file: {filePath}", filePath);
        }
    }

    private ValueTask ClearAllImpl(Unit arg, CancellationToken cancellationToken)
    {
        _packetsBuffer.RemoveAll(); // TODO: Find out why it doesn't work when void
        return ValueTask.CompletedTask;
    }

    private IEnumerable<PacketMessageViewModel> ConvertToPacketMessage(
        IEnumerable<MavlinkMessage> messages
    )
    {
        foreach (var packet in messages)
        {
            var converter =
                _converters.FirstOrDefault(_ => _.CanConvert(packet))
                ?? new DefaultMavlinkPacketConverter();
            var vm = new PacketMessageViewModel(packet, converter);
            _disposables.Add(vm);
            yield return vm;
        }
    }

    private void UpdateFilters(PacketMessageViewModel vm)
    {
        UpdateSourceFilters(vm);
        UpdateTypeFilters(vm);
    }

    private void UpdateSourceFilters(PacketMessageViewModel vm)
    {
        var filter = _filtersBySourceSet.FirstOrDefault(x => x.FilterValue.Value == vm.Source);
        if (filter is not null)
        {
            filter.IncreaseRatesCounterSafe();
            return;
        }

        var newFilter = new SourcePacketFilterViewModel(vm, _unit, _loggerFactory);
        _disposables.Add(newFilter);
        var isAdded = _filtersBySourceSet.Add(newFilter);

        if (!isAdded)
        {
            newFilter.Dispose();
        }

        Logger.LogInformation("Added new source filter: {Source}", vm.Source);
    }

    private void UpdateTypeFilters(PacketMessageViewModel vm)
    {
        var filter = _filtersByTypeSet.FirstOrDefault(x => x.FilterValue.Value == vm.Type);
        if (filter is not null)
        {
            filter.IncreaseRatesCounterSafe();
            return;
        }

        var newFilter = new TypePacketFilterViewModel(vm, _unit, _loggerFactory);
        _disposables.Add(newFilter);
        var isAdded = _filtersByTypeSet.Add(newFilter);

        if (!isAdded)
        {
            newFilter.Dispose();
        }

        Logger.LogInformation("Added new type filter: {Type}", vm.Type);
    }

    public override IEnumerable<IRoutable> GetRoutableChildren()
    {
        foreach (var item in _packetsBuffer)
        {
            yield return item;
        }

        foreach (var item in _filtersBySourceSet)
        {
            yield return item;
        }

        foreach (var item in _filtersByTypeSet)
        {
            yield return item;
        }
    }

    protected override void AfterLoadExtensions() { }

    #region Dispose

    private readonly CompositeDisposable _disposables;

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _packetsBuffer.RemoveAll();
            _filtersBySourceSet.ClearWithItemsDispose();
            _filtersByTypeSet.ClearWithItemsDispose();
            _disposables.Dispose();
        }

        base.Dispose(disposing);
    }

    #endregion

    public override IExportInfo Source => SystemModule.Instance;
}
