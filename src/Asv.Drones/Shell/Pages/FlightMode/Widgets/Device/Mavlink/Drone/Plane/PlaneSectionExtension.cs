using System;
using Asv.Avalonia;
using Microsoft.Extensions.DependencyInjection;

namespace Asv.Drones.Plane;

public class PlaneSectionExtension(IServiceProvider services) : IExtensionFor<IPlaneWidget>
{
    public void Extend(IPlaneWidget context, R3.CompositeDisposable contextDispose)
    {
        var section = services.GetRequiredKeyedService<PlaneSectionViewModel>(
            PlaneSectionViewModel.SectionId
        );
        context.Sections.Add(section);

        section.InitWith(context.Device ?? throw new NullReferenceException());
    }
}
