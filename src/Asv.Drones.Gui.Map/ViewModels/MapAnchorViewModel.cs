using System.Collections.ObjectModel;
using Asv.Common;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Media;
using Material.Icons;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Asv.Avalonia.Map
{

    public enum OffsetXEnum
    {
        Left,
        Center,
        Right
    }

    public enum OffsetYEnum
    {
        Top,
        Center,
        Bottom
    }


    public interface IMapAnchorViewModel
    {
        bool IsEditable { get; set; }
        int ZOrder { get; set; }
        OffsetXEnum OffsetX { get; set; }
        OffsetYEnum OffsetY { get; set; }
        bool IsSelected { get; set; }
        bool IsVisible { get; set; }
        GeoPoint Location { get; set; }
        MaterialIconKind Icon { get; set; }
        double RotateCenterX { get; set; }
        double RotateCenterY { get; set; }
        IBrush IconBrush { get; set; }
        double RotateAngle { get; set; }
        string Title { get; set; }
        string Description { get; set; }
        double Size { get; set; }
        ReadOnlyObservableCollection<MapAnchorActionViewModel> Actions { get; }
        ReadOnlyObservableCollection<GeoPoint> Path { get; }
        IBrush Fill { get; set; }
        IBrush Stroke { get; set; }
        double StrokeThickness { get; set; }
        AvaloniaList<double> StrokeDashArray { get; set; }
        double PathOpacity { get; set; }
    }

    public class MapAnchorViewModel: ReactiveObject, IMapAnchorViewModel
    {
        public MapAnchorViewModel()
        {
            if (Design.IsDesignMode)
            {
                Actions = new ReadOnlyObservableCollection<MapAnchorActionViewModel>(
                    new ObservableCollection<MapAnchorActionViewModel>
                    {
                        new() {Title = "Action1", Icon = MaterialIconKind.Run},
                        new() {Title = "Action2", Icon = MaterialIconKind.Run}
                    });
            }
        }
        [Reactive]
        public bool IsEditable { get; set; } = false;
        [Reactive]
        public int ZOrder { get; set; }
        [Reactive]
        public OffsetXEnum OffsetX { get; set; }
        [Reactive]
        public OffsetYEnum OffsetY { get; set; }
        [Reactive]
        public bool IsSelected { get; set; }
        [Reactive]
        public bool IsVisible { get; set; }
        [Reactive]
        public GeoPoint Location { get; set; }
        [Reactive]
        public MaterialIconKind Icon { get; set; }
        [Reactive]
        public double RotateCenterX { get; set; }
        [Reactive]
        public double RotateCenterY { get; set; }
        [Reactive] 
        public IBrush IconBrush { get; set; }
        [Reactive]
        public double RotateAngle { get; set; }
        [Reactive]
        public string Title { get; set; }
        [Reactive]
        public string Description { get; set; }
        [Reactive]
        public double Size { get; set; } = 32;
        
        public virtual ReadOnlyObservableCollection<MapAnchorActionViewModel> Actions { get; }
        
        public virtual ReadOnlyObservableCollection<GeoPoint> Path { get; }

        [Reactive]
        public IBrush Fill { get; set; }
        [Reactive]
        public IBrush Stroke { get; set; } = Brushes.Blue;
        [Reactive]
        public double StrokeThickness { get; set; } = 3;
        [Reactive]
        public AvaloniaList<double> StrokeDashArray { get; set; }
        [Reactive]
        public double PathOpacity { get; set; } = 0.6;
        
        
    }
}