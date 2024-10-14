using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Composition;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using System.Windows.Input;
using Asv.Cfg;
using Asv.Common;
using Asv.Drones.Gui.Api;
using Asv.Mavlink;
using DynamicData;
using DynamicData.Binding;
using FluentAvalonia.UI.Controls;
using Material.Icons;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Asv.Drones.Gui;

[ExportShellPage(WellKnownUri.ShellPagePacketViewer)]
public class PacketViewerViewModel : ShellPage
{
    private readonly IApplicationHost _app;
    private readonly IConfiguration _cfg;
    private readonly ILocalizationService _localization;
    private readonly ILogService _log;
    public const int MaxPacketSize = 1000;
    
    
    private readonly Subject<Func<PacketMessageViewModel, bool>> _sourceFilterUpdate = new();
    private readonly Subject<Func<PacketMessageViewModel, bool>> _typeFilterUpdate = new();
    private readonly Subject<Func<PacketMessageViewModel, bool>> _searchUpdate = new();

    private readonly SourceCache<PacketFilterViewModel, string> _filtersSource;
    private readonly SourceCache<PacketFilterViewModel, string> _filtersSourceType;
    private readonly ReadOnlyObservableCollection<PacketFilterViewModel> _filtersBySource;
    private readonly ReadOnlyObservableCollection<PacketFilterViewModel> _filtersByType;

    private readonly SourceList<PacketMessageViewModel> _packetsSource;
    private readonly ReadOnlyObservableCollection<PacketMessageViewModel> _packets;

    public PacketViewerViewModel() : base(WellKnownUri.UndefinedUri)
    {
        DesignTime.ThrowIfNotDesignMode();
        Title = RS.PacketViewerShellMenuItem_Name;
        Icon = MaterialIconKind.Package;

        _packets = new ReadOnlyObservableCollection<PacketMessageViewModel>(
            new ObservableCollection<PacketMessageViewModel>(new[]
            {
                new PacketMessageViewModel
                    { DateTime = DateTime.Now, Source = "[1,1]", Type = "HEARTBEAT", Message = "asdasdasdasdas" },
                new PacketMessageViewModel
                    { DateTime = DateTime.Now, Source = "[1,1]", Type = "HEARTBEAT", Message = "asdasdasdasdas" },
                new PacketMessageViewModel
                    { DateTime = DateTime.Now, Source = "[1,1]", Type = "HEARTBEAT", Message = "asdasdasdasdas" },
            }));
    }
    private PacketPrinter _printer;
    [ImportingConstructor]
    public PacketViewerViewModel(IMavlinkDevicesService mavlinkDevicesService, IApplicationHost app,
        IConfiguration cfg, ILocalizationService localizationService, ILogService log,[ImportMany] IEnumerable<IPacketPrinterHandler> handlers)
        : base(WellKnownUri.ShellPagePacketViewer)
    {
        _localization = localizationService;
        _log = log;
        _cfg = cfg;
        _app = app;
        Title = RS.PacketViewerShellMenuItem_Name;
        Icon = MaterialIconKind.Package;
        ExportToCsv = ReactiveCommand.CreateFromTask(Export, this.WhenValueChanged(_ => _.IsPause));
        ClearAll = ReactiveCommand.Create(() => _packetsSource.Clear());
        _printer = new PacketPrinter(handlers);
        
        _packetsSource = new SourceList<PacketMessageViewModel>();
        _packetsSource.LimitSizeTo(MaxPacketSize).Subscribe().DisposeItWith(Disposable);
        _filtersSource = new SourceCache<PacketFilterViewModel, string>(_ => _.Source);
        _filtersSourceType = new SourceCache<PacketFilterViewModel, string>(_ => _.Type);

        mavlinkDevicesService.Router
            .Where(_ => IsPause == false)
            .Buffer(TimeSpan.FromSeconds(1),
                RxApp.MainThreadScheduler) // fix slow rendering when high rate of packets: buffer it 1 sec and then render
            .Select(ConvertPacketToMessagesAndUpdateFilters).Subscribe(_packetsSource.AddRange)
            .DisposeItWith(Disposable);

        _sourceFilterUpdate.OnNext(FilterBySourcePredicate);
        _typeFilterUpdate.OnNext(FilterBySourcePredicate);
        _searchUpdate.OnNext(SearchPredicate);

        _packetsSource
            .Connect()
            .Filter(_sourceFilterUpdate)
            .Filter(_typeFilterUpdate)
            .Filter(_searchUpdate)
            .Bind(out _packets)
            .Subscribe()
            .DisposeItWith(Disposable);

        this.WhenAnyValue(_ => _.SearchText)
            .Subscribe(_ => _searchUpdate.OnNext(SearchPredicate))
            .DisposeItWith(Disposable);

        #region Filters

        _filtersSource
            .Connect()
            //.Sort(SortExpressionComparer<PacketFilterViewModel>.Descending(_=>_.Source), SortOptimisations.ComparesImmutableValuesOnly)
            .Bind(out _filtersBySource, useReplaceForUpdates: true)
            .Subscribe()
            .DisposeItWith(Disposable);

        _filtersSource
            .Connect()
            .WhenValueChanged(_ => _.IsChecked)
            .Subscribe(_ => _sourceFilterUpdate.OnNext(FilterBySourcePredicate))
            .DisposeItWith(Disposable);

        _filtersSourceType
            .Connect()
            .Bind(out _filtersByType, useReplaceForUpdates: true)
            .Subscribe()
            .DisposeItWith(Disposable);

        _filtersSourceType
            .Connect()
            .WhenValueChanged(_ => _.IsChecked)
            .Subscribe(_ => _typeFilterUpdate.OnNext(FilterByTypePredicate))
            .DisposeItWith(Disposable);

        #endregion

        Observable.Timer(TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1), RxApp.MainThreadScheduler).Subscribe(_ =>
        {
            _filtersSource.Items.ForEach(_ => _.UpdateRateText());
            _filtersSourceType.Items.ForEach(_ => _.UpdateRateText());
        }).DisposeItWith(Disposable);

