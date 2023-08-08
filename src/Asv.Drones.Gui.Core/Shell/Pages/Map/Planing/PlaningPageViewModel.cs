using System.ComponentModel.Composition;
using Asv.Cfg;
using Asv.Common;
using DynamicData.Binding;
using Material.Icons;
using ReactiveUI.Fody.Helpers;

namespace Asv.Drones.Gui.Core
{
    public class PlanningPageViewModelConfig
    {
        public double Zoom { get; set; }
        public GeoPoint MapCenter { get; set; }
    }
    
    [ExportShellPage(UriString)]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class PlaningPageViewModel:MapPageViewModel
    {
        public const string UriString = "asv:shell.page.map.planing";
        public static readonly Uri Uri = new(UriString);
        
        [ImportingConstructor]
        public PlaningPageViewModel( IMapService map, IConfiguration cfg, 
            [ImportMany(UriString)] IEnumerable<IViewModelProvider<IMapAnchor>> markers,
            [ImportMany(UriString)] IEnumerable<IViewModelProvider<IMapWidget>> widgets,
            [ImportMany(UriString)] IEnumerable<IViewModelProvider<IMapAction>> actions):base(Uri,map,markers,widgets,actions)
        {
            Title = RS.PlaningShellMenuItem_Name;
            Icon = MaterialIconKind.MapMarkerCheck;
            PlanningConfig = cfg.Get<PlanningPageViewModelConfig>();

            Zoom = PlanningConfig.Zoom is 0 ? 1 : PlanningConfig.Zoom;

            Center = PlanningConfig.MapCenter;

            this.WhenPropertyChanged(_ => _.Zoom)
                .Subscribe(_ =>
                {
                    PlanningConfig.Zoom = _.Value;
                    cfg.Set(PlanningConfig);
                })
                .DisposeItWith(Disposable);
            
            this.WhenPropertyChanged(_ => _.Center)
                .Subscribe(_ =>
                {
                    PlanningConfig.MapCenter = _.Value;
                    cfg.Set(PlanningConfig);
                })
                .DisposeItWith(Disposable);
        }

        [Reactive]
        public PlanningPageViewModelConfig PlanningConfig { get; set; }
    }
}