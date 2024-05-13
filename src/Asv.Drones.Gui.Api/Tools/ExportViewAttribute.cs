using System.Composition;
using Avalonia.Controls;

namespace Asv.Drones.Gui.Api
{
    /// <summary>
    /// This attribute is used to find a matching View for the ViewModel in ViewLocator
    /// </summary>
    [MetadataAttribute]
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class ExportViewAttribute : ExportAttribute, IViewMetadata
    {
        public ExportViewAttribute(Type viewModelType)
            : base(null, typeof(Control))
        {
            if (viewModelType.IsSubclassOf(typeof(Control)))
                throw new ArgumentException("ViewModelType cannot be a View type", nameof(viewModelType));
            this.ViewModelType = viewModelType;
        }

        public Type ViewModelType { get; }
    }

    public interface IViewMetadata
    {
        Type ViewModelType { get; }
    }
}