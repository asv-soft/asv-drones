#nullable enable
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading;
using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Generators;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Selection;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.LogicalTree;
using Avalonia.Media;
using Avalonia.VisualTree;
using ReactiveUI;
using GeoPoint = Asv.Common.GeoPoint;

namespace Asv.Avalonia.Map
{
    public class MapView : SelectingItemsControl,IActivatableView,IDisposable
    {
        private const double MinimumHorizontalDragDistance = 50;
        private const double MinimumVerticalDragDistance = 50;

        private readonly Core _core = new();
        internal readonly TranslateTransform MapTranslateTransform = new();
        internal readonly TranslateTransform MapOverlayTranslateTransform = new();
        internal ScaleTransform MapScaleTransform = new();
        private readonly ScaleTransform _lastScaleTransform = new();
        private readonly MouseDevice _mouse = new();

        static MapView()
        {
            MapImageProxy.Enable();
            
            LocationProperty.Changed.Subscribe(_=>UpdateLocalPosition(_.Sender));
            OffsetXProperty.Changed.Subscribe(_ => UpdateLocalPosition(_.Sender));
            OffsetYProperty.Changed.Subscribe(_ => UpdateLocalPosition(_.Sender));
            PathProperty.Changed.Subscribe(_ => UpdatePath(_.Sender));
            ZOrderProperty.Changed.Subscribe(_ => UpdateZOrder(_.Sender,_.NewValue));
            IsEditableProperty.Changed.Subscribe(_ => UpdateIsEditable(_.Sender, _.NewValue));

        }

        private static void UpdateIsEditable(IAvaloniaObject obj, BindingValue<bool> objNewValue)
        {
            var find = (obj as ILogical).GetLogicalParent<MapViewItem>();
            if (find != null)
            {
                find.IsEditable = objNewValue.Value;
            }
        }

        private static void UpdateZOrder(IAvaloniaObject obj, BindingValue<int> objNewValue)
        {
            var find = (obj as ILogical).GetLogicalParent<MapViewItem>();
            if (find != null)
            {
                find.ZIndex = objNewValue.Value;
            }
        }

        private static void UpdatePath(IAvaloniaObject obj)
        {
            var find = (obj as ILogical).GetLogicalParent<MapViewItem>();
            find?.UpdatePathCollection();
        }

        private static void UpdateLocalPosition(IAvaloniaObject obj)
        {
            var find = (obj as ILogical).GetLogicalParent<MapViewItem>();
            find?.UpdateLocalPosition();
        }

        public MapView()
        {
            Disposable.Add(_core);
            _core.SystemType = "WindowsPresentation";
            _core.RenderMode = RenderMode.WPF;
            
            Zoom = _core.MaxZoom;
            MinZoom = _core.MinZoom;
            MaxZoom = _core.MaxZoom;
            MapProvider = GMapProviders.BingHybridMap;
            Position = new GeoPoint(55.1644, 61.4368,0);
            if (Design.IsDesignMode)
            {
                
                
            }
            _core.OnMapZoomChanged += ForceUpdateOverlays;
            _core.OnCurrentPositionChanged += point => Position = point;
            this.WhenAnyValue(_ => _.Bounds).Subscribe(_ =>
            {
                _core.OnMapSizeChanged((int)_.Width, (int)_.Height);
                UpdateZoom();
            }).DisposeWith(Disposable);
        }

        protected CompositeDisposable Disposable { get; } = new();

        #region AttachedProperty




        public static readonly AttachedProperty<IBrush?> StrokeProperty =
            AvaloniaProperty.RegisterAttached<MapView, AvaloniaObject, IBrush?>("Stroke", Brushes.Blue);
        public static void SetStroke(IAvaloniaObject element, IBrush? value) => element.SetValue(StrokeProperty, value);
        public static IBrush? GetStroke(IAvaloniaObject element) => (IBrush?)element.GetValue(StrokeProperty);

        public static readonly AttachedProperty<IBrush?> FillProperty =
            AvaloniaProperty.RegisterAttached<MapView, AvaloniaObject, IBrush?>("Fill",null);
        public static void SetFill(IAvaloniaObject element, IBrush? value) => element.SetValue(FillProperty, value);
        public static IBrush? GetFill(IAvaloniaObject element) => (IBrush?)element.GetValue(FillProperty);

