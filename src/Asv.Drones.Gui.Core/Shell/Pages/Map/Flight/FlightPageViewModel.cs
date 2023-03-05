using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Reactive.Linq;
using Asv.Avalonia.Map;
using Asv.Common;
using Avalonia.Controls;
using DynamicData;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Asv.Drones.Gui.Core
{
    [ExportShellPage(UriString)]
    [PartCreationPolicy(CreationPolicy.Shared)] //Important shared mode
    public class FlightPageViewModel: MapPageViewModel
    {
        public const string UriString = ShellPage.UriString + ".flight";
        public static readonly Uri Uri = new(UriString);
        
        [ImportingConstructor]
        public FlightPageViewModel( IMapService map, 
            [ImportMany(UriString)] IEnumerable<IViewModelProvider<IMapAnchor>> markers,
            [ImportMany(UriString)] IEnumerable<IViewModelProvider<IMapWidget>> widgets):base(Uri,map,markers,widgets)
        {
            
        }
    }
}