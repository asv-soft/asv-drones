using System.ComponentModel.Composition;
using Avalonia.Controls;

namespace Asv.Drones.Gui.Core
{
    [MetadataAttribute]
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class ExportViewAttribute : ExportAttribute, IViewMetadata
    {
        public ExportViewAttribute(Type viewModelType)
            : base(null, typeof(IControl))
        {
            this.ViewModelType = viewModelType;  
        }

        public Type ViewModelType { get; }
    }

    public interface IViewMetadata
    {
        Type ViewModelType { get; }
    }
}
