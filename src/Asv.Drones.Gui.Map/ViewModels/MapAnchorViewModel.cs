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
    /// <summary>
    /// Enumeration for X-axis offset values.
    /// </summary>
    public enum OffsetXEnum
    {
        /// <summary>
        /// Represents the offset value for the X-axis of an element to be positioned on the left side.
        /// </summary>
        Left,

        /// <summary>
        /// Enumeration for X-axis offset values.
        /// </summary>
        Center,

        /// <summary>
        /// Enumeration for X-axis offset values.
        /// </summary>
        Right
    }

    /// <summary>
    /// Represents the possible vertical offset positions for an
    public enum OffsetYEnum
    {
        /// <summary>
        /// Represents the vertical offset position of an element.
        /// </summary>
        Top,

        /// <summary>
        /// Enum member representing the vertical offset position from the center of the target element.
        /// </summary>
        Center,

        /// <summary>
        /// Represents the vertical alignment of an element relative to its parent or container.
        /// </summary>
        Bottom
    }


    /// <summary>
    /// Represents a view model for a map anchor on the map.
    /// </summary>
    public interface IMapAnchorViewModel
    {
        /// describe the purpose and behavior of the property. The code snippet also correctly declares the property
        bool IsEditable { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the object is in edit mode.
        /// </summary>
        /// <value>
        /// <c>true</c> if the object is in edit mode; otherwise, <c>false</c>.
        /// </value>
        bool IsInEditMode { get; set; }

        /// <summary>
        /// Gets or sets the Z-order of the property.
        /// </summary>
        /// <value>
        /// An integer value representing the Z-order.
        /// </value>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// Thrown when the value set is less than zero.
        /// </exception>
        int ZOrder { get; set; }

        /// <summary>
        /// Gets or sets the horizontal offset for an element.
        /// </summary>
        /// <value>
        /// An <see cref="OffsetXEnum"/> value representing the horizontal offset.
        /// </value>
        OffsetXEnum OffsetX { get; set; }

        /// <summary>
        /// Gets or sets the vertical offset for an element.
        /// </summary>
        /// <value>
        /// The offset along the Y-axis.
        /// </value>
        OffsetYEnum OffsetY { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the object is selected.
        /// </summary>
        /// <value>
        /// <c>true</c> if the object is selected; otherwise, <c>false</c>.
        /// </value>
        bool IsSelected { get; set; }

        /// <summary>
        /// Gets or sets the visibility of the item.
        /// </summary>
        /// <value>
        /// <c>true</c> if the item is visible; otherwise, <c>false</c>.
        /// </value>
        bool IsVisible { get; set; }

        /// <summary>
        /// Gets or sets the location of an object using coordinates in a GeoPoint format.
        /// </summary
        GeoPoint Location { get; set; }

        /// <summary>
        /// Gets or sets the icon used in the material design.
        /// </summary>
        /// <value>
        /// The material design icon.
        /// </value>
        MaterialIconKind Icon { get; set; }

        /// <summary>
        /// Gets or sets the X coordinate of the center point of rotation.
        /// </summary>
        /// <value>The X coordinate of the center point of rotation.</value>
        double RotateCenterX { get; set; }

        /// <summary>
        /// The Y-coordinate of the center point used for rotation.
        /// </summary>
        /// <remarks>
        /// This property allows you to specify the Y-coordinate of the center point
        /// around which rotation is performed. It represents the vertical position
        /// of the rotation center.
        /// </remarks>
        /// <value>
        /// A double value representing the Y-coordinate of the center point used
        /// for rotation.
        /// </value>
        /// <seealso cref="RotateCenterX"/>
        double RotateCenterY { get; set; }

        /// <summary>
        /// Gets or sets the brush used to render the icon.
        /// </summary>
        /// <value>
        /// The brush used to render the icon.
        /// </value>
        IBrush IconBrush { get; set; }

        /// <summary>
        /// Gets or sets the rotation angle in degrees.
        /// </summary>
        /// <value>
        double RotateAngle { get; set; }

        /// <summary>
        /// Gets or sets the title of the property.
        /// </summary>
        /// <value>
        /// The title.
        /// </value>
        string Title { get; set; }

        /// <summary>
        /// Gets or sets the description of the property.
        /// </summary>
        /// <value>
        /// The description of the property.
        /// </value>
        string Description { get; set; }

        /// <summary>
        /// Gets or sets the size of an object.
        /// </summary>
        /// <remarks>
        /// The size represents the physical dimension of an object.
        /// </remarks>
        /// <value>
        /// The size of the object.
        /// </value>
        double Size { get; set; }

        /// <summary>
        /// Gets the read-only ObservableCollection of MapAnchorActionViewModel objects.
        /// </summary>
        /// <remarks>
        /// This property provides access to a collection of MapAnchorActionViewModel objects that represent
        /// the actions associated with a map anchor.
        /// The collection is read-only, meaning that the caller can only read the elements.
        /// </remarks>
        /// <value>
        /// The read-only ObservableCollection of MapAnchorActionViewModel objects.
        /// </value>
        ReadOnlyObservableCollection<MapAnchorActionViewModel> Actions { get; }

        /// <summary>
        /// Gets the read-only collection of GeoPoint objects representing the path.
        /// </summary>
        /// <value>
        /// The read-only collection of GeoPoint objects representing the path.
        /// </value>
        ReadOnlyObservableCollection<GeoPoint> Path { get; }

        /// <summary>
        /// Gets or sets the brush used to fill the object.
        /// </summary>
        /// <remarks>
        /// The Fill property represents the brush used to fill an object, such as a shape or a text
        IBrush Fill { get; set; }

        /// <summary>
        /// Gets or sets the brush used for the stroke.
        /// </summary>
        /// <value>
        /// The brush used for the stroke.
        /// </value>
        IBrush Stroke { get; set; }

        /// property `StrokeThickness` in C#
        double StrokeThickness { get; set; }

        /// <summary>
        /// Gets or sets the stroke dash array for the shape's stroke.
        /// </summary>
        /// <value>
        /// The stroke dash array for the shape's stroke.
        /// </value>
        /// <remarks>
        /// The stroke dash array determines the pattern of dashes and gaps used to draw the stroke outline of a shape.
        /// The array is specified in terms of the lengths of alternating dashes and gaps in logical units.
        /// The default value is an empty array, indicating that the shape has a solid stroke.
        /// The specified values in the array are alternated between the on and off dashes.
        /// When the array contains an odd number of elements, the values are repeated to form an even number of elements.
        /// Zero-length dashes are not drawn.
        /// Negative values, including Infinite, NaN, and NegativeInfinity, are invalid values for the dash array.
        /// </remarks>
        /// <exception cref="ArgumentException">
        /// The specified dash array is invalid.
        /// </exception>
        AvaloniaList<double> StrokeDashArray { get; set; }

        /// <summary>
        /// Gets or sets the opacity of a path.
        /// </summary>
        /// <remarks>
        /// The opacity value is a double between 0.0 and 1.0, where 0.0 represents
        /// completely transparent and 1.0 represents completely opaque.
        /// </remarks>
        double PathOpacity { get; set; }
    }

    /// <summary>
    /// This class represents a view model for a map anchor.
    /// </summary>
    public class MapAnchorViewModel : ReactiveObject, IMapAnchorViewModel
    {
        /// <summary>
        /// This class represents a view model for a map anchor.
        /// </summary>
        public MapAnchorViewModel()
        {
            if (Design.IsDesignMode)
            {
                Actions = new ReadOnlyObservableCollection<MapAnchorActionViewModel>(
                    new ObservableCollection<MapAnchorActionViewModel>
                    {
                        new() { Title = "Action1", Icon = MaterialIconKind.Run },
                        new() { Title = "Action2", Icon = MaterialIconKind.Run }
                    });
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the property is editable.
        /// </summary>
        /// <remarks>
        /// The <see cref="IsEditable"/> property indicates whether the property can be edited by the user.
        /// If the value is true, the property can be edited; otherwise, it is read-only.
        /// </remarks>
        /// <value>
        /// <c>true</c> if the property is editable; otherwise, <c>false</c>.
        /// </value>
        [Reactive]
        public bool IsEditable { get; set; } = false;

        /// <summary>
        /// Gets or sets the Z-order of the property.
        /// </summary>
        /// <remarks>
        /// The Z-order determines the stacking order of visual elements within a container.
        /// Elements with a higher Z-order appear on top of elements with a lower Z-order.
        /// </remarks>
        [Reactive]
        public int ZOrder { get; set; }

        /// <summary>
        /// Represents the X-axis offset.
        /// </summary>
        [Reactive]
        public OffsetXEnum OffsetX { get; set; }

        /// <summary>
        /// Gets or sets the offset Y value.
        /// </summary>
        /// <remarks>
        /// The OffsetY property represents the Y coordinate offset from the original position.
        /// </remarks>
        [Reactive]
        public OffsetYEnum OffsetY { get; set; }

        /// <summary>
        /// Gets or sets the value indicating whether the item is selected.
        /// </summary>
        /// <remarks>
        /// This property is marked as reactive, which means that changes to it will automatically trigger any subscribed events.
        /// </remarks>
        [Reactive]
        public bool IsSelected { get; set; }

        /// <summary>
        /// Gets or sets the visibility status of the property.
        /// </summary>
        /// <remarks>
        /// The visibility status determines
        [Reactive] public bool IsVisible { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the object is in edit mode.
        /// </summary>
        /// <remarks>
        /// When the property is true, it indicates that the object is in edit mode. During this mode,
        /// certain operations or changes to the object may be restricted or locked.
        /// </remarks>
        /// <value>
        /// <c>true</c> if the object is in edit mode; otherwise, <c>false</c>.
        /// </value>
        [Reactive]
        public bool IsInEditMode { get; set; }

        /// <summary>
        /// Gets or sets the location of a geo point.
        /// </summary>
        /// <remarks>
        /// The location property is decorated with the [Reactive] attribute, indicating that it is observable or reactive.
        /// </remarks>
        [Reactive]
        public GeoPoint Location { get; set; }

        /// <summary>
        /// Gets or sets the material icon kind for the Icon property.
        /// </summary>
        /// <remarks>
        /// This property should be used to specify the icon kind for the control.
        /// </remarks>
        /// <value>
        /// The material icon kind.
        /// </value>
        [Reactive]
        public MaterialIconKind Icon { get; set; }

        /// <summary>
        /// Gets or sets the X coordinate of the rotation center.
        /// </summary>
        [Reactive]
        public double RotateCenterX { get; set; }

        /// <summary>
        /// Gets or sets the Y coordinate of the rotation center.
        /// </summary>
        /// <remarks>
        /// This property is used for specifying the Y coordinate of the rotation center.
        /// </remarks>
        /// <value>
        /// The Y coordinate of the rotation center.
        /// </value>
        [Reactive]
        public double RotateCenterY { get; set; }

        /// <summary>
        /// Gets or sets the brush used to render the icon for the property.
        /// </summary>
        /// <value>
        /// An implementation of the <see cref="IBrush"/> interface that represents the brush used to render the icon.
        /// </value>
        [Reactive]
        public IBrush IconBrush { get; set; }

        /// <summary>
        /// Gets or sets the rotation angle.
        /// </summary>
        /// <remarks>
        /// The RotateAngle property is used to specify the rotation angle of an object.
        /// A positive value rotates the object in a clockwise direction, and a negative
        /// value rotates the object in a counterclockwise direction. The rotation angle
        /// value is specified in degrees.
        /// </remarks>
        /// <value>
        /// The rotation angle in degrees.
        /// </value>
        [Reactive]
        public double RotateAngle { get; set; }

        /// <summary>
        /// Gets or sets the title of the object.
        /// </summary>
        /// <value>
        /// A <see cref="string"/> representing the title of the object.
        /// </value>
        [Reactive]
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the description of the property.
        /// </summary>
        /// <remarks>
        /// This property represents the description of an object. It is typically used to provide additional information or context about the object.
        /// </remarks>
        [Reactive]
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the size of the property.
        /// </summary>
        /// <value>
        /// The size of the property.
        /// </value>
        [Reactive]
        public double Size { get; set; } = 32;

        /// <summary>
        /// Gets or sets a value indicating whether the property is filled.
        /// </summary>
        /// <remarks>
        /// This property is used to determine if the associated object is filled or not.
        /// When <c>true</c>, it indicates that the object is filled, while <c>false</c> indicates that the object is not filled.
        /// </remarks>
        /// <value>
        /// <c>true</c> if the object is filled; otherwise, <c>false</c>.
        /// </value>
        [Reactive] public bool IsFilled { get; set; }

        /// <summary>
        /// Gets the read-only collection of MapAnchorActionViewModel actions.
        /// </summary>
        /// <value>
        /// The read-only collection of MapAnchorActionViewModel actions.
        /// </value>
        public virtual ReadOnlyObservableCollection<MapAnchorActionViewModel> Actions { get; }

        /// <summary>
        /// Gets the collection of GeoPoints that define the path.
        /// </summary>
        /// <remarks>
        /// The Path property is a read-only collection of GeoPoints that represent the path.
        /// </remarks>
        /// <value>
        /// A ReadOnlyObservableCollection of GeoPoint objects.
        /// </value>
        public virtual ReadOnlyObservableCollection<GeoPoint> Path { get; }

        /// <summary>
        /// Gets or sets the fill brush for the object.
        /// </summary>
        [Reactive]
        public IBrush Fill { get; set; }

        /// <summary>
        /// Gets or sets the brush used for the stroke.
        /// The stroke color defines the color of the outline of a shape or line.
        /// </summary>
        /// <value>
        /// The brush used for the stroke.
        /// </value>
        /// <remarks>
        /// By default, the stroke is set to Brushes.Blue.
        /// This property is reactive, meaning changes to the brush will automatically update the UI.
        /// </remarks>
        [Reactive]
        public IBrush Stroke { get; set; } = Brushes.Blue;

        /// <summary>
        /// Gets or sets the thickness of the stroke used to outline the shape.
        /// The default value is 3.
        /// </summary>
        /// <remarks>
        /// This property is reactive, meaning that changes to this property will automatically
        [Reactive]
        public double StrokeThickness { get; set; } = 3;

        /// <summary>
        /// Gets or sets the collection of values that specify the pattern of dashes and gaps used to stroke lines.
        /// </summary>
        [Reactive]
        public AvaloniaList<double> StrokeDashArray { get; set; }

        /// <summary>
        /// Gets or sets the opacity of the path.
        /// </summary>
        [Reactive]
        public double PathOpacity { get; set; } = 0.6;
        
        
    }
}