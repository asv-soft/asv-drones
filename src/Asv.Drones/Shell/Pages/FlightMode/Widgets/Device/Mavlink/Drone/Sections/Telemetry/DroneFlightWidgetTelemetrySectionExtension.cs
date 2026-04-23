using System;
using Asv.Avalonia;
using Asv.Drones.Api;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using R3;

namespace Asv.Drones;

public sealed class DroneFlightWidgetTelemetrySectionExtension(IServiceProvider services)
    : IExtensionFor<IDroneFlightWidget>
{
    public void Extend(IDroneFlightWidget context, CompositeDisposable contextDispose)
    {
        var vm = services.GetRequiredKeyedService<TelemetrySectionViewModel>(
            TelemetrySectionViewModel.WidgetId
        );
        context.Sections.Add(vm);
        vm.InitWith(context.Device ?? throw new NullReferenceException());
    }
}
