using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Windows.Input;
using Asv.Common;
using Asv.Drones.Gui.Uav;
using Asv.Mavlink;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using DynamicData;
using DynamicData.Binding;

namespace Asv.Drones.Gui.Core;

[ExportShellPage(UriString)]
[PartCreationPolicy(CreationPolicy.NonShared)]
public class PacketViewerViewModel : ViewModelBase, IShellPage
{
    private readonly ILocalizationService _localization;
    private readonly IGlobalCommandsService _cmd;
    public const string UriString = ShellPage.UriString + ".packetViewer";
    public static readonly Uri Uri = new Uri(UriString);
    public const int MaxPacketSize = 1000; 

    private readonly Subject<Func<PacketMessageViewModel, bool>> _sourceFilterUpdate = new ();
    private readonly Subject<Func<PacketMessageViewModel, bool>> _typeFilterUpdate = new ();
    private readonly Subject<Func<PacketMessageViewModel, bool>> _searchUpdate = new ();

    private readonly SourceCache<PacketFilterViewModel,string> _filtersSource;
    private readonly SourceCache<PacketFilterViewModel,string> _filtersSourceType;
    private readonly ReadOnlyObservableCollection<PacketFilterViewModel> _filtersBySource;
    private readonly ReadOnlyObservableCollection<PacketFilterViewModel> _filtersByType;
    
    private readonly SourceList<PacketMessageViewModel> _packetsSource;
    private readonly ReadOnlyObservableCollection<PacketMessageViewModel> _packets;

    public PacketViewerViewModel() : base(Uri)
    {
        //TODO: Implement export to CSV
        //ExportToCsv = ReactiveCommand.Create(Export);
        ClearAll = ReactiveCommand.Create(() => _packetsSource.Clear());
        if (Design.IsDesignMode)
        {
            _packets = new ReadOnlyObservableCollection<PacketMessageViewModel>(
                new ObservableCollection<PacketMessageViewModel>(new[]
                {
                    new PacketMessageViewModel{ DateTime = DateTime.Now, Source = "[1,1]", Type = "HEARTBEAT", Message = "asdasdasdasdas"},
                    new PacketMessageViewModel{ DateTime = DateTime.Now, Source = "[1,1]", Type = "HEARTBEAT", Message = "asdasdasdasdas"},
                    new PacketMessageViewModel{ DateTime = DateTime.Now, Source = "[1,1]", Type = "HEARTBEAT", Message = "asdasdasdasdas"},
                }));
        }
    }
    
    [ImportingConstructor]
    public PacketViewerViewModel(IMavlinkDevicesService mavlinkDevicesService, ILocalizationService localizationService, IGlobalCommandsService cmd) : this()
    {
        _localization = localizationService;
        _cmd = cmd;

        _packetsSource = new SourceList<PacketMessageViewModel>();
        _packetsSource.LimitSizeTo(MaxPacketSize).Subscribe().DisposeItWith(Disposable);
        _filtersSource = new SourceCache<PacketFilterViewModel, string>(_ => _.Source);
        _filtersSourceType = new SourceCache<PacketFilterViewModel, string>(_ => _.Type);
        
        mavlinkDevicesService.Router
            .Where(_=>IsPause == false)
            .Buffer(TimeSpan.FromSeconds(1)) // fix slow rendering when high rate of packets: buffer it 1 sec and then render
            .ObserveOn(RxApp.MainThreadScheduler)
            .Select(ConvertPacketToMessagesAndUpdateFilters)
            .Subscribe(_packetsSource.AddRange)
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
            .Bind(out _filtersBySource, useReplaceForUpdates:true )
            .Subscribe()
            .DisposeItWith(Disposable);

        _filtersSource
            .Connect()
            .WhenValueChanged(_ => _.IsChecked)
            .Subscribe(_ => _sourceFilterUpdate.OnNext(FilterBySourcePredicate))
            .DisposeItWith(Disposable);

        _filtersSourceType
            .Connect()
            .Bind(out _filtersByType, useReplaceForUpdates:true)
            .Subscribe()
            .DisposeItWith(Disposable);

        _filtersSourceType
            .Connect()
            .WhenValueChanged(_ => _.IsChecked)
            .Subscribe(_ => _typeFilterUpdate.OnNext(FilterByTypePredicate))
            .DisposeItWith(Disposable);

        #endregion

        Observable.Timer(TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1)).Subscribe(_ =>
        {
            _filtersSource.Items.ForEach(_=>_.UpdateRateText());
            _filtersSourceType.Items.ForEach(_=>_.UpdateRateText());
            
        }).DisposeItWith(Disposable);

        this.WhenValueChanged(_ => _.SelectedPacket)
            .ObserveOn(RxApp.MainThreadScheduler)
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
            var obj = new PacketMessageViewModel(packet);
            result.Add(obj);
            var sourceExists = _filtersSource.Lookup(obj.Source);
            if (sourceExists.HasValue)
            {
                sourceExists.Value.UpdateRates();
            }
            else
            {
                _filtersSource.AddOrUpdate(new PacketFilterViewModel(obj,_localization));
            }
        
            var typeExists = _filtersSourceType.Lookup(obj.Type);
            if (typeExists.HasValue)
            {
                typeExists.Value.UpdateRates();
            }
            else
            {
                _filtersSourceType.AddOrUpdate(new PacketFilterViewModel(obj,_localization));
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
        return _filtersBySource.Where(_=>_.IsChecked).Any(_=>_.Source == vm.Source);
    }
    
    private bool FilterByTypePredicate(PacketMessageViewModel vm)
    {
        return _filtersByType.Where(_=>_.IsChecked).Any(_=>_.Type == vm.Type);
    }

    private bool SearchPredicate(PacketMessageViewModel vm)
    {
        if (string.IsNullOrEmpty(SearchText))
        {
            return true;
        }

        return vm.Message.Contains(SearchText, StringComparison.OrdinalIgnoreCase);
    }

    public async void Export()
    {
        var file = await new Window().StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions(){Title = "Save to CSV..."});
        
        try
        {
            CsvHelper.SaveToCsv(_packets, "", ";", ",",
                new CsvColumn<PacketMessageViewModel>("Date", _ => _.DateTime.ToString("G")),
                new CsvColumn<PacketMessageViewModel>("Type", _ => _.Type),
                new CsvColumn<PacketMessageViewModel>("Source", _ => _.Source),
                new CsvColumn<PacketMessageViewModel>("Message", _ => _.Message));
        }
        catch(Exception ex)
        {
            throw;
        }
    }

    public void SetArgs(Uri link)
    {
    }
}