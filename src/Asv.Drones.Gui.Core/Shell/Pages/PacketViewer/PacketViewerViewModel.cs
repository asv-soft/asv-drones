using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Windows.Input;
using Asv.Common;
using Asv.Drones.Gui.Uav;
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
    public const string UriString = ShellPage.UriString + ".packetViewer";
    public static readonly Uri Uri = new Uri(UriString);
    

    private readonly Subject<Func<PacketMessageViewModel, bool>> _sourceFilterUpdate = new ();
    private readonly Subject<Func<PacketMessageViewModel, bool>> _typeFilterUpdate = new ();

    private readonly SourceCache<PacketFilterViewModel,string> _filtersSource;
    private readonly SourceCache<PacketFilterViewModel,string> _filtersSourceType;
    private readonly ReadOnlyObservableCollection<PacketFilterViewModel> _filtersBySource;
    private readonly ReadOnlyObservableCollection<PacketFilterViewModel> _filtersByType;
    
    private readonly SourceCache<PacketMessageViewModel,Guid> _packetsSource;
    private readonly ReadOnlyObservableCollection<PacketMessageViewModel> _packets;

    public PacketViewerViewModel() : base(Uri)
    {
        PlayPause = ReactiveCommand.Create(() => IsPause = !IsPause);
        //TODO: Implement delete all items from collection. Currently impossible due to collection being read-only
        //ClearAll = ReactiveCommand.Create(() => Packets.RemoveMany(Packets));
    }
    
    [ImportingConstructor]
    public PacketViewerViewModel(IMavlinkDevicesService mavlinkDevicesService, ILocalizationService localizationService) : this()
    {
        _localization = localizationService;

        _packetsSource = new SourceCache<PacketMessageViewModel, Guid>(_ => _.Id);
        _filtersSource = new SourceCache<PacketFilterViewModel, string>(_ => _.Source);
        _filtersSourceType = new SourceCache<PacketFilterViewModel, string>(_ => _.Id);
        
        mavlinkDevicesService.Router
            .Where(_=>IsPause == false)
            .ObserveOn(RxApp.MainThreadScheduler)
            .Select(_=>new PacketMessageViewModel(_))
            .Do(UpdateFilterSource)
            .Subscribe(_=>_packetsSource.AddOrUpdate(_))
            .DisposeItWith(Disposable);
        
        _sourceFilterUpdate.OnNext(FilterBySourcePredicate);
        _typeFilterUpdate.OnNext(FilterBySourcePredicate);
        
        _packetsSource
            .Connect()
            .Filter(_sourceFilterUpdate)
            .Filter(_typeFilterUpdate)
            .LimitSizeTo(1000)
            .Bind(out _packets)
            .SortBy(_=>_.DateTime, SortDirection.Descending)
            .Subscribe()
            .DisposeItWith(Disposable);

        #region Filters

        _filtersSource
            .Connect()
            .Bind(out _filtersBySource)
            .Subscribe()
            .DisposeItWith(Disposable);

        _filtersSource
            .Connect()
            .WhenValueChanged(_ => _.IsChecked)
            .Subscribe(_ => _sourceFilterUpdate.OnNext(FilterBySourcePredicate))
            .DisposeItWith(Disposable);

        _filtersSourceType
            .Connect()
            .Bind(out _filtersByType)
            .Subscribe()
            .DisposeItWith(Disposable);

        _filtersSourceType
            .Connect()
            .WhenValueChanged(_ => _.IsChecked)
            .Subscribe(_ => _typeFilterUpdate.OnNext(FilterByTypePredicate))
            .DisposeItWith(Disposable);

        #endregion
    }

    private void UpdateFilterSource(PacketMessageViewModel obj)
    {
        var sourceExists = _filtersSource.Lookup(obj.Source);
        if (sourceExists.HasValue)
        {
            sourceExists.Value.UpdateRates();
        }
        else
        {
            _filtersSource.AddOrUpdate(new PacketFilterViewModel(obj,_localization));
        }
        
        var typeExists = _filtersSourceType.Lookup(obj.FilterId);
        if (typeExists.HasValue)
        {
            typeExists.Value.UpdateRates();
        }
        else
        {
            _filtersSourceType.AddOrUpdate(new PacketFilterViewModel(obj,_localization));
        }
    }

    public ICommand PlayPause { get; }
    public ICommand ClearAll { get; }

    public ReadOnlyObservableCollection<PacketMessageViewModel> Packets => _packets;
    public ReadOnlyObservableCollection<PacketFilterViewModel> FiltersBySource => _filtersBySource;
    public ReadOnlyObservableCollection<PacketFilterViewModel> FiltersByType => _filtersByType;
    
    [Reactive] public bool IsPause { get; set; }

    private bool FilterBySourcePredicate(PacketMessageViewModel vm)
    {
        return _filtersBySource.Where(_=>_.IsChecked).Any(_=>_.Source == vm.Source);
    }
    
    private bool FilterByTypePredicate(PacketMessageViewModel vm)
    {
        return _filtersByType.Where(_=>_.IsChecked).Any(_=>_.Type == vm.Type);
    }
  

    public void SetArgs(Uri link)
    {
    }
}