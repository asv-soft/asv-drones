using System.ComponentModel.Composition;
using Avalonia.Controls;

namespace Asv.Drones.Gui.Core
{
    /// <summary>
    /// This attribute is used to find a matching View for the ViewModel in ViewLocator
    /// </summary>
    [MetadataAttribute]
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class ExportViewAttribute : ExportAttribute, IViewMetadata
    {
        public ExportViewAttribute(Type viewModelType)
            : base(null, ViewLocator.BaseViewType)
        {
            // TODO: need to check if developer made mistake and viewModelType is a View type
            this.ViewModelType = viewModelType;  
        }

        public Type ViewModelType { get; }
    }

    public interface IViewMetadata
    {
        Type ViewModelType { get; }
    }
}
