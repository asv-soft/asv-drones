using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Reactive.Linq;
using Asv.Avalonia.Map;
using Asv.Common;
using Avalonia.Controls;
using DynamicData;
using DynamicData.Binding;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Asv.Drones.Gui.Core
{
    [ExportShellPage(UriString)]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class PlaningPageViewModel:MapPageViewModel
    {
        public const string UriString = ShellPage.UriString + ".planing";
        public static readonly Uri Uri = new(UriString);
        
        [ImportingConstructor]
        public PlaningPageViewModel( IMapService map, 
            [ImportMany(UriString)] IEnumerable<IViewModelProvider<IMapAnchor>> markers,
            [ImportMany(UriString)] IEnumerable<IViewModelProvider<IMapWidget>> widgets):base(Uri,map,markers,widgets)
        {
            
        }

        
    }
}