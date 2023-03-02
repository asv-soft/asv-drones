using Asv.Avalonia.Map;
using Asv.Common;
using DynamicData;
using System.Collections.ObjectModel;
using System.Reactive.Disposables;

namespace Asv.Drones.Gui.Core
{
    /// <summary>
    /// Map interface
    /// </summary>
    public interface IMap
    {
        int MaxZoom { get; set; }
        int MinZoom { get; set; }
        double Zoom { get; set; }
        GeoPoint Center { get; set; }
        ReadOnlyObservableCollection<IMapAnchor> Markers { get; }
        IMapAnchor SelectedItem { get; set; }
        Task<GeoPoint> ShowTargetDialog(string text, CancellationToken cancel);
    }
    
}