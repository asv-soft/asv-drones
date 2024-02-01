using System.ComponentModel.Composition;

namespace Asv.Drones.Gui.Core
{
    /// <summary>
    /// This attribute is used to find a matching View for the ViewModel in ViewLocator.
    /// </summary>
    /// <remarks>
    /// When applied to a class, this attribute is used to mark the class as a View that is associated with a ViewModel.
    /// The ViewLocator uses this attribute to find the corresponding View for a given ViewModel.
    /// </remarks>
    [MetadataAttribute]
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class ExportViewAttribute : ExportAttribute, IViewMetadata
    {
        /// <summary>
        /// Creates a new instance of ExportViewAttribute class with the specified view model type.
        /// </summary>
        /// <param name="viewModelType">The type of the view model.</param>
        public ExportViewAttribute(Type viewModelType)
            : base(null, ViewLocator.BaseViewType)
        {
            // TODO: need to check if developer made mistake and viewModelType is a View type
            this.ViewModelType = viewModelType;  
        }

        /// <summary>
        /// Gets the type of the view model associated with the property.
        /// </summary>
        /// <remarks>
        /// The ViewModelType property represents the type of the view model that is associated with
        /// the current property. This property is read-only and can be used to retrieve the runtime
        /// type of the view model associated with the property.
        /// </remarks>
        public Type ViewModelType { get; }
    }

    /// <summary>
    /// Represents the metadata of a view including its associated view model type.
    /// </summary>
    public interface IViewMetadata
    {
        /// <summary>
        /// Gets the type of the ViewModel associated with this property.
        /// </summary>
        /// <returns>The type of the ViewModel.</returns>
        Type ViewModelType { get; }
    }
}
