using System.ComponentModel.Composition;
using Asv.Cfg;
using Asv.Common;
using DynamicData.Binding;
using Material.Icons;
using ReactiveUI.Fody.Helpers;

namespace Asv.Drones.Gui.Core
{
    public class FlightPageViewModelConfig
    {
        public double Zoom { get; set; }
        public GeoPoint MapCenter { get; set; }
    }
    
    [ExportShellPage(UriString)]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class FlightPageViewModel: MapPageViewModel
    {
        public const string UriString = "asv:shell.page.map.flight";
        public static readonly Uri Uri = new(UriString);
        
        [ImportingConstructor]
        public FlightPageViewModel( IMapService map, IConfiguration cfg,
            [ImportMany(UriString)] IEnumerable<IViewModelProvider<IMapAnchor>> markers,
            [ImportMany(UriString)] IEnumerable<IViewModelProvider<IMapWidget>> widgets,
            [ImportMany(UriString)] IEnumerable<IViewModelProvider<IMapAction>> actions):base(Uri,map,markers,widgets,actions)
        {
            Title = RS.FlightShellMenuItem_Name;
            Icon = MaterialIconKind.Map;
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