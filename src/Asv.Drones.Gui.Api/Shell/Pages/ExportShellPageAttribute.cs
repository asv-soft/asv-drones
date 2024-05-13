using System.Composition;

namespace Asv.Drones.Gui.Api
{
    /// <summary>
    /// Define this attribute to export shell page
    /// </summary>
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