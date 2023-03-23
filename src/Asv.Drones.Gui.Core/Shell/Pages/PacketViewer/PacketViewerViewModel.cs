using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
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
    
    private ReadOnlyObservableCollection<PacketMessageViewModel> _filteredPackets;
    private DateTime _lastTime;
    
    public const string UriString = ShellPage.UriString + ".packetViewer";
    public static readonly Uri Uri = new Uri(UriString);

    public ReadOnlyObservableCollection<PacketMessageViewModel> FilteredPackets => _filteredPackets;
    [Reactive] public SourceList<PacketMessageViewModel> Packets { get; set; } = new();
    [Reactive] public ObservableCollection<PacketFilterViewModel> Sources { get; set; } = new();
    [Reactive] public bool IsPause { get; set; }

    public Subject<Unit> OnFilterChanged { get; } = new();
    
    public ReactiveCommand<Unit, Unit> PlayPause { get; set; }

    public PacketViewerViewModel() : base(Uri)
    {
        PlayPause = ReactiveCommand.CreateFromTask(SwitchIsPause);
    }
    
    [ImportingConstructor]
    public PacketViewerViewModel(IMavlinkDevicesService mavlinkDevicesService, ILocalizationService localizationService) : this()
    {
        _localization = localizationService;
        
        mavlinkDevicesService.Router
            .ObserveOn(RxApp.MainThreadScheduler)
            .Subscribe(AddMessage)
            .DisposeItWith(Disposable);

        Packets.LimitSizeTo(1000);
        
        Packets
            .Connect()
            .AutoRefreshOnObservable(_ => OnFilterChanged)
            .Filter(Filter)
            .Bind(out _filteredPackets)
            .Subscribe()
            .DisposeItWith(Disposable);

        Sources
            .ToObservableChangeSet()
            .AutoRefresh(vm => vm.IsChecked)
            .Subscribe(_ => OnFilterChanged.Next())
            .DisposeItWith(Disposable);
    }

    private bool Filter(PacketMessageViewModel vm)
    {
        return Sources.Any(o => o.IsChecked && o.Type == vm.Type);
    }

    private void AddMessage(IPacketV2<IPayload> packetV2)
    {
        if (IsPause) return;
        
        var message = new PacketMessageViewModel(DateTime.Now, $"{packetV2.SystemId},{packetV2.ComponenId}",
            JsonConvert.SerializeObject(packetV2?.Payload), packetV2.Name);
        
        AddOrUpdateFilters(message);

        Packets.Insert(0, message);
    }

    private void AddOrUpdateFilters(PacketMessageViewModel vm)
    {
        var source = Sources.Where(_ => _.Type == vm.Type && _.Source == vm.Source);
        
        if (!source.Any())
        {
            Sources.Add(new PacketFilterViewModel(vm.Type, vm.Source, _localization));
        }
        else
        {
            source.FirstOrDefault().UpdateRates();
        }
    }

    public void SetArgs(Uri link)
    {
    }

    private Task SwitchIsPause()
    {
        IsPause = !IsPause;
        return Task.CompletedTask;
    }

    private Task UncheckFilters()
    {
        foreach (var source in Sources)
        {
            source.IsChecked = false;
        }

        return Task.CompletedTask;
    }
    
    private Task CheckFilters()
    {
        foreach (var source in Sources)
        {
            source.IsChecked = true;
        }

        return Task.CompletedTask;
    }
}