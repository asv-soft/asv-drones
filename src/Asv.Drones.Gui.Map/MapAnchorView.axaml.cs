using System;
using System.Collections;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Mixins;
using Avalonia.Controls.Primitives;
using Avalonia.Media;
using Material.Icons;
using ReactiveUI;

namespace Asv.Avalonia.Map
{
    [PseudoClasses(":pressed", ":selected")]
    public class MapAnchorView : TemplatedControl,ISelectable
    {
        public MapAnchorView()
        {
            SelectableMixin.Attach<MapAnchorView>(IsSelectedProperty);
            // PressedMixin.Attach<MapAnchorView>();
            this.WhenAnyValue(_ => _.Description).Subscribe(_ => IsPopupNotEmpty = !string.IsNullOrWhiteSpace(Description) );
            this.WhenAnyValue(_ => _.CanvasBounds).Subscribe(_ => UpdateOffset(_));

        }

        private void UpdateOffset(Rect rect)
        {
            OffsetX = OffsetXType switch
            {
                OffsetXEnum.Left => 0,
                OffsetXEnum.Center => rect.X + rect.Width / 2,
                OffsetXEnum.Right => rect.X + rect.Width,
                _ => throw new ArgumentOutOfRangeException()
            };
            OffsetY = OffsetYType switch
            {
                OffsetYEnum.Top => 0,
                OffsetYEnum.Center => rect.Y + rect.Height / 2,
                OffsetYEnum.Bottom => rect.Y + rect.Height,
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        public static readonly DirectProperty<MapAnchorView, Rect> CanvasBoundsProperty =
            AvaloniaProperty.RegisterDirect<MapAnchorView, Rect>(nameof(MapAnchorView.CanvasBounds), o => o.CanvasBounds, (o, v) => o.CanvasBounds = v);

        private Rect _canvasBounds;
        public Rect CanvasBounds
        {
            get => _canvasBounds;
            set => SetAndRaise(CanvasBoundsProperty, ref _canvasBounds, value);
        }

        public static readonly DirectProperty<MapAnchorView, bool> IsPopupNotEmptyProperty =
            AvaloniaProperty.RegisterDirect<MapAnchorView, bool>(nameof(IsPopupNotEmpty), o => o.IsPopupNotEmpty, (o, v) => o.IsPopupNotEmpty = v);

        private bool _isPopupNotEmpty = true;
        public bool IsPopupNotEmpty
        {
            get => _isPopupNotEmpty;
            set => SetAndRaise(IsPopupNotEmptyProperty, ref _isPopupNotEmpty, value);
        }

        public static readonly StyledProperty<IBrush?> IconBrushProperty = AvaloniaProperty.Register<MapAnchorView, IBrush?>(nameof(IconBrush));
        public IBrush? IconBrush
        {
            get => GetValue(IconBrushProperty);
            set => SetValue(IconBrushProperty, value);
        }

        public static readonly StyledProperty<string> TitleProperty = AvaloniaProperty.Register<MapAnchorView, string>(nameof(Title),String.Empty, coerce:OnTitleChanged);

        private static string OnTitleChanged(IAvaloniaObject arg1, string arg2)
        {
            return arg2 ?? string.Empty;
        }

        public string Title
        {
            get => GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }

        public static readonly StyledProperty<MaterialIconKind> IconProperty = AvaloniaProperty.Register<MapAnchorView, MaterialIconKind>(nameof(Icon));
        public MaterialIconKind Icon
        {
            get => (MaterialIconKind)GetValue(IconProperty);
            set => SetValue(IconProperty, value);
        }

        public static readonly StyledProperty<bool> IsSelectedProperty = AvaloniaProperty.Register<MapAnchorView, bool>(nameof(IsSelected));
        public bool IsSelected
        {
            get => GetValue(IsSelectedProperty);
            set => SetValue(IsSelectedProperty, value);
        }

        public static readonly StyledProperty<double> RotateCenterXProperty = AvaloniaProperty.Register<MapAnchorView, double>(nameof(RotateCenterX));
        public double RotateCenterX
        {
            get => GetValue(RotateCenterXProperty);
            set => SetValue(RotateCenterXProperty, value);
        }

        public static readonly StyledProperty<double> RotateCenterYProperty = AvaloniaProperty.Register<MapAnchorView, double>(nameof(RotateCenterY));
        public double RotateCenterY
        {
            get => GetValue(RotateCenterYProperty);
            set => SetValue(RotateCenterYProperty, value);
        }

        public static readonly StyledProperty<double> RotateAngleProperty = AvaloniaProperty.Register<MapAnchorView, double>(nameof(RotateAngle), defaultValue:300);
        public double RotateAngle
        {
            get => GetValue(RotateAngleProperty);
            set => SetValue(RotateAngleProperty, value);
        }

        public static readonly StyledProperty<double> SizeProperty = AvaloniaProperty.Register<MapAnchorView, double>(nameof(Size), defaultValue: 30);
        public double Size
        {
            get => GetValue(SizeProperty);
            set => SetValue(SizeProperty, value);
        }

        public static readonly DirectProperty<MapAnchorView, string> DescriptionProperty =
            AvaloniaProperty.RegisterDirect<MapAnchorView, string>(nameof(MapAnchorView.Description), o => o.Description, (o, v) => o.Description = v);

        private string _description;
        public string Description
        {
            get => _description;
            set => SetAndRaise(DescriptionProperty, ref _description, value);
        }

       
        public static readonly DirectProperty<MapAnchorView, IEnumerable> ActionsProperty =
            AvaloniaProperty.RegisterDirect<MapAnchorView, IEnumerable>(nameof(Actions), o => o.Actions, (o, v) => o.Actions = v);

        private IEnumerable _actions;
        

        public IEnumerable Actions
        {
            get => _actions;
            set => SetAndRaise(ActionsProperty, ref _actions, value);
        }

        public static readonly StyledProperty<double> OffsetXProperty = AvaloniaProperty.Register<MapAnchorView, double>(nameof(OffsetX));
        public double OffsetX
        {
            get => GetValue(OffsetXProperty);
            set => SetValue(OffsetXProperty, value);
        }
        public static readonly StyledProperty<double> OffsetYProperty = AvaloniaProperty.Register<MapAnchorView, double>(nameof(OffsetY));
        public double OffsetY
        {
            get => GetValue(OffsetYProperty);
            set => SetValue(OffsetYProperty, value);
        }

        public static readonly StyledProperty<OffsetXEnum> OffsetXTypeProperty = AvaloniaProperty.Register<MapAnchorView, OffsetXEnum>(nameof(OffsetY));
        public OffsetXEnum OffsetXType
        {
            get => GetValue(OffsetXTypeProperty);
            set => SetValue(OffsetXTypeProperty, value);
        }
        public static readonly StyledProperty<OffsetYEnum> OffsetYTypeProperty = AvaloniaProperty.Register<MapAnchorView, OffsetYEnum>(nameof(OffsetY));
        public OffsetYEnum OffsetYType
        {
            get => GetValue(OffsetYTypeProperty);
            set => SetValue(OffsetYTypeProperty, value);
        }
    }
}
