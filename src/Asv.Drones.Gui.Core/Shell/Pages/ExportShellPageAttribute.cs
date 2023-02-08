using System.ComponentModel.Composition;

namespace Asv.Drones.Gui.Core
{
    [MetadataAttribute]
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class ExportShellPageAttribute : ExportAttribute
    {
        public ExportShellPageAttribute(string baseUri)
            : base(new Uri(baseUri).AbsolutePath, typeof(IShellPage))
        {

        }

    }
}