using System.Collections.Specialized;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Asv.Common;
using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Mixins;
using Avalonia.Input;
using Avalonia.LogicalTree;
using Avalonia.Media;
using ReactiveUI;
using Path = Avalonia.Controls.Shapes.Path;

namespace Asv.Avalonia.Map
{
    [PseudoClasses(":pressed", ":selected")]
    public class MapViewItem : ContentControl, ISelectable, IActivatableView
    {
        private MapView _map;

        static MapViewItem()
        {
            SelectableMixin.Attach<MapViewItem>(IsSelectedProperty);
            PressedMixin.Attach<MapViewItem>();
            FocusableProperty.OverrideDefaultValue<MapViewItem>(true);

        }

        public MapViewItem()
        {
            this.WhenActivated(disp =>
            {
                DisposableMixins.DisposeWith(this.WhenAnyValue(_ => _.IsSelected).Subscribe(UpdateSelectableZindex), disp);
                DisposableMixins.DisposeWith(this.WhenAnyValue(_ => _.Bounds).Subscribe(_ => UpdateLocalPosition()), disp);

                Observable.FromEventPattern<EventHandler<PointerPressedEventArgs>, PointerPressedEventArgs>(
                    handler => PointerPressed += handler,
                    handler => PointerPressed -= handler).Subscribe(_=>DragPointerPressed(_.EventArgs)).DisposeItWith(disp);
                Observable.FromEventPattern<EventHandler<PointerEventArgs>, PointerEventArgs>(
                    handler => PointerMoved += handler,
                    handler => PointerMoved -= handler).Subscribe(_ => DragPointerMoved(_.EventArgs)).DisposeItWith(disp);
                
                // DisposableMixins.DisposeWith(this.Events().PointerReleased.Where(_ => IsEditable).Subscribe(DragPointerReleased), disp);
                

            });

        }

        public bool IsEditable
        {
            get => _isEditable;
            set => _isEditable = value;
        }

        private void DragPointerMoved(PointerEventArgs args)
        {
            if ((args.KeyModifiers & KeyModifiers.Control) != 0 && IsSelected)
            {
                if (_map == null) return;

                var child = LogicalChildren.FirstOrDefault() as Visual;
                if (child == null) return;

                var point = args.GetCurrentPoint(_map.MapCanvas);
                var offsetX = 0;
                var offsetY = 0;
                var old = MapView.GetLocation(child);
                var location = _map.FromLocalToLatLng((int)(point.Position.X  + _map.MapTranslateTransform.X + offsetX), (int)(point.Position.Y + _map.MapTranslateTransform.Y + offsetY));
                location = new GeoPoint(location.Latitude, location.Longitude, old.Altitude);
                MapView.SetLocation(child, location); 
            }
        }
        private void DragPointerPressed(PointerPressedEventArgs args)
        {
            
            if ((args.KeyModifiers & KeyModifiers.Control) != 0)
            {
                IsSelected = true;
                args.Handled = true;
            }
        }
        // private void DragPointerReleased(PointerReleasedEventArgs args)
        // {
        //     
        // }

        

        private void UpdateSelectableZindex(bool isSelected)
        {
            if (LogicalChildren.FirstOrDefault() is ISelectable item)
            {
                item.IsSelected = isSelected;
            }

            if (LogicalChildren.FirstOrDefault() is ISelectable item2)
            {
                item2.IsSelected = isSelected;
            }

            if (isSelected)
            {
                ZIndex += 10000;
            }
            else
            {
                ZIndex -= 10000;
            }
        }

        protected override void LogicalChildrenCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            base.LogicalChildrenCollectionChanged(sender, e);
            if (LogicalChildren.FirstOrDefault() is not Visual child) return;
            ZIndex = MapView.GetZOrder(child);
            IsEditable = MapView.GetIsEditable(child);
        }

        protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
        {
            base.OnAttachedToVisualTree(e);

            IControl a = this;
            while (a != null)
            {
                a = a.Parent;
                if (a is MapView map)
                {
                    _map = map;
                    UpdateLocalPosition();
                    break;
                }
            }
        }


        private IDisposable _collectionSubscribe;
        private bool _firstCall = true;
        private int? _lastHash;

        public void UpdatePathCollection()
        {
            _collectionSubscribe?.Dispose();
            if (LogicalChildren.FirstOrDefault() is not Visual child) return;
            var pathPoints = MapView.GetPath(child);
            if (pathPoints is INotifyCollectionChanged coll)
            {
                _collectionSubscribe = Observable.FromEventPattern<NotifyCollectionChangedEventHandler, NotifyCollectionChangedEventArgs>(
                    _ => coll.CollectionChanged += _, _ => coll.CollectionChanged -= _).ObserveOn(RxApp.MainThreadScheduler).Subscribe(_=>UpdateLocalPosition());
            }
        }

        protected override void OnAttachedToLogicalTree(LogicalTreeAttachmentEventArgs e)
        {
            base.OnAttachedToLogicalTree(e);
            UpdateLocalPosition();
        }