        this.WhenValueChanged(_ => _.SelectedPacket)
            .Subscribe(_ =>
            {
                if (_ != null)
                {
                    foreach (var packet in _packets)
                    {
                        packet.Highlight = false;

                        if (packet.Type == _.Type)
                        {
                            packet.Highlight = true;
                        }
                    }
                }
            })
            .DisposeItWith(Disposable);
    }
    
    private IList<PacketMessageViewModel> ConvertPacketToMessagesAndUpdateFilters(IList<IPacketV2<IPayload>> items)
    {
        var result = new List<PacketMessageViewModel>(items.Count);
        foreach (var packet in items)
        {
           var message =  _printer.Print(packet);
           var description = _printer.Print(packet, PacketFormatting.Indented);
            var obj = new PacketMessageViewModel(packet, message, description);

            result.Add(obj);
            var sourceExists = _filtersSource.Lookup(obj.Source);
            if (sourceExists.HasValue)
            {
                sourceExists.Value.UpdateRates();
            }
            else
            {
                _filtersSource.AddOrUpdate(new PacketFilterViewModel(obj, _localization));
            }

            var typeExists = _filtersSourceType.Lookup(obj.Type);
            if (typeExists.HasValue)
            {
                typeExists.Value.UpdateRates();
            }
            else
            {
                _filtersSourceType.AddOrUpdate(new PacketFilterViewModel(obj, _localization));
            }
        }

        return result;
    }

    public ICommand ClearAll { get; }
    public ICommand ExportToCsv { get; }

    public ReadOnlyObservableCollection<PacketMessageViewModel> Packets => _packets;
    public ReadOnlyObservableCollection<PacketFilterViewModel> FiltersBySource => _filtersBySource;
    public ReadOnlyObservableCollection<PacketFilterViewModel> FiltersByType => _filtersByType;

    [Reactive] public bool IsPause { get; set; }
    [Reactive] public string SearchText { get; set; }
    [Reactive] public PacketMessageViewModel SelectedPacket { get; set; }

    private bool FilterBySourcePredicate(PacketMessageViewModel vm)
    {
        return _filtersBySource.Where(_ => _.IsChecked).Any(_ => _.Source == vm.Source);
    }

    private bool FilterByTypePredicate(PacketMessageViewModel vm)
    {
        return _filtersByType.Where(_ => _.IsChecked).Any(_ => _.Type == vm.Type);
    }

    private bool SearchPredicate(PacketMessageViewModel vm)
    {
        if (string.IsNullOrEmpty(SearchText))
        {
            return true;
        }

        return vm.Message.Contains(SearchText, StringComparison.OrdinalIgnoreCase);
    }

    public async Task Export()
    {
        var dialog = new ContentDialog()
        {
            Title = RS.PacketViewerViewModel_SeparatorDialog_Title,
            PrimaryButtonText = RS.PacketViewerViewModel_SeparatorDialog_DialogPrimaryButton,
            IsSecondaryButtonEnabled = true,
            SecondaryButtonText = RS.PacketViewerViewModel_SeparatorDialog_DialogSecondaryButton
        };

        using var viewModel = new SeparatorViewModel();
        viewModel.ApplyDialog(dialog);
        dialog.Content = viewModel;

        var result = await dialog.ShowAsync();

        if (result == ContentDialogResult.Primary)
        {
            var separator = ";";
            var shieldSymbol = ",";

            if (viewModel.IsComa)
            {
                separator = ",";
                shieldSymbol = ";";
            }
            else if (viewModel.IsTab)
            {
                separator = "\t";
            }
            
            var fileName = Path.Join(_app.Paths.AppDataFolder, $"packets{DateTime.Now:yyyy-M-d h-mm-ss}.csv");

            try
            {
                CsvHelper.SaveToCsv(_packets, fileName, separator, shieldSymbol,
                    new CsvColumn<PacketMessageViewModel>("Date", _ => _.DateTime.ToString("G")),
                    new CsvColumn<PacketMessageViewModel>("Type", _ => _.Type),
                    new CsvColumn<PacketMessageViewModel>("Source", _ => _.Source),
                    new CsvColumn<PacketMessageViewModel>("Message", _ => _.Message));

                _log.Info("Export file saved to {0}", fileName);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}