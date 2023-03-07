using Asv.Avalonia.Map;
using Asv.Common;
using DynamicData;
using System.Collections.ObjectModel;
using System.Reactive.Disposables;
using Material.Icons;

namespace Asv.Drones.Gui.Core
{
    /// <summary>
    /// Map interface
    /// </summary>
    public interface IMap
    {
        bool IsInDialogMode { get; set; }
        int MaxZoom { get; set; }
        int MinZoom { get; set; }
        double Zoom { get; set; }
        GeoPoint Center { get; set; }
        ReadOnlyObservableCollection<IMapAnchor> Markers { get; }
        IMapAnchor SelectedItem { get; set; }
        Task<GeoPoint> ShowTargetDialog(string text, CancellationToken cancel);
    }
    
    /// <summary>
    /// Anchor on map
    /// </summary>
    public interface IMapAnchor : IMapAnchorViewModel,IViewModel
    {
        IMapAnchor Init(IMap map);
    }
    
    public enum WidgetLocation
    {
        Left,   
        Right,
        Bottom
    }
    public interface IMapWidget:IViewModel
    {
        WidgetLocation Location { get; }
        public string Title { get; }
        public MaterialIconKind Icon { get; }
        IMapWidget Init(IMap map);
    }
}