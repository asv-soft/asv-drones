using System.Composition;

namespace Asv.Drones.Api;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class ExportSetupAttribute : ExportAttribute
{
    public ExportSetupAttribute(string id)
        : base(id, typeof(ISetupSubpage)) { }
}
