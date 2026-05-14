using System;
using Asv.Avalonia;
using Asv.Common;
using Microsoft.Extensions.DependencyInjection;
using R3;

namespace Asv.Drones.Plane;

public sealed class PlaneFlightWidgetTelemetrySectionExtension(
    IServiceProvider services,
    ILayoutService layoutService
) : IExtensionFor<IPlaneWidget>
{
    public void Extend(IPlaneWidget context, CompositeDisposable contextDispose)
    {
        var vm = services.GetRequiredKeyedService<TelemetrySectionViewModel>(
            TelemetrySectionViewModel.SectionId
        );
        var device = context.Device ?? throw new NullReferenceException();

        context.Sections.Add(vm);
        vm.InitWith(device);
        vm.RequestLoadLayout(layoutService).SafeFireAndForget();
    }
}
