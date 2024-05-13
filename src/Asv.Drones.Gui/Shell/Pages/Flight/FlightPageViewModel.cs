using System;
using System.Collections.Generic;
using System.Composition;
using Asv.Cfg;
using Asv.Common;
using Asv.Drones.Gui.Api;
using DynamicData.Binding;
using Material.Icons;
using ReactiveUI.Fody.Helpers;

namespace Asv.Drones.Gui
{
    public class FlightPageViewModelConfig
    {
        public double Zoom { get; set; }
        public GeoPoint MapCenter { get; set; }
    }

    [ExportShellPage(WellKnownUri.ShellPageMapFlight)]
    public class FlightPageViewModel : MapPageViewModel
    {
        [ImportingConstructor]
        public FlightPageViewModel(IMapService map, IConfiguration cfg,
            [ImportMany(WellKnownUri.ShellPageMapFlight)]
            IEnumerable<IViewModelProvider<IMapMenuItem>> menuItems,
            [ImportMany(WellKnownUri.ShellPageMapFlight)]
            IEnumerable<IViewModelProvider<IMapStatusItem>> statusItems,
            [ImportMany(WellKnownUri.ShellPageMapFlight)]
            IEnumerable<IViewModelProvider<IMapAnchor>> markers,
            [ImportMany(WellKnownUri.ShellPageMapFlight)]
            IEnumerable<IViewModelProvider<IMapWidget>> widgets,
            [ImportMany(WellKnownUri.ShellPageMapFlight)]
            IEnumerable<IViewModelProvider<IMapAction>> actions)
            : base(WellKnownUri.ShellPageMapFlightUri, map, statusItems, menuItems, markers, widgets, actions)
        {
            Title = RS.FlightShellMenuItem_Name;
            Icon = MaterialIconKind.Map;
            FlightConfig = cfg.Get<FlightPageViewModelConfig>();

            Zoom = FlightConfig.Zoom is 0 ? 1 : FlightConfig.Zoom;

            Center = FlightConfig.MapCenter;

            this.WhenValueChanged(x => x.Zoom, false)
                .Subscribe(_ =>
                {
                    FlightConfig.MapCenter = Center;
                    FlightConfig.Zoom = Zoom;
                    cfg.Set(FlightConfig);
                })
                .DisposeItWith(Disposable);

            this.WhenValueChanged(x => x.Center, false)
                .Subscribe(_ =>
                {
                    FlightConfig.MapCenter = Center;
                    FlightConfig.Zoom = Zoom;
                    cfg.Set(FlightConfig);
                })
                .DisposeItWith(Disposable);
        }


        [Reactive] public FlightPageViewModelConfig FlightConfig { get; set; }
    }
}