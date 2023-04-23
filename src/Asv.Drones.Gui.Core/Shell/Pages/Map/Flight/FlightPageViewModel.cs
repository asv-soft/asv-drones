using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Reactive.Linq;
using Asv.Avalonia.Map;
using Asv.Cfg;
using Asv.Common;
using Avalonia.Controls;
using DynamicData;
using DynamicData.Binding;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Asv.Drones.Gui.Core
{
    public class FlightPageViewModelConfig
    {
        public double Zoom { get; set; }
        public GeoPoint MapCenter { get; set; }
    }
    
    [ExportShellPage(UriString)]
    [PartCreationPolicy(CreationPolicy.Shared)] //Important shared mode
    public class FlightPageViewModel: MapPageViewModel
    {
        public const string UriString = ShellPage.UriString + ".flight";
        public static readonly Uri Uri = new(UriString);
        
        [ImportingConstructor]
        public FlightPageViewModel( IMapService map, IConfiguration cfg,
            [ImportMany(UriString)] IEnumerable<IViewModelProvider<IMapAnchor>> markers,
            [ImportMany(UriString)] IEnumerable<IViewModelProvider<IMapWidget>> widgets):base(Uri,map,markers,widgets)
        {
            FlightConfig = cfg.Get<FlightPageViewModelConfig>();
            
            Zoom = FlightConfig.Zoom is 0 ? 1 : FlightConfig.Zoom;

            Center = FlightConfig.MapCenter;
            
            this.WhenPropertyChanged(_ => _.Zoom)
                .Subscribe(_ =>
                {
                    FlightConfig.MapCenter = Center;
                    FlightConfig.Zoom = Zoom;
                    cfg.Set(FlightConfig);
                })
                .DisposeItWith(Disposable);
            
            this.WhenPropertyChanged(_ => _.Center)
                .Subscribe(_ =>
                {
                    FlightConfig.MapCenter = Center;
                    FlightConfig.Zoom = Zoom;
                    cfg.Set(FlightConfig);
                })
                .DisposeItWith(Disposable);
        }
        
        
        
        [Reactive]
        public FlightPageViewModelConfig FlightConfig { get; set; }
    }

}