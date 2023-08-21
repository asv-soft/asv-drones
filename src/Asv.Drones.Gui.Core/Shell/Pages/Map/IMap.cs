using Asv.Avalonia.Map;
using Asv.Common;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Material.Icons;

namespace Asv.Drones.Gui.Core
{
    /// <summary>
    /// Map interface
    /// </summary>
    public interface IMap:INotifyPropertyChanged
    {
        bool IsInDialogMode { get; set; }
        int MaxZoom { get; set; }
        int MinZoom { get; set; }
        double Zoom { get; set; }
        GeoPoint Center { get; set; }
        ReadOnlyObservableCollection<IMapAnchor> Markers { get; }
        IMapAnchor SelectedItem { get; set; }
        IMapAnchor? ItemToFollow { get; set; }
        bool IsInAnchorEditMode { get; set; }
        Task<GeoPoint> ShowTargetDialog(string text, CancellationToken cancel);
    }
    
    /// <summary>
    /// Anchor on map
    /// </summary>
    public interface IMapAnchor : IMapAnchorViewModel, IViewModel
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
        string Title { get; }
        int Order { get; }
        MaterialIconKind Icon { get; }
        IMapWidget Init(IMap context);
    }
}