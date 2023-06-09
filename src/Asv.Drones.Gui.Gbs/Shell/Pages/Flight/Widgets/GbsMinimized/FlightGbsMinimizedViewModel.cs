using System.Collections.ObjectModel;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Windows.Input;
using Asv.Cfg;
using Asv.Common;
using Asv.Drones.Gui.Core;
using Asv.Mavlink;
using Avalonia.Controls;
using DynamicData;
using DynamicData.Binding;
using Material.Icons;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Asv.Drones.Gui.Gbs;

public class FlightGbsMinimizedViewModel : FlightGbsWidgetBase
{
    private readonly ReadOnlyObservableCollection<IGbsRttItem> _rttItems;
    private readonly ILogService _logService;
    private readonly ILocalizationService _loc;
    public static Uri GenerateUri(IGbsClientDevice gbs) => FlightGbsWidgetBase.GenerateUri(gbs,"gbs_minimized");
    
    public FlightGbsMinimizedViewModel(IGbsClientDevice baseStationDevice, ILogService log, ILocalizationService loc, RxValue<bool> collapsed, IEnumerable<IGbsRttItemProvider> rttItems)
        :base(baseStationDevice, GenerateUri(baseStationDevice))
    {
        _logService = log;
        _loc = loc;
        
        Icon = MaterialIconKind.RouterWireless;
        Title = RS.FlightGbsViewModel_Title;
        
        rttItems
            .SelectMany(_ => _.Create(baseStationDevice))
            .OrderBy(_=>_.Order)
            .AsObservableChangeSet()
            .AutoRefresh(_=>_.IsVisible)
            .Filter(_=> _.IsVisible 
                        & (_ is LinkQualityGbsRttViewModel | 
                           _ is VisibleSatellitesGbsRttViewModel))
            .Bind(out _rttItems)
            .DisposeMany()
            .Subscribe()
            .DisposeItWith(Disposable);
        
        MaximizeCommand = ReactiveCommand.Create(() =>
        {
            collapsed.Value = false;
        }).DisposeItWith(Disposable);
        
        collapsed.Subscribe(_ => IsVisible = _).DisposeItWith(Disposable);
    }
    
    protected override void InternalAfterMapInit(IMap map)
    {
        base.InternalAfterMapInit(map);
        LocateBaseStationCommand = ReactiveCommand.Create(() =>
        {
            Map.Center = BaseStation.Gbs.Position.Value;
            var selectedGbs = Map.Markers.Where(_=>_ is GbsAnchor).Cast<GbsAnchor>().FirstOrDefault(_=>_.Device.FullId == BaseStation.FullId);
            if (selectedGbs != null)
            {
                selectedGbs.IsSelected = true;
            }
        }).DisposeItWith(Disposable);
    }

    public ReadOnlyObservableCollection<IGbsRttItem> RttItems => _rttItems;

    public ICommand LocateBaseStationCommand { get; set; }
    public ICommand MaximizeCommand { get; set; }
    
    [Reactive] 
    public bool IsVisible { get; set; }
}