        public static readonly AttachedProperty<double> StrokeThicknessProperty =
            AvaloniaProperty.RegisterAttached<MapView, AvaloniaObject, double>("StrokeThickness",3);
        public static void SetStrokeThickness(IAvaloniaObject element, double value) => element.SetValue(StrokeThicknessProperty, value);
        public static double GetStrokeThickness(IAvaloniaObject element) => (double)element.GetValue(StrokeThicknessProperty);

        public static readonly AttachedProperty<AvaloniaList<double>> StrokeDashArrayProperty =
            AvaloniaProperty.RegisterAttached<MapView, AvaloniaObject, AvaloniaList<double>>("StrokeDashArray");
        public static void SetStrokeDashArray(IAvaloniaObject element, AvaloniaList<double> value) => element.SetValue(StrokeDashArrayProperty, value);
        public static AvaloniaList<double> GetStrokeDashArray(IAvaloniaObject element) => (AvaloniaList<double>)element.GetValue(StrokeDashArrayProperty);

        public static readonly AttachedProperty<double> PathOpacityProperty =
            AvaloniaProperty.RegisterAttached<MapView, AvaloniaObject, double>("PathOpacity");
        public static void SetPathOpacity(IAvaloniaObject element, double value) => element.SetValue(PathOpacityProperty, value);
        public static double GetPathOpacity(IAvaloniaObject element) => (double)element.GetValue(PathOpacityProperty);


        public static readonly AttachedProperty<IList<GeoPoint>> PathProperty =
            AvaloniaProperty.RegisterAttached<MapView, AvaloniaObject, IList<GeoPoint>>("Path");
        public static void SetPath(IAvaloniaObject element, IList<GeoPoint> value) => element.SetValue(PathProperty, value);
        public static IList<GeoPoint> GetPath(IAvaloniaObject element) => (IList<GeoPoint>)element.GetValue(PathProperty);

        public static readonly AttachedProperty<GeoPoint> LocationProperty =
            AvaloniaProperty.RegisterAttached<MapView, AvaloniaObject, GeoPoint>("Location", GeoPoint.ZeroWithAlt);
        public static void SetLocation(IAvaloniaObject element, GeoPoint value) => element.SetValue(LocationProperty, value);
        public static GeoPoint GetLocation(IAvaloniaObject element) => (GeoPoint)element.GetValue(LocationProperty);

        public static readonly AttachedProperty<double> OffsetXProperty =
            AvaloniaProperty.RegisterAttached<MapView, AvaloniaObject, double>("OffsetX", 0);
        public static void SetOffsetX(IAvaloniaObject element, double value) => element.SetValue(OffsetXProperty, value);
        public static double GetOffsetX(IAvaloniaObject element) => (double)element.GetValue(OffsetXProperty);

        public static readonly AttachedProperty<double> OffsetYProperty =
            AvaloniaProperty.RegisterAttached<MapView, AvaloniaObject, double>("OffsetY", 0);
        public static void SetOffsetY(IAvaloniaObject element, double value) => element.SetValue(OffsetYProperty, value);
        public static double GetOffsetY(IAvaloniaObject element) => (double)element.GetValue(OffsetYProperty);

        public static readonly AttachedProperty<int> ZOrderProperty =
            AvaloniaProperty.RegisterAttached<MapView, AvaloniaObject, int>("ZOrder", defaultBindingMode: BindingMode.OneWay);
        public static void SetZOrder(IAvaloniaObject element, int value) => element.SetValue(ZOrderProperty, value);
        public static int GetZOrder(IAvaloniaObject element) => (int)element.GetValue(ZOrderProperty);

        public static readonly AttachedProperty<bool> IsEditableProperty =
            AvaloniaProperty.RegisterAttached<MapView, AvaloniaObject, bool>("IsEditable", defaultBindingMode: BindingMode.TwoWay);
        public static void SetIsEditable(IAvaloniaObject element, bool value) => element.SetValue(IsEditableProperty, value);
        public static bool GetIsEditable(IAvaloniaObject element) => (bool)element.GetValue(IsEditableProperty);


        #endregion

        #region Render

