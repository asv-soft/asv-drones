using System;
using Asv.Avalonia;
using Asv.Common;
using Asv.Drones.Api;
using Microsoft.Extensions.DependencyInjection;
using R3;

namespace Asv.Drones;

public sealed class DroneFlightWidgetTelemetrySectionExtension(
    IServiceProvider services,
    ILayoutService layoutService
) : IExtensionFor<IDroneFlightWidget>
{
    public void Extend(IDroneFlightWidget context, CompositeDisposable contextDispose)
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