        public void UpdateLocalPosition()
        {
            if (_map == null) return;

            var child = LogicalChildren.FirstOrDefault() as Visual;
            if (child == null) return;

            var pathPoints = MapView.GetPath(child)?.ToArray();

            
                        
            
            if (pathPoints is { Length: > 1 })
            {
                IsShapeNotAvailable = false;// this is for hide content and draw only path
                if (_firstCall)
                {
                    _firstCall = false;
                    UpdatePathCollection();
                }
                
                var newHash = 0;
                
                
                var localPath = new List<GPoint>(pathPoints.Length);
                var minX = long.MaxValue;
                var minY = long.MaxValue;
                var lastPointAdded = GPoint.Empty;
                foreach (var p in pathPoints)
                {
                    var itemPoint = _map.FromLatLngToLocal(p);
                    itemPoint.Offset(-(long)(_map.MapTranslateTransform.X), -(long)(_map.MapTranslateTransform.Y));
                    if (itemPoint.X < minX)
                    {
                        minX = itemPoint.X;
                    }
                    if (itemPoint.Y < minY)
                    {
                        minY = itemPoint.Y;
                    }
                    // this is for optimization (if last two points are the same - no need to draw it)
                    if (lastPointAdded == itemPoint) continue;
                    localPath.Add(itemPoint);
                    lastPointAdded = itemPoint;
                    newHash = HashCode.Combine(newHash, itemPoint);
                }
                
                if (localPath.Count < 2) return;
                
                // this is for optimization (if values not changed - no need to update)
                if (newHash == _lastHash) return;
                _lastHash = newHash;
                
                Canvas.SetLeft(this, minX);
                Canvas.SetTop(this, minY);
                
                var truePath = new List<Point>(pathPoints.Length);
                foreach (var p in localPath)
                {
                    p.Offset(-minX,-minY);
                    truePath.Add(new Point(p.X, p.Y));
                }
                
                // Create a StreamGeometry to use to specify _myPath.
                var geometry = new StreamGeometry();
            
                geometry.BeginBatchUpdate();
                using (var ctx = geometry.Open())
                {
                    ctx.BeginFigure(truePath[0], false);
                    // Draw a line to the next specified point.
                    foreach (var path in truePath)
                    {
                        ctx.LineTo(path);
                    }
                    //ctx.PolyLineTo(localPath, true, true);
                }
            
                // Freeze the geometry (make it unmodifiable)
                // for additional performance benefits.
                geometry.EndBatchUpdate();
                if (Shape == null)
                {
                    // Create a path to draw a geometry with.
                    Shape = new Path();
                    {
                        // Specify the shape of the Path using the StreamGeometry.
                        Shape.Data = geometry;
                        Shape.Stroke = MapView.GetStroke(child);
                        Shape.StrokeThickness = MapView.GetStrokeThickness(child);
                        Shape.StrokeDashArray = MapView.GetStrokeDashArray(child);
                        Shape.Fill = MapView.GetFill(child);
                        Shape.Opacity = MapView.GetPathOpacity(child);
                        Shape.StrokeJoin = PenLineJoin.Round;
                        Shape.StrokeLineCap = PenLineCap.Square;
                        Shape.IsHitTestVisible = false;
                    }
                }
                else
                {
                    Shape.Data = geometry;
                    Shape.Stroke = MapView.GetStroke(child);
                    Shape.StrokeThickness = MapView.GetStrokeThickness(child);
                    Shape.StrokeDashArray = MapView.GetStrokeDashArray(child);
                    Shape.Fill = MapView.GetFill(child);
                    Shape.Opacity = MapView.GetPathOpacity(child);
                }
            }
            else
            {
                IsShapeNotAvailable = true;
                var location = MapView.GetLocation(child);
                var point = _map.FromLatLngToLocal(location);
                var offsetX = MapView.GetOffsetX(child);
                var offsetY = MapView.GetOffsetY(child);
                if (double.IsNaN(offsetX))
                {
                    offsetX = Bounds.Width / 2.0;
                }
                if (double.IsNaN(offsetY))
                {
                    offsetY = Bounds.Height / 2.0;
                }
                point.Offset(-(long)(_map.MapTranslateTransform.X + offsetX),
                    -(long)(_map.MapTranslateTransform.Y+ offsetY));
                Canvas.SetLeft(this, point.X);
                Canvas.SetTop(this, point.Y);
            }
        }

        public static readonly DirectProperty<MapViewItem, bool> IsShapeNotAvailableProperty =
            AvaloniaProperty.RegisterDirect<MapViewItem, bool>(nameof(IsShapeNotAvailable), o => o.IsShapeNotAvailable);
        private bool _isShapeNotAvailable = true;
        private bool _isEditable;

        public bool IsShapeNotAvailable
        {
            get => _isShapeNotAvailable;
            private set => SetAndRaise(IsShapeNotAvailableProperty, ref _isShapeNotAvailable, value);
        }

        public static readonly StyledProperty<Path> ShapeProperty = AvaloniaProperty.Register<MapViewItem, Path>(nameof(Shape));
        public Path Shape
        {
            get => GetValue(ShapeProperty);
            set => SetValue(ShapeProperty, value);
        }

        
        


        public static IEnumerable<TSource[]> Chunked<TSource>(IEnumerable<TSource> source, int size)
        {
            return ChunkIterator(source, size);
        }

        private static IEnumerable<TSource[]> ChunkIterator<TSource>(IEnumerable<TSource> source, int size)
        {
            if (size <= 0) throw new ArgumentOutOfRangeException(nameof(size));
            using IEnumerator<TSource> e = source.GetEnumerator();
            while (e.MoveNext())
            {
                TSource[] chunk = new TSource[size];
                chunk[0] = e.Current;

                int i = 1;
                for (; i < chunk.Length && e.MoveNext(); i++)
                {
                    chunk[i] = e.Current;
                }

                if (i == chunk.Length)
                {
                    yield return chunk;
                }
                else
                {
                    Array.Resize(ref chunk, i);
                    yield return chunk;
                    yield break;
                }
            }
        }

        public static readonly StyledProperty<bool> IsSelectedProperty =
            AvaloniaProperty.Register<MapViewItem, bool>(nameof(IsSelected));

        

        public bool IsSelected
        {
            get => GetValue(IsSelectedProperty);
            set => SetValue(IsSelectedProperty, value);
        }

        
    }
}