        private void DrawMap(DrawingContext g)
        {
            if (Equals(MapProvider, EmptyProvider.Instance) || MapProvider == null)
            {
                return;
            }

            _core.TileDrawingListLock.AcquireReaderLock();
            _core.Matrix.EnterReadLock();

            try
            {
                foreach (var tilePoint in _core.TileDrawingList)
                {
                    _core.TileRect.Location = tilePoint.PosPixel;
                    _core.TileRect.OffsetNegative(_core.CompensationOffset);

                    //if(region.IntersectsWith(Core.tileRect) || IsRotated)
                    {
                        bool found = false;

                        var t = _core.Matrix.GetTileWithNoLock(_core.Zoom, tilePoint.PosXY);

                        if (t.NotEmpty)
                        {
                            foreach (MapImage img in t.Overlays)
                            {
                                if (img != null && img.Img != null)
                                {
                                    if (!found)
                                        found = true;

                                    var imgRect = new Rect(_core.TileRect.X + 0.6,
                                        _core.TileRect.Y + 0.6,
                                        _core.TileRect.Width + 0.6,
                                        _core.TileRect.Height + 0.6);

                                    if (!img.IsParent)
                                    {
                                        g.DrawImage(img.Img, imgRect);
                                    }
                                    else
                                    {
                                        // TODO: move calculations to loader thread
                                        var geometry = new RectangleGeometry(imgRect);
                                        var parentImgRect =
                                            new Rect(_core.TileRect.X - _core.TileRect.Width * img.Xoff + 0.6,
                                                _core.TileRect.Y - _core.TileRect.Height * img.Yoff + 0.6,
                                                _core.TileRect.Width * img.Ix + 0.6,
                                                _core.TileRect.Height * img.Ix + 0.6);

                                        using (g.PushClip(geometry.Rect))
                                        {
                                            g.DrawImage(img.Img, parentImgRect);
                                        }
                                        geometry = null;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            finally
            {
                _core.Matrix.LeaveReadLock();
                _core.TileDrawingListLock.ReleaseReaderLock();
            }
        }

        public override void Render(DrawingContext drawingContext)
        {
            if (!_core.IsStarted)
                return;


            if (MapScaleTransform != null)
            {
                using (drawingContext.PushPreTransform(MapScaleTransform.Value))
                using (drawingContext.PushPreTransform(MapTranslateTransform.Value))
                {
                    DrawMap(drawingContext);
                }
            }
            else
            {
                using var _ = drawingContext.PushPreTransform(MapTranslateTransform.Value);
                DrawMap(drawingContext);
            }

            base.Render(drawingContext);
        }

        private void ForceUpdateOverlays()
        {
            UpdateMarkersOffset();
            foreach (var obj in LogicalChildren.Cast<MapViewItem>())
            {
                obj.UpdateLocalPosition();
                if (obj.IsSelected)
                {
                    var child = obj.GetLogicalChildren().FirstOrDefault() as Visual;
                    if (child == null) return;
                    Position = GetLocation(child);
                }
            }

            
        }

        private Canvas _mapCanvas;
        internal Canvas MapCanvas
        {
            get
            {
                if (_mapCanvas != null) return _mapCanvas;
                if (VisualChildren.Count <= 0) return _mapCanvas;
                _mapCanvas = this.GetVisualDescendants().FirstOrDefault(w => w is Canvas) as Canvas;
                if (_mapCanvas != null) _mapCanvas.RenderTransform = MapTranslateTransform;
                return _mapCanvas;
            }
        }
        
        protected override void OnInitialized()
        {
            base.OnInitialized();

            if (!_core.IsStarted)
            {
                _core.OnMapOpen().ProgressChanged += (_, _) => base.InvalidateVisual();
                ForceUpdateOverlays();
            }
        }

        public void InvalidateVisual(bool forced)
        {
            if (forced)
            {
                lock (_core.InvalidationLock)
                {
                    _core.LastInvalidation = DateTime.Now;
                }

                base.InvalidateVisual();
            }
            else
            {
                InvalidateVisual();
            }
        }

        public new void InvalidateVisual()
        {
            _core.Refresh?.Set();
        }

        private void UpdateMarkersOffset()
        {
            if (MapCanvas == null) return;

            if (MapScaleTransform != null)
            {
                var (x, y) = MapScaleTransform.Transform(
                    new Point(_core.RenderOffset.X, _core.RenderOffset.Y));
                MapOverlayTranslateTransform.X = x;
                MapOverlayTranslateTransform.Y = y;

                // map is scaled already
                MapTranslateTransform.X = _core.RenderOffset.X;
                MapTranslateTransform.Y = _core.RenderOffset.Y;
            }
            else
            {
                MapTranslateTransform.X = _core.RenderOffset.X;
                MapTranslateTransform.Y = _core.RenderOffset.Y;

                MapOverlayTranslateTransform.X = MapTranslateTransform.X;
                MapOverlayTranslateTransform.Y = MapTranslateTransform.Y;
            }
        }

        #endregion

        #region Zoom

        public static readonly DirectProperty<MapView, double> ZoomProperty =
            AvaloniaProperty.RegisterDirect<MapView, double>(nameof(Zoom), o => o.Zoom, (o, v) => o.Zoom = v);
        private double _zoom;
        public double Zoom
        {
            get => _zoom;
            set
            {
                if (SetAndRaise(ZoomProperty, ref _zoom, value))
                {
                    UpdateZoom();
                }
            }
        }

        public static readonly DirectProperty<MapView, ScaleModes> ScaleModeProperty =
            AvaloniaProperty.RegisterDirect<MapView, ScaleModes>(nameof(ScaleMode), o => o.ScaleMode, (o, v) => o.ScaleMode = v);

        private ScaleModes _scaleMode = ScaleModes.Dynamic;
        public ScaleModes ScaleMode
        {
            get => _scaleMode;
            set
            {
                if (SetAndRaise(ScaleModeProperty, ref _scaleMode, value))
                {
                    InvalidateVisual();
                }
            }
        }

        public static readonly DirectProperty<MapView, int> MinZoomProperty =
            AvaloniaProperty.RegisterDirect<MapView, int>(nameof(MinZoom), o => o.MinZoom, (o, v) => o.MinZoom = v);
        public int MinZoom
        {
            get => _core.MinZoom;
            set
            {
                if (SetAndRaise(MinZoomProperty, ref _core.MinZoom, value))
                {
                    UpdateZoom();
                }
            }
        }
        public static readonly DirectProperty<MapView, int> MaxZoomProperty =
            AvaloniaProperty.RegisterDirect<MapView, int>(nameof(MaxZoom), o => o.MaxZoom, (o, v) => o.MaxZoom = v);
        public int MaxZoom
        {
            get => _core.MaxZoom;
            set
            {
                if (SetAndRaise(MinZoomProperty, ref _core.MaxZoom, value))
                {
                    UpdateZoom();
                }
            }
        }

        private void UpdateZoom()
        {
            if (MapProvider?.Projection != null)
            {
                double remainder = Zoom % 1;

                if (ScaleMode != ScaleModes.Integer && remainder != 0 && Bounds.Width > 0)
                {
                    bool scaleDown;

                    switch (ScaleMode)
                    {
                        case ScaleModes.ScaleDown:
                            scaleDown = true;
                            break;

                        case ScaleModes.Dynamic:
                            scaleDown = remainder > 0.25;
                            break;

                        default:
                            scaleDown = false;
                            break;
                    }

                    if (scaleDown)
                        remainder--;

                    double scaleValue = Math.Pow(2d, remainder);
                    {
                        if (MapScaleTransform == null)
                        {
                            MapScaleTransform = _lastScaleTransform;
                        }

                        MapScaleTransform.ScaleX = scaleValue;
                        _core.ScaleX = 1 / scaleValue;
                        MapScaleTransform.ScaleY = scaleValue;
                        _core.ScaleY = 1 / scaleValue;
                    }

                    _core.Zoom = Convert.ToInt32(scaleDown ? Math.Ceiling(Zoom) : Zoom - remainder);
                }
                else
                {
                    MapScaleTransform = null;

                    _core.ScaleX = 1;
                    _core.ScaleY = 1;
                    _core.Zoom = (int)Math.Floor(Zoom);
                }

                if (IsInitialized)
                {
                    ForceUpdateOverlays();
                    InvalidateVisual();
                }
            }
        }

        #endregion

        #region Coordinate convertion

        public GeoPoint FromLocalToLatLng(int x, int y)
        {
            if (MapScaleTransform != null)
            {
                var tp = MapScaleTransform.Inverse().Transform(new Point(x, y));
                x = (int)tp.X;
                y = (int)tp.Y;
            }
            return _core.FromLocalToLatLng(x, y);
        }

        public GPoint FromLatLngToLocal(GeoPoint point)
        {
            var ret = _core.FromLatLngToLocal(point);

            if (MapScaleTransform != null)
            {
                var tp = MapScaleTransform.Transform(new Point(ret.X, ret.Y));
                ret.X = (int)tp.X;
                ret.Y = (int)tp.Y;
            }
            return ret;
        }

        #endregion

        #region Position

        public static readonly DirectProperty<MapView, GeoPoint> PositionProperty =
            AvaloniaProperty.RegisterDirect<MapView, GeoPoint>(nameof(Position), o => o.Position, (o, v) => o.Position = v);
        private GeoPoint _position = new GeoPoint(55.1644, 61.4368,0);
        public GeoPoint Position
        {
            get => _position;
            set
            {
                if (SetAndRaise(PositionProperty, ref _position, value))
                {
                    OnPositionChanged();
                }
            }
        }

        private void OnPositionChanged()
        {
            if (Interlocked.CompareExchange(ref _positionUpdateCiclicUpdateFlag,0,1) !=0) return;
            try
            {
                _core.Position = Position;
                if (_core.IsStarted)
                {
                    ForceUpdateOverlays();
                }
            }
            finally
            {
                Interlocked.Exchange(ref _positionUpdateCiclicUpdateFlag, 0);
            }
            
        }

        #endregion

        #region ViewArea \ SelectedArea

        public static readonly DirectProperty<MapView, RectLatLng> SelectedAreaProperty =
            AvaloniaProperty.RegisterDirect<MapView, RectLatLng>(nameof(SelectedArea), o => o.SelectedArea, (o, v) => o.SelectedArea = v);
        private RectLatLng _selectedArea;
        public RectLatLng SelectedArea
        {
            get => _selectedArea;
            set
            {
                if (SetAndRaise(SelectedAreaProperty, ref _selectedArea, value))
                {
                    InvalidateVisual();
                }
            }
        }

        public RectLatLng ViewArea
        {
            get
            {
                if (_core.Provider.Projection != null)
                {
                    var p = FromLocalToLatLng(0, 0);
                    var p2 = FromLocalToLatLng((int)Bounds.Width, (int)Bounds.Height);

                    return RectLatLng.FromLTRB(p.Longitude, p.Latitude, p2.Longitude, p2.Latitude);
                }

                return RectLatLng.Empty;
            }
        }

        #endregion

        #region MapProvider

        public static readonly DirectProperty<MapView, GMapProvider> MapProviderProperty =
            AvaloniaProperty.RegisterDirect<MapView, GMapProvider>(nameof(MapProvider), o => o.MapProvider, (o, v) => o.MapProvider = v);

        private GMapProvider _mapProvider;
        public GMapProvider MapProvider
        {
            get => _mapProvider;
            set
            {
                if (SetAndRaise(MapProviderProperty, ref _mapProvider, value))
                {
                    UpdateMapProvider();
                }
            }
        }

        

        private void UpdateMapProvider()
        {
            var viewarea = SelectedArea;

            if (viewarea != RectLatLng.Empty)
            {
                Position = new GeoPoint(viewarea.Lat - viewarea.HeightLat / 2,
                    viewarea.Lng + viewarea.WidthLng / 2,0);
            }
            else
            {
                viewarea = ViewArea;
            }

            _core.Provider = MapProvider;

            if (_core.IsStarted && _core.ZoomToArea)
            {
                // restore zoomrect as close as possible
                if (viewarea != RectLatLng.Empty && viewarea != ViewArea)
                {
                    int bestZoom = _core.GetMaxZoomToFitRect(viewarea);

                    if (bestZoom > 0 && Zoom != bestZoom)
                        Zoom = bestZoom;
                }
            }
        }

        #endregion

        #region OnWheelChanged

        public static readonly DirectProperty<MapView, bool> InvertedMouseWheelZoomingProperty =
            AvaloniaProperty.RegisterDirect<MapView, bool>(nameof(InvertedMouseWheelZooming), o => o.InvertedMouseWheelZooming, (o, v) => o.InvertedMouseWheelZooming = v);
        private bool _invertedMouseWheelZooming = false;
        public bool InvertedMouseWheelZooming
        {
            get => _invertedMouseWheelZooming;
            set => SetAndRaise(InvertedMouseWheelZoomingProperty, ref _invertedMouseWheelZooming, value);
        }


        public static readonly DirectProperty<MapView, bool> MouseWheelZoomEnabledProperty =
            AvaloniaProperty.RegisterDirect<MapView, bool>(nameof(MouseWheelZoomEnabled), o => o.MouseWheelZoomEnabled, (o, v) => o.MouseWheelZoomEnabled = v);
        private bool _mouseWheelZoomEnabled = true;
        public bool MouseWheelZoomEnabled
        {
            get => _mouseWheelZoomEnabled;
            set => SetAndRaise(MouseWheelZoomEnabledProperty, ref _mouseWheelZoomEnabled, value);
        }


        public static readonly DirectProperty<MapView, MouseWheelZoomType> MouseWheelZoomTypeProperty =
            AvaloniaProperty.RegisterDirect<MapView, MouseWheelZoomType>(nameof(MouseWheelZoomType), o => o.MouseWheelZoomType, (o, v) => o.MouseWheelZoomType = v);
        private MouseWheelZoomType _mouseWheelZoomType;
        public MouseWheelZoomType MouseWheelZoomType
        {
            get => _mouseWheelZoomType;
            set => SetAndRaise(MouseWheelZoomTypeProperty, ref _mouseWheelZoomType, value);
        }

        protected override void OnPointerWheelChanged(PointerWheelEventArgs e)
        {
            base.OnPointerWheelChanged(e);
            //TODO: && (IsMouseDirectlyOver || IgnoreMarkerOnMouseWheel)
            if (MouseWheelZoomEnabled && !_core.IsDragging)
            {
                var p = e.GetPosition(this);

                if (MapScaleTransform != null)
                {
                    p = MapScaleTransform.Inverse().Transform(p);
                }

                if (_core.MouseLastZoom.X != (int)p.X && _core.MouseLastZoom.Y != (int)p.Y)
                {
                    if (MouseWheelZoomType == MouseWheelZoomType.MousePositionAndCenter)
                    {
                        Position = FromLocalToLatLng((int)p.X, (int)p.Y);
                    }
                    else if (MouseWheelZoomType == MouseWheelZoomType.ViewCenter)
                    {
                        Position = FromLocalToLatLng((int)Bounds.Width / 2, (int)Bounds.Height / 2);
                    }
                    else if (MouseWheelZoomType == MouseWheelZoomType.MousePositionWithoutCenter)
                    {
                        Position = FromLocalToLatLng((int)p.X, (int)p.Y);
                    }

                    _core.MouseLastZoom.X = (int)p.X;
                    _core.MouseLastZoom.Y = (int)p.Y;
                }

                // set mouse position to map center
                if (MouseWheelZoomType != MouseWheelZoomType.MousePositionWithoutCenter)
                {
                    var ps = this.PointToScreen(new Point(Bounds.Width / 2, Bounds.Height / 2));
                    // Stuff.SetCursorPos(ps.X, ps.Y);
                }

                _core.MouseWheelZooming = true;

                if (e.Delta.Y > 0)
                {
                    if (!InvertedMouseWheelZooming)
                    {
                        Zoom = (int)Zoom + 1;
                    }
                    else
                    {
                        Zoom = (int)(Zoom + 0.99) - 1;
                    }
                }
                else
                {
                    if (InvertedMouseWheelZooming)
                    {
                        Zoom = (int)Zoom + 1;
                    }
                    else
                    {
                        Zoom = (int)(Zoom + 0.99) - 1;
                    }
                }

                _core.MouseWheelZooming = false;
            }
        }

        #endregion

        #region Selection

        protected override IItemContainerGenerator CreateItemContainerGenerator()
        {
            return new ItemContainerGenerator<MapViewItem>(
                this,
                ContentControl.ContentProperty,
                ContentControl.ContentTemplateProperty);
        }

        public new static readonly DirectProperty<SelectingItemsControl, IList?> SelectedItemsProperty =
            SelectingItemsControl.SelectedItemsProperty;

        public new static readonly DirectProperty<SelectingItemsControl, ISelectionModel> SelectionProperty =
            SelectingItemsControl.SelectionProperty;

        public new static readonly StyledProperty<SelectionMode> SelectionModeProperty =
            SelectingItemsControl.SelectionModeProperty;

        public new IList? SelectedItems
        {
            get => base.SelectedItems;
            set => base.SelectedItems = value;
        }

        public new SelectionMode SelectionMode
        {
            get => base.SelectionMode;
            set => base.SelectionMode = value;
        }

        public new ISelectionModel Selection
        {
            get => base.Selection;
            set => base.Selection = value;
        }

        public bool IsDragging { get; private set; }

        protected override void OnGotFocus(GotFocusEventArgs e)
        {
            base.OnGotFocus(e);

            if (e.NavigationMethod == NavigationMethod.Directional)
            {
                e.Handled = UpdateSelectionFromEventSource(
                    e.Source,
                    true,
                    e.KeyModifiers.HasAllFlags(KeyModifiers.Shift),
                    e.KeyModifiers.HasAllFlags(KeyModifiers.Control));
            }
        }

        public void SelectAll() => Selection.SelectAll();
        public void UnselectAll() => Selection.Clear();

        #endregion

        #region Pointer events

        protected override void OnPointerMoved(PointerEventArgs e)
        {
            // this is for disable drag on touch screens after dialog clicked
            if (_disablePointerActions) return;
            base.OnPointerMoved(e);

            // wpf generates to many events if mouse is over some visual
            // and OnMouseUp is fired, wtf, anyway...
            // http://greatmaps.codeplex.com/workitem/16013
            if ((e.Timestamp & UInt32.MaxValue) - _onMouseUpTimestamp < 55)
            {
                Debug.WriteLine("OnMouseMove skipped: " + ((e.Timestamp & Int32.MaxValue) - _onMouseUpTimestamp) + "ms");
                return;
            }

            if (!_core.IsDragging && !_core.MouseDown.IsEmpty)
            {
                var p = e.GetPosition(this);

                // disable drag, when click popup
                if (p.X == 0 && p.Y == 0) return;

                if (MapScaleTransform != null)
                {
                    p = MapScaleTransform.Inverse().Transform(p);
                }

                // cursor has moved beyond drag tolerance
                if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
                {
                    if (Math.Abs(p.X - _core.MouseDown.X) * 2 >= MinimumHorizontalDragDistance ||
                        Math.Abs(p.Y - _core.MouseDown.Y) * 2 >= MinimumVerticalDragDistance)
                    {
                        _core.BeginDrag(_core.MouseDown);
                    }
                }
            }

            if (_core.IsDragging && (e.KeyModifiers & KeyModifiers.Control)!=0)
            {
                _core.IsDragging = false;
            }

            if (_core.IsDragging)
            {
                if (!IsDragging)
                {
                    IsDragging = true;
                    Debug.WriteLine("IsDragging = " + IsDragging);
                    _cursorBefore = Cursor;
                    Cursor = new Cursor(StandardCursorType.SizeAll);
                    
                    //_mouse.Capture(this);
                }

                if (BoundsOfMap.HasValue && !BoundsOfMap.Value.Contains(Position))
                {
                    // ...
                }
                else
                {
                    var p = e.GetPosition(this);

                    if (MapScaleTransform != null)
                    {
                        p = MapScaleTransform.Inverse().Transform(p);
                    }

                    _core.MouseCurrent.X = (int)p.X;
                    _core.MouseCurrent.Y = (int)p.Y;
                    {
                        _core.Drag(_core.MouseCurrent);
                    }

                    if (_scaleMode != ScaleModes.Integer)
                    {
                        ForceUpdateOverlays();
                    }
                    else
                    {
                        UpdateMarkersOffset();
                    }
                }

                InvalidateVisual(true);
            }
        }

        private ulong _onMouseUpTimestamp;
        private Cursor _cursorBefore = Cursor.Default;
        private RectLatLng? BoundsOfMap { get; }

        protected override void OnPointerReleased(PointerReleasedEventArgs e)
        {
            // this is for disable drag on touch screens after dialog clicked
            if (_disablePointerActions) return;

            base.OnPointerReleased(e);

            if (_core.IsDragging)
            {
                if (IsDragging)
                {
                    _onMouseUpTimestamp = e.Timestamp & ulong.MaxValue;
                    IsDragging = false;
                    Debug.WriteLine("IsDragging = " + IsDragging);
                    Cursor = _cursorBefore;
                    //_mouse.Capture(null);
                }

                _core.EndDrag();

                if (BoundsOfMap.HasValue && !BoundsOfMap.Value.Contains(Position))
                {
                    if (_core.LastLocationInBounds.HasValue)
                    {
                        Position = _core.LastLocationInBounds.Value;
                    }
                }
            }

        }

        protected override void OnPointerPressed(PointerPressedEventArgs e)
        {
            if (IsInDialogMode)
            {
                var point = e.GetPosition(this);

                if (MapScaleTransform != null)
                {
                    point = MapScaleTransform.Inverse().Transform(point);
                }
                
                DialogTarget = FromLocalToLatLng((int)(point.X), (int)(point.Y));
                IsInDialogMode = false;
                return;
            }
            // this is for disable drag on touch screens after dialog clicked
            if (_disablePointerActions) return;

            base.OnPointerPressed(e);

            var p = e.GetPosition(this);

            if (MapScaleTransform != null)
            {
                p = MapScaleTransform.Inverse().Transform(p);
            }

            _core.MouseDown.X = (int)p.X;
            _core.MouseDown.Y = (int)p.Y;

            InvalidateVisual();


            if (e.Source is IVisual source)
            {
                var point = e.GetCurrentPoint(source);

                if (point.Properties.IsLeftButtonPressed || point.Properties.IsRightButtonPressed)
                {
                    e.Handled = UpdateSelectionFromEventSource(
                        e.Source,
                        true,
                        e.KeyModifiers.HasAllFlags(KeyModifiers.Shift),
                        e.KeyModifiers.HasAllFlags(KeyModifiers.Control),
                        point.Properties.IsRightButtonPressed);
                    if (e.Handled == false)
                    {
                        UnselectAll();
                    }
                }
            }
        }

        #endregion

        #region DialogMode

        public static readonly DirectProperty<MapView, string> DialogTextProperty =
            AvaloniaProperty.RegisterDirect<MapView, string>(nameof(DialogText), o => o.DialogText, (o, v) => o.DialogText = v);
        private string _dialogText;
        public string DialogText
        {
            get => _dialogText;
            set => SetAndRaise(DialogTextProperty, ref _dialogText, value);
        }

        public static readonly DirectProperty<MapView, GeoPoint> DialogTargetProperty =
            AvaloniaProperty.RegisterDirect<MapView, GeoPoint>(nameof(IsInDialogMode), o => o.DialogTarget, (o, v) => o.DialogTarget = v);
        private GeoPoint _dialogTarget;
        public GeoPoint DialogTarget
        {
            get => _dialogTarget;
            set => SetAndRaise(DialogTargetProperty, ref _dialogTarget, value);
        }

        public static readonly DirectProperty<MapView, bool> IsInDialogModeProperty =
            AvaloniaProperty.RegisterDirect<MapView, bool>(nameof(IsInDialogMode), o => o.IsInDialogMode, (o, v) => o.IsInDialogMode = v);

        private bool _isInDialogMode;
        public bool IsInDialogMode
        {
            get => _isInDialogMode;
            set
            {
                if (EqualityComparer<bool>.Default.Equals(_isInDialogMode, value))
                {
                    return;
                }
                _isInDialogMode = value;
                if (value)
                {
                    EnableDialogMode();
                }
                else
                {
                    DisableDialogMode();
                }
                var old = _isInDialogMode;
                _isInDialogMode = value;
                RaisePropertyChanged(IsInDialogModeProperty, old, value);
            }
        }

        private Cursor _oldCursor;
        private bool _disablePointerActions = false;
        private double _positionUpdateCiclicUpdateFlag;

        private void DisableDialogMode()
        {
            Observable.Timer(TimeSpan.FromSeconds(1)).Subscribe(_=>_disablePointerActions = false).DisposeWith(Disposable);
            foreach (var item in LogicalChildren)
            {
                if (item is IVisual visual)
                {
                    visual.Opacity = 1;
                }
            }
            Cursor = _oldCursor;
        }
        private void EnableDialogMode()
        {
            _oldCursor = Cursor;
            _disablePointerActions = true;
            Cursor = new Cursor(StandardCursorType.Hand);
            foreach (var item in LogicalChildren.Cast<MapViewItem>())
            {
                item.Opacity = item.IsSelected ? 1 : 0.5;
                item.IsSelected = false;
            }

        }

        #endregion

        public void Dispose()
        {
            Disposable.Dispose();
        }
    }
}
