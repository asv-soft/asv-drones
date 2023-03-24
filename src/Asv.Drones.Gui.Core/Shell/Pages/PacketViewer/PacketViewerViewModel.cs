using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Windows.Input;
using Asv.Common;
using Asv.Drones.Gui.Uav;
using Asv.Mavlink;
using Newtonsoft.Json;
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
    

    private readonly Subject<Func<PacketMessageViewModel, bool>> _filterUpdate = new ();

    private readonly SourceCache<PacketFilterViewModel,string> _filtersSource;
    private readonly ReadOnlyObservableCollection<PacketFilterViewModel> _filters;
    
    private readonly SourceCache<PacketMessageViewModel,Guid> _packetsSource;
    private readonly ReadOnlyObservableCollection<PacketMessageViewModel> _packets;

    public PacketViewerViewModel() : base(Uri)
    {
        PlayPause = ReactiveCommand.Create(() => IsPause = !IsPause);
    }
    
    [ImportingConstructor]
    public PacketViewerViewModel(IMavlinkDevicesService mavlinkDevicesService, ILocalizationService localizationService) : this()
    {
        _localization = localizationService;

        _packetsSource = new SourceCache<PacketMessageViewModel, Guid>(_ => _.Id);
        _filtersSource = new SourceCache<PacketFilterViewModel, string>(_ => _.Id);
        
        mavlinkDevicesService.Router
            .Where(_=>IsPause == false)
            .ObserveOn(RxApp.MainThreadScheduler)
            .Select(_=>new PacketMessageViewModel(_))
            .Do(UpdateFilterSource)
            .Subscribe(_=>_packetsSource.AddOrUpdate(_))
            .DisposeItWith(Disposable);
        
        _filterUpdate.OnNext(FilterPredicate);
        _packetsSource
            .Connect()
            .Filter(_filterUpdate)
            .SortBy(_=>_.DateTime)
            .Bind(out _packets)
            .Subscribe()
            .DisposeItWith(Disposable);

        _filtersSource
            .Connect()
            .Bind(out _filters)
            .Subscribe()
            .DisposeItWith(Disposable);

        _filtersSource
            .Connect()
            .WhenValueChanged(_ => _.IsChecked)
            .Subscribe(_ => _filterUpdate.OnNext(FilterPredicate))
            .DisposeItWith(Disposable);

    }

    private void UpdateFilterSource(PacketMessageViewModel obj)
    {
        var exist = _filtersSource.Lookup(obj.FilterId);
        if (exist.HasValue)
        {
            exist.Value.UpdateRates();
        }
        else
        {
            _filtersSource.AddOrUpdate(new PacketFilterViewModel(obj,_localization));
        }
    }

    public ICommand PlayPause { get; }

    public ReadOnlyObservableCollection<PacketMessageViewModel> Packets => _packets;
    public ReadOnlyObservableCollection<PacketFilterViewModel> Filters => _filters;
    
    
    
    [Reactive] public bool IsPause { get; set; }

    private bool FilterPredicate(PacketMessageViewModel vm)
    {
        return _filters.Where(_=>_.IsChecked).Any(_=>_.Id == vm.FilterId);
    }
  

    public void SetArgs(Uri link)
    {
    }

  

    private Task UncheckFilters()
    {
        foreach (var source in Filters)
        {
            source.IsChecked = false;
        }

        return Task.CompletedTask;
    }
    
    private Task CheckFilters()
    {
        foreach (var source in Filters)
        {
            source.IsChecked = true;
        }

        return Task.CompletedTask;
    }
}