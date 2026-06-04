using System;
using Asv.Avalonia;
using Microsoft.Extensions.DependencyInjection;

namespace Asv.Drones.Plane;

public class PlaneSectionExtension(IServiceProvider services) : IExtensionFor<IPlaneWidget>
{
    public void Extend(IPlaneWidget context, R3.CompositeDisposable contextDispose)
    {
        var device = context.Device ?? throw new NullReferenceException();
        var section = ActivatorUtilities.CreateInstance<PlaneSectionViewModel>(services, device);

        context.Sections.Add(section);
    }
}